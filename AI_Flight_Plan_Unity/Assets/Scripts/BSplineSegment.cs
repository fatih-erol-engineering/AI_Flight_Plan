using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

/// <summary>
/// 4 kontrol noktasý ile derece-3 clamped B-spline çizer (tek segment).
/// LineRenderer üzerinden örnekleyip gösterir.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class BSplineSegment
    : MonoBehaviour
{
    [Header("4 Control Points (Transforms)")]
    public Transform startPoint;
    public Transform controlPoint1;
    public Transform controlPoint2;
    public Transform endPoint;

    [Header("Sampling")]
    [Range(4, 256)] public int samples = 32;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public float gizmoSize = 0.04f;

    

    [Header("Restricted Areas")]
    public Transform[] restrictedAreas;
    public bool isCollide{ get; private set; } = false;
    private LineRenderer lr;
    

    // Clamped uniform knot vector for n=3 (4 pts), degree p=3:
    // U = [0,0,0,0, 1,1,1,1]
    private readonly float[] U = new float[] { 0, 0, 0, 0, 1, 1, 1, 1 };

    void OnEnable()
    {
        lr = GetComponent<LineRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        var mat = new Material(shader);
        lr.sharedMaterial = mat;
        UpdateCurve();
    }

    void OnValidate()
    {
        lr = GetComponent<LineRenderer>();
        UpdateCurve();
    }

    void Update()
    {
        // Editörde sürüklerken de canlý güncelle
        UpdateCurve();
        CheckCollision();
    }
    public void CreateControlPoints()
    {
        if ((startPoint != null) && (endPoint != null))
        {
            GameObject controlPoint1GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlPoint1GO.name = "Control Point 1";
            controlPoint1GO.transform.SetParent(transform, false);
            controlPoint1GO.transform.localPosition = Vector3.Lerp(startPoint.localPosition, endPoint.localPosition, 0.1f);

            GameObject controlPoint2GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            controlPoint2GO.name = "Control Point 2";
            controlPoint2GO.transform.SetParent(transform, false);
            controlPoint2GO.transform.localPosition = Vector3.Lerp(startPoint.localPosition, endPoint.localPosition, 0.9f);

            controlPoint1 = controlPoint1GO.transform;
            controlPoint2 = controlPoint2GO.transform;

            
        }
    }
    void UpdateCurve()
    {
        if (!lr) return;
        if (!(startPoint && controlPoint1 && controlPoint2 && endPoint)) return;

        Vector3[] P = new Vector3[4] { startPoint.position, controlPoint1.position, controlPoint2.position, endPoint.position };
        
        lr.positionCount = samples;
        for (int i = 0; i < samples; i++)
        {
            float t = (samples == 1) ? 0f : (float)i / (samples - 1); // [0,1]
            Vector3 C = BSplinePointDegree3(P, t);
            lr.SetPosition(i, C);
        }
        
    }
    
    public bool[] CheckCollision()
    {
        /// <summary>
        /// Checks collision for restricted area objects.
        /// </summary>
        Vector3[] posList = new Vector3[restrictedAreas.Length];
        float[] radList = new float[restrictedAreas.Length];
        bool[] collisionFlag = new bool[restrictedAreas.Length];
        for (int i = 0; i < restrictedAreas.Length; i++) 
        {
            if (restrictedAreas[i].gameObject.activeSelf)
            {            
                posList[i] = restrictedAreas[i].position;
                radList[i] = restrictedAreas[i].localScale.x/2;
                collisionFlag[i] = CheckCollision(posList[i], radList[i]);
            }
        }
        if (collisionFlag.Contains(true))
        {
            lr.sharedMaterial.color = Color.red;
            isCollide = true;
        }
        else
        {
            lr.sharedMaterial.color = Color.green;
            isCollide = false;
        };


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

    /// <summary>
    /// Cubic (p=3) clamped B-spline noktasý. 4 kontrol noktasý ve U=[0,0,0,0,1,1,1,1].
    /// Cox–de Boor baz fonksiyonlarý ile hesaplar.
    /// </summary>
    Vector3 BSplinePointDegree3(Vector3[] P, float t)
    {
        // p=3, i=0..3
        float N0 = Nip(0, 3, t, U);
        float N1 = Nip(1, 3, t, U);
        float N2 = Nip(2, 3, t, U);
        float N3 = Nip(3, 3, t, U);

        return N0 * P[0] + N1 * P[1] + N2 * P[2] + N3 * P[3];
    }

    /// <summary>
    /// Cox–de Boor rekürsiyonu: N_{i,p}(t)
    /// </summary>
    float Nip(int i, int p, float t, float[] knots)
    {
        if (p == 0)
        {
            // N_{i,0}(t) = 1 if U_i <= t < U_{i+1} (sað uçta kapanýþ için t==1 özel durumu da düþünülür)
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

        if (startPoint) Gizmos.DrawSphere(startPoint.position, gizmoSize);
        if (controlPoint1) Gizmos.DrawSphere(controlPoint1.position, gizmoSize);
        if (controlPoint2) Gizmos.DrawSphere(controlPoint2.position, gizmoSize);
        if (endPoint) Gizmos.DrawSphere(endPoint.position, gizmoSize);

        // Kontrol poligonu
        if (startPoint && controlPoint1) Gizmos.DrawLine(startPoint.position, controlPoint1.position);
        if (controlPoint1 && controlPoint2) Gizmos.DrawLine(controlPoint1.position, controlPoint2.position);
        if (controlPoint2 && endPoint) Gizmos.DrawLine(controlPoint2.position, endPoint.position);
    }
}
