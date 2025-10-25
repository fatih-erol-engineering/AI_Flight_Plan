using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BSplineDrawer : MonoBehaviour
{
    [field: SerializeField] public Transform controlPointContainer { get; private set; }
    [field: SerializeField] public Transform tubeContainer { get; private set; }
    [field: SerializeField] public GameObject controlPointPrefab { get; private set; }
    [field: SerializeField] public Waypoint waypointStart { get; private set; }
    [field: SerializeField] public Waypoint waypointEnd { get; private set; }        
    [SerializeField] public TimeGame startTime { get => waypointStart.time;}
    [SerializeField] public TimeGame endTime { get => waypointEnd.time; }
    [SerializeField,HideInInspector] private TimeGame prev_startTime;
    [SerializeField,HideInInspector] private TimeGame prev_endTime;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField, HideInInspector] private ControlPoint[] controlPoints;
    [SerializeField] private int segmentCount = 32;
    [SerializeField, HideInInspector] private LineRenderer lineRenderer;
    [SerializeField, HideInInspector] private Transform[] points;
    [SerializeField, HideInInspector] private Vector3[] prev_pointPositions;
    [SerializeField, HideInInspector] private float[] distanceArray;
    [SerializeField, HideInInspector] private Vector3[] trajectoryPositions;
    [SerializeField, HideInInspector] private TrajectoryPoint[] trajectoryPoints;


    [Header("Tube")]
    [SerializeField] private bool showTube = true;
    [SerializeField] private GameObject tubePrefab;
    [SerializeField] private float tubeRadius = 5f;
    [SerializeField] private float tubeEdgeSize = 0.1f;
    [SerializeField, ColorUsage(true, true)] private Color tubeEdgeColor = Color.white;
    [SerializeField] private Color tubeSurfaceColor = Color.blue;

    [SerializeField, HideInInspector] private Transform tube;
    [SerializeField, HideInInspector] private TubeManager[] tubeManagers;

    [SerializeField, HideInInspector] Vector3 prev_CollisionPoint = Vector3.positiveInfinity;
    

    public void AssignData()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        lineRenderer.colorGradient = new Gradient();

        int ct = 0;
        controlPoints = new ControlPoint[controlPointContainer.childCount];

        for (int i = 0; i < controlPointContainer.childCount; i++)
        {
            Transform child = controlPointContainer.GetChild(i);
            var controlPoint = child.GetComponent<ControlPoint>();
            controlPoints[ct] = controlPoint;
            ct++;
            
        }
        
        points = new Transform[controlPoints.Length + 2]; // 2 comes from start and end waypoints
        points[0] = waypointStart.transform;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            points[i + 1] = controlPoints[i].transform;
        }
        points[points.Length - 1] = waypointEnd.transform;
        trajectoryPositions = new Vector3[segmentCount];
        distanceArray = new float[trajectoryPositions.Length - 1];
        DrawCurve(points, lineRenderer, segmentCount);

        trajectoryPoints = new TrajectoryPoint[trajectoryPositions.Length];

        tubeManagers = new TubeManager[controlPoints.Length + 1];
        if (controlPoints != null)
        {
            GameObject go = Instantiate(tubePrefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(tubeContainer, true);
            tubeManagers[0] = go.GetComponentInChildren<TubeManager>();

            Vector3 start = waypointStart.transform.position;
            Vector3 end = controlPoints[0].GetClosestPointToSpline();

            tubeManagers[0].SetStartAndEndPositions(start, end);

            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                if (showTube)
                {
                    go = Instantiate(tubePrefab, Vector3.zero, Quaternion.identity);
                    go.transform.SetParent(tubeContainer, true);
                    tubeManagers[i + 1] = go.GetComponentInChildren<TubeManager>();

                    start = controlPoints[i].GetClosestPointToSpline();
                    end = controlPoints[i + 1].GetClosestPointToSpline();

                    tubeManagers[i + 1].SetStartAndEndPositions(start, end);
                }
            }
            go = Instantiate(tubePrefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(tubeContainer, true);
            tubeManagers[tubeManagers.Length - 1] = go.GetComponentInChildren<TubeManager>();

            start = controlPoints[controlPoints.Length - 1].GetClosestPointToSpline();
            end = waypointEnd.transform.position;

            tubeManagers[tubeManagers.Length - 1].SetStartAndEndPositions(start, end);



            prev_pointPositions = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                prev_pointPositions[i] = points[i].position;
            }




        }
    }
    public void Tick()
    {
        UpdateWithPerformance();          
    }

    public void UpdateWithPerformance()
    {
        bool updateCondition = false;


        for (int i = 0; i < points.Length; i++)
        {
            updateCondition |= prev_pointPositions[i] != points[i].position;
            prev_pointPositions[i] = points[i].position;
        }

        if (!updateCondition)
        {
            updateCondition |= (prev_startTime.second != startTime.second) || (prev_endTime.second != endTime.second);
            prev_startTime = startTime;
            prev_endTime = endTime;
        }   
        
        if (updateCondition)
        {
            UpdateImmediately();
        }

    }



    public void UpdateImmediately()
    {
        DrawCurve(points, lineRenderer, segmentCount);

        if (showTube)
        {
            if (controlPoints != null)
            {
                Vector3 start = waypointStart.transform.position;
                Vector3 end = controlPoints[0].GetClosestPointToSpline();

                tubeManagers[0].SetStartAndEndPositions(start, end);

                for (int i = 0; i < controlPoints.Length - 1; i++)
                {
                    if (showTube)
                    {
                        start = controlPoints[i].GetClosestPointToSpline();
                        end = controlPoints[i + 1].GetClosestPointToSpline();


                        tubeManagers[i + 1].SetStartAndEndPositions(start, end);
                        tubeManagers[i + 1].Tick();
                    }
                }
                start = controlPoints[controlPoints.Length - 1].GetClosestPointToSpline();
                end = waypointEnd.transform.position;

                tubeManagers[tubeManagers.Length - 1].SetStartAndEndPositions(start, end);
                tubeManagers[tubeManagers.Length - 1].Tick();
            }
        }
        UpdateDistanceList();       
        UpdateTrajectoryPointsWithTimeInfo();        
    }













    public void UpdateColor()
    {
        if (!lineRenderer) return;

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
        lineRenderer.colorGradient = g;

    }

    public void SetStartColor(Color _color)
    {
        startColor = _color;
        UpdateColor();
    }
    public void SetEndColor(Color _color)
    {
        endColor = _color;
        UpdateColor();
    }
    public void SetStartWaypoint(Waypoint _waypoint)
    {
        waypointStart = _waypoint;
    }
    public void SetEndWaypoint(Waypoint _waypoint)
    {
        waypointEnd = _waypoint;
    }
    public void SetControlPoints(Vector3[] _controlPointPositions)
    {
        DeleteControlPoints();
        controlPoints = new ControlPoint[_controlPointPositions.Length];
        int ct = 0;
        foreach (Vector3 _controlPointPosition in _controlPointPositions)
        {
            ControlPoint controlPoint = Instantiate(controlPointPrefab, _controlPointPosition, Quaternion.identity, controlPointContainer).GetComponent<ControlPoint>();
            controlPoints[ct] = controlPoint;
            ct++;
        }
    }
    
     public void SetControlPoints(ControlPoint[] _controlPoints)
    {
        DeleteControlPoints();
        int ct = 0;
        controlPoints = new ControlPoint[_controlPoints.Length];
        foreach (ControlPoint _controlPoint in _controlPoints)
        {            
            _controlPoint.transform.parent = controlPointContainer;
            controlPoints[ct] = _controlPoint;
            ct++;
        }
    }
    
    
    
    public void DeleteControlPoints()
    {
        foreach (Transform child in controlPointContainer)
        {
            var controlPoint = child.GetComponent<ControlPoint>();
            if (controlPoint != null)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(controlPoint);
#else
                Destroy(controlPoint);
#endif
            }
        }
        controlPoints = new ControlPoint[0];
    }

    public void Create()
    {
        AssignData();
        DrawCurve(points, lineRenderer, segmentCount);
        UpdateDistanceList();
        UpdateTrajectoryPointsWithTimeInfo();
    }
    public void Clear()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
        
    }



    public void SetTubeRadius(float _radius)
    {
        if (tubeRadius != _radius)
        {
            tubeRadius = _radius;
            foreach (TubeManager tubeManager in tubeManagers)
                tubeManager.SetRadius(_radius);            
        }
    }
    public void SetTubeEdgeColor(Color _color)
    {
        if (tubeEdgeColor != _color)
        {
            tubeEdgeColor = _color;
            foreach (TubeManager tubeManager in tubeManagers)
                tubeManager.SetEdgeColor(_color);
        }
    }
        public void SetTubeEdgeSize(float _size)
    {
        if (tubeEdgeSize != _size)
        {
            tubeEdgeSize = _size;
            foreach (TubeManager tubeManager in tubeManagers)
                tubeManager.SetEdgeSize(_size);
        }        
    }
    public void SetTubeSurfaceColor(Color _color)
    {
        if (tubeSurfaceColor != _color)
        {
            tubeSurfaceColor = _color;
            foreach (TubeManager tubeManager in tubeManagers)
                tubeManager.SetSurfaceColor(_color);
        }
    }












    public void DrawCurve(Transform[] _points, LineRenderer lineRenderer, int segmentCount)
    {
        // need at least degree+1 control points for cubic B-spline
        int degree = 2;
        if (_points.Length <= degree)
        {
            // fallback: draw straight polyline through control points
            lineRenderer.positionCount = _points.Length;
            trajectoryPositions = new Vector3[_points.Length];
            distanceArray = new float[_points.Length - 1];
            float cumulativeDistance = 0f;
            for (int i = 0; i < _points.Length; i++)
            {
                lineRenderer.SetPosition(i, _points[i].transform.position);
                trajectoryPositions[i] = _points[i].transform.position;
                if (i > 0) 
                {
                    cumulativeDistance += Vector3.Distance(trajectoryPositions[i], trajectoryPositions[i - 1]);
                    distanceArray[i - 1] = cumulativeDistance;
                }
            }
            return;
        }

        segmentCount = Mathf.Max(1, segmentCount);
        lineRenderer.positionCount = segmentCount;
        int cpCt = 0;
        bool changeCpFlag = false;
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 position = DeBoorCox(_points, t, degree);
            lineRenderer.SetPosition(i, position);
            trajectoryPositions[i] = position;
            float cumulativeDistance = 0f;
            if (i > 0) 
            {
                cumulativeDistance += Vector3.Distance(trajectoryPositions[i], trajectoryPositions[i - 1]);
                distanceArray[i - 1] = cumulativeDistance;
            }

            // Set closest points to spline for control points
            if (i > 0)
            {
                if (cpCt <= controlPoints.Length - 1)
                {
                    float dist1 = Vector3.Distance(controlPoints[cpCt].transform.position, lineRenderer.GetPosition(i - 1));
                    float dist2 = Vector3.Distance(controlPoints[cpCt].transform.position, position);

                    changeCpFlag = dist2 > dist1;
                    if (changeCpFlag)
                    {
                        controlPoints[cpCt].SetClosestPointToSpline(position);
                        cpCt++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// De Boor algorithm for clamped uniform B-spline
    /// points = control points (P0..Pn)
    /// t in [0,1]
    /// degree = spline degree (e.g. 3 for cubic)
    /// </summary>
    private Vector3 DeBoorCox(Transform[] _points, float t, int degree)
    {
        int n = _points.Length - 1;
        int p = degree;
        int m = n + p + 1; // highest knot index
        // build clamped uniform knot vector in [0,1]
        float[] knots = new float[m + 1];
        for (int i = 0; i <= m; i++)
        {
            if (i <= p) knots[i] = 0f;
            else if (i >= m - p) knots[i] = 1f;
            else knots[i] = (float)(i - p) / (float)(m - 2 * p);
        }

        // clamp t into [0,1]
        t = Mathf.Clamp01(t);

        // find knot span k such that knots[k] <= t < knots[k+1]
        int k = p; // default
        if (t >= 1f - Mathf.Epsilon)
        {
            k = m - p - 1; // last valid span
        }
        else
        {
            for (int i = p; i <= m - p - 1; i++)
            {
                if (t >= knots[i] && t < knots[i + 1])
                {
                    k = i;
                    break;
                }
            }
        }

        // initialize d[0..p] = P_{k-p} .. P_{k}
        Vector3[] d = new Vector3[p + 1];
        for (int j = 0; j <= p; j++)
        {
            int idx = k - p + j;
            idx = Mathf.Clamp(idx, 0, n);
            d[j] = _points[idx].transform.position;
            // // // // // if (idx != 0 || idx == _points.Length - 1) // start end and points are waypoints, so only control points are set
            // // // // // {
            // // // // //     controlPoints[idx - 1].SetClosestPointToSpline(d[j]); 
            // // // // // }
        }

        // de Boor recursion
        for (int r = 1; r <= p; r++)
        {
            for (int j = p; j >= r; j--)
            {
                float denom = knots[k + j - r + 1] - knots[k - p + j];
                float alpha = 0f;
                if (Mathf.Abs(denom) > Mathf.Epsilon)
                    alpha = (t - knots[k - p + j]) / denom;
                d[j] = (1f - alpha) * d[j - 1] + alpha * d[j];
            }
        }

        return d[p];
    }
















    void UpdateDistanceList()
    {
        distanceArray = new float[segmentCount - 1];
        float cumulativeDistance = 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            if (i > 1)
            {
                float delta = (lineRenderer.GetPosition(i) - lineRenderer.GetPosition(i-1)).magnitude;
                cumulativeDistance += delta;
                distanceArray[i - 1] = cumulativeDistance;
            }
        }
    }

    public float[] GetDistanceArray()
    {
        return distanceArray;
    }

    public Vector3[] GetTrajectoryPositionArray()
    {
        return trajectoryPositions;
    }

    public void UpdateTrajectoryPointsWithTimeInfo()
    {
        for (int i = 0; i < trajectoryPositions.Length; i++)
        {
            float t = (float)i / (trajectoryPositions.Length - 1);
            float timeSeconds = Mathf.Lerp(startTime.second, endTime.second, t);
            trajectoryPoints[i] = new TrajectoryPoint(trajectoryPositions[i], timeSeconds);
        }
    }

      public List<CollisionInfo> CheckCollisionWithAnotherSegment(BSplineDrawer otherSegment, float geometricCollisionThreshold_m, float timeCollision_s)
    {
        TrajectoryPoint[] traj1Points = trajectoryPoints;
        TrajectoryPoint[] traj2Points = otherSegment.trajectoryPoints;
        List<CollisionInfo> innerCollisionInfoList = new List<CollisionInfo>();
        List<CollisionInfo> collisionInfoList = new List<CollisionInfo>();

        foreach (var traj1Point in traj1Points)
        {
            foreach (var traj2Point in traj2Points)
            {
                if (Mathf.Abs(traj1Point.time - traj2Point.time) < timeCollision_s)
                {
                    if ((Vector3.Distance(traj2Point.position, traj1Point.position) < geometricCollisionThreshold_m) && prev_CollisionPoint != traj1Point.position)
                    {

                        collisionInfoList.Add(new CollisionInfo
                        {
                            objCurrent = gameObject,
                            objCollidedWith = otherSegment.gameObject,
                            point = traj1Point.position,
                            time = traj1Point.time,
                        });
                        prev_CollisionPoint = traj1Point.position;

                    }
                }
            }
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

