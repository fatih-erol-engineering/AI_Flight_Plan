using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;   

// [ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class BSplineSegment
    : SelectableMonoBehaviour
{
    [Header("4 Control Points (Transforms)")]
    public Waypoint startPoint { get; private set; }
    public ControlPoint controlPoint1;
    public ControlPoint controlPoint2;
    public Waypoint endPoint { get; private set; }
    public float initialControlPointDistance = 3f;
    public Vector3[] trajPosList { get; private set; }  
    public float[] distanceList { get; private set; }   

    [Header("Sampling")]
    [Range(4, 256)] public int samples = 32;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public float gizmoSize = 0.04f;
    

    [Header("Restricted Areas")]
    public RestrictedArea[] restrictedAreas;
    public bool isCollide{ get; private set; } = false;



    public TrajectoryPoint[] trajectoryPoints { get; private set; }

    public LineRenderer lr;

    [Header("Time")]
    public TimeGame startTime;
    public TimeGame endTime;

    public GameObject controlPointPrefab;
    public Theme theme;


    // Clamped uniform knot vector for n=3 (4 pts), degree p=3:
    // U = [0,0,0,0, 1,1,1,1]
    private readonly float[] U = new float[] { 0, 0, 0, 0, 1, 1, 1, 1 };

    private bool collisionFlagTick = false;
    private bool prev_collisionFlagTick = false;
    private bool collisionFlagEnd = false;
    private bool collisionFlagStart = false;

    private bool same_collisionFlagTick = false;
    private bool same_prev_collisionFlagTick = false;
    private bool same_collisionFlagEnd = false;
    private bool same_collisionFlagStart = false;
    

    void OnEnable()
    {
        lr = GetComponent<LineRenderer>();
        lr.colorGradient = new Gradient();
        var shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        var mat = new Material(shader);
        lr.sharedMaterial = mat;
        UpdateCurve();
    }

    void OnValidate()
    {
        lr = GetComponent<LineRenderer>();
        UpdateCurve();
    }

    public void SetStartAndEndTime(TimeGame startTime_, TimeGame endTime_)
    {
        startTime = startTime_;
        endTime = endTime_;
    }
    public void SetStartAndEndWaypoints(Waypoint startWP , Waypoint endWP)
    {
        startPoint = startWP;
        endPoint = endWP;

        SetStartAndEndTime(startWP.time, endWP.time);
    }
    void Update()
    {
        UpdateCurve();
        CheckCollision();
    }
    public void CreateControlPoints()
    {
        
        if ((startPoint != null) && (endPoint != null))
        {            
            GameObject controlPoint1GO = Instantiate(controlPointPrefab,transform, true);            
            controlPoint1GO.name = this.name + " Control Point 1";                        
            controlPoint1GO.transform.position = startPoint.transform.position + (endPoint.transform.position - startPoint.transform.position).normalized * initialControlPointDistance;            
            
            GameObject controlPoint2GO = Instantiate(controlPointPrefab, transform, true);
            controlPoint2GO.transform.parent= transform;
            controlPoint2GO.name = this.name + " Control Point 2";                        
            controlPoint2GO.transform.position = endPoint.transform.position + (startPoint.transform.position - endPoint.transform.position).normalized * initialControlPointDistance;

            controlPoint1 = controlPoint1GO.GetComponent<ControlPoint>();
            controlPoint2 = controlPoint2GO.GetComponent<ControlPoint>();            
        }
    }
    public void UpdateCurve()
    {
        if (!lr) return;
        if (!(startPoint && controlPoint1 && controlPoint2 && endPoint)) return;

        Vector3[] P = new Vector3[4] { startPoint.transform.position, controlPoint1.transform.position, controlPoint2.transform.position, endPoint.transform.position };

        lr.positionCount = samples;
        trajPosList = new Vector3[samples];
        distanceList = new float[samples - 1];
        trajectoryPoints = new TrajectoryPoint[samples];
        float cumulativeDistance = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (samples == 1) ? 0f : (float)i / (samples - 1); // [0,1]
            Vector3 C = BSplinePointDegree3(P, t);
            trajPosList[i] = C;
            if (i>1)
            {
                distanceList[i-1] = cumulativeDistance + (trajPosList[i] - trajPosList[i - 1]).magnitude;
                cumulativeDistance = distanceList[i-1];
            }

            lr.SetPosition(i, C);
            trajectoryPoints[i] = new TrajectoryPoint(C, Mathf.Lerp(startTime.second, endTime.second, t));
        }

    }
    public void UpdateColorWithTotalTime(TimeGame totalStartTime,TimeGame totalEndTime)
    {

        // Alpha'yı koruyarak HDR parlaklık uygula

        float startVal = Mathf.Lerp(0,1, (startTime.second - totalStartTime.second) / (totalEndTime.second - totalStartTime.second));
        Color startColor = Color.Lerp(theme.startColor, theme.endColor, startVal);            

        float endVal = Mathf.Lerp(0,1, (endTime.second - totalStartTime.second) / (totalEndTime.second - totalStartTime.second));
        Color endColor = Color.Lerp(theme.startColor, theme.endColor, endVal);      
        Color c0 = startColor;
        Color c1 = endColor;

        if (!lr) return;

        int n = 2;
        if (n < 2) n = 2;

        var cKeys = new GradientColorKey[n];
        var aKeys = new GradientAlphaKey[n];

        // HDR yoğunluk: RGB’yi çarpıyoruz, alpha’yı ayrı yönetiyoruz
        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 1f : (float)i / (n - 1);
            Color c = Color.Lerp(startColor, endColor, t);
            c = new Color(c.r, c.g, c.b, 1f);

            float a = Mathf.Lerp(startColor.a, endColor.a, t);
            
            cKeys[i] = new GradientColorKey(c, t);
            aKeys[i] = new GradientAlphaKey(a, t);
        }

        var g = new Gradient { mode = GradientMode.Blend };
        try
        {
            g.SetKeys(cKeys, aKeys);            
        }
        catch (System.Exception)
        {            
            throw;
        }
        lr.colorGradient = g;

    }
    
    public bool[] CheckCollision()
    {
        /// <summary>
        /// Checks collision for restricted area objects.
        /// </summary>
        Vector3[] posList = new Vector3[restrictedAreas.Length];
        float[] radList = new float[restrictedAreas.Length];
        bool[] collisionFlag = new bool[restrictedAreas.Length];
        // // // // for (int i = 0; i < restrictedAreas.Length; i++) 
        // // // // {
        // // // //     if (restrictedAreas[i].gameObject.activeSelf)
        // // // //     {            
        // // // //         posList[i] = restrictedAreas[i].position;
        // // // //         radList[i] = restrictedAreas[i].localScale.x/2;
        // // // //         collisionFlag[i] = CheckCollision(posList[i], radList[i]);
        // // // //     }
        // // // // }
        //if (collisionFlag.Contains(true))
        //{
        //    lr.sharedMaterial.color = Color.red;
        //    isCollide = true;
        //}
        //else
        //{
        //    lr.sharedMaterial.color = Color.green;
        //    isCollide = false;
        //};


        return collisionFlag;
    }
    public bool[] CheckCollision(Vector3[] P, float[] radious)
    {
        bool[] collisionFlag = new bool[P.Length];
        for (int j = 0; j < P.Length; j++)
        {
        collisionFlag[j] = CheckCollision(P[j], radious[j]);
        }                               
        return collisionFlag;
    }
    public bool CheckCollision(Vector3 P, float radious)
    {
        bool collisionFlag = false;
        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 pi = lr.GetPosition(i);
            float lrWidth = lr.startWidth;
            collisionFlag = Vector3.Distance(pi, P) < radious + lrWidth/2;
            if (collisionFlag)
            {
                break;
            }
        }
        return collisionFlag;
    }


    Vector3 BSplinePointDegree3(Vector3[] P, float t)
    {
        // p=3, i=0..3
        float N0 = Nip(0, 3, t, U);
        float N1 = Nip(1, 3, t, U);
        float N2 = Nip(2, 3, t, U);
        float N3 = Nip(3, 3, t, U);

        return N0 * P[0] + N1 * P[1] + N2 * P[2] + N3 * P[3];
    }


    float Nip(int i, int p, float t, float[] knots)
    {
        if (p == 0)
        {
            // N_{i,0}(t) = 1 if U_i <= t < U_{i+1} (sa� u�ta kapan�� i�in t==1 �zel durumu da d���n�l�r)
            if ((knots[i] <= t && t < knots[i + 1]) || (t == 1f && Mathf.Approximately(knots[i + 1], 1f)))
                return 1f;
            return 0f;
        }

        float leftDen = knots[i + p] - knots[i];
        float rightDen = knots[i + p + 1] - knots[i + 1];

        float left = 0f, right = 0f;

        if (leftDen > 1e-8f)
            left = (t - knots[i]) / leftDen * Nip(i, p - 1, t, knots);

        if (rightDen > 1e-8f)
            right = (knots[i + p + 1] - t) / rightDen * Nip(i + 1, p - 1, t, knots);

        return left + right;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.white;

        if (startPoint) Gizmos.DrawSphere(startPoint.transform.position, gizmoSize);
        if (controlPoint1) Gizmos.DrawSphere(controlPoint1.transform.position, gizmoSize);
        if (controlPoint2) Gizmos.DrawSphere(controlPoint2.transform.position, gizmoSize);
        if (endPoint) Gizmos.DrawSphere(endPoint.transform.position, gizmoSize);

        // Kontrol poligonu
        if (startPoint && controlPoint1) Gizmos.DrawLine(startPoint.transform.position, controlPoint1.transform.position);
        if (controlPoint1 && controlPoint2) Gizmos.DrawLine(controlPoint1.transform.position, controlPoint2.transform.position);
        if (controlPoint2 && endPoint) Gizmos.DrawLine(controlPoint2.transform.position, endPoint.transform.position);
    }
    public List<CollisionInfo> CheckCollisionWithAnotherSegment(BSplineSegment otherSegment, float geometricCollisionThreshold_m, float timeCollision_s)
    {
        TrajectoryPoint[] traj1Points = trajectoryPoints;
        TrajectoryPoint[] traj2Points = otherSegment.trajectoryPoints;
        List<CollisionInfo> innerCollisionInfoList = new List<CollisionInfo>();
        List<CollisionInfo> collisionInfoList = new List<CollisionInfo>();

        Vector3 sumOfPositions = new Vector3(0, 0, 0);
        float sumOfTimes = 0f;
        int collisionCount = 0;
        Vector3 meanPosition = new Vector3(0, 0, 0);
        float meanTime_s = 0f;
        prev_collisionFlagTick = false;
        collisionFlagTick = false;

        foreach (var traj1Point in traj1Points)
        {
            foreach (var traj2Point in traj2Points)
            {
                if (Mathf.Abs(traj1Point.time - traj2Point.time) < timeCollision_s)
                {
                    if ((Vector3.Distance(traj2Point.position, traj1Point.position) < geometricCollisionThreshold_m))
                    {
                        collisionFlagTick = true;
                    }
                    else
                    {
                        collisionFlagTick = false;
                    }
                }
                collisionFlagStart = ((prev_collisionFlagTick == false) && collisionFlagTick == true);
                collisionFlagEnd = ((prev_collisionFlagTick == true) && collisionFlagTick == false);

                if (collisionFlagStart)
                {
                    sumOfPositions = new Vector3(0, 0, 0);
                    sumOfTimes = 0f;
                    collisionCount = 0;
                    meanPosition = new Vector3(0, 0, 0);
                    meanTime_s = 0f;
                }

                if (collisionFlagEnd)
                {
                    meanPosition = sumOfPositions / (float)collisionCount;
                    meanTime_s = sumOfTimes / (float)collisionCount;
                    innerCollisionInfoList.Add(new CollisionInfo
                    {
                        point = (meanPosition),
                        time = meanTime_s,
                    });
                }

                if (collisionFlagTick)
                {
                    sumOfPositions += (traj2Point.position + traj1Point.position) / 2;
                    sumOfTimes += (traj2Point.time + traj1Point.time) / 2;
                    collisionCount++;
                }

                prev_collisionFlagTick = collisionFlagTick;
            }
        }


        sumOfPositions = new Vector3(0, 0, 0);
        sumOfTimes = 0f;
        collisionCount = 0;
        meanPosition = new Vector3(0, 0, 0);
        meanTime_s = 0f;        
        same_prev_collisionFlagTick = false;
        same_collisionFlagTick = false;
        for (int i = 0; i < innerCollisionInfoList.Count; i++)
        {
            var collision1 = innerCollisionInfoList[i];
            var collision2 = new CollisionInfo();
            collision2.point = new Vector3(0, 0, 0);
            collision2.time = 0f;
            if (i != innerCollisionInfoList.Count - 1)
            {
                collision2 = innerCollisionInfoList[i + 1];

                float dist = Vector3.Distance(collision1.point, collision2.point);
                float timeDiff = Mathf.Abs(collision1.time - collision2.time);

                if (dist >= geometricCollisionThreshold_m || timeDiff >= timeCollision_s)
                {
                    same_collisionFlagTick = false;
                }
                else
                {
                    same_collisionFlagTick = true;
                }
            }
            else
            {
                same_collisionFlagTick = false;
            }

            same_collisionFlagStart = ((same_prev_collisionFlagTick == false) && same_collisionFlagTick == true);
            same_collisionFlagEnd = ((same_prev_collisionFlagTick == true) && same_collisionFlagTick == false);

            if (same_collisionFlagStart)
            {
                sumOfPositions = collision1.point;
                sumOfTimes = collision1.time;
                collisionCount = 1;
            }

            if (same_collisionFlagEnd)
            {
                meanPosition = sumOfPositions / (float)collisionCount;
                meanTime_s = sumOfTimes / (float)collisionCount;
                collisionInfoList.Add(new CollisionInfo
                {
                    point = (meanPosition),
                    time = meanTime_s,
                });
            }

            if (same_collisionFlagTick)
            {
                sumOfPositions +=  collision2.point;
                sumOfTimes +=  collision2.time;
                collisionCount++;
            }

            same_prev_collisionFlagTick = same_collisionFlagTick;



        }
        




        return collisionInfoList;
    }
    
}

public class TrajectoryPoint
{
    public Vector3 position;
    public float time;
    public TrajectoryPoint(Vector3 pos, float t)
    {
        position = pos;
        time = t;
    }    
}

