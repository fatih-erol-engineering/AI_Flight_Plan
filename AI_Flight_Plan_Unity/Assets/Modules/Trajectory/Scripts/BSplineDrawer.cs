using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BSplineDrawer : MonoBehaviour
{
    [Header("Waypoints & Control Points")]
    [SerializeField, HideInInspector] private List<ControlPoint> controlPoints;
    [field: SerializeField] public Waypoint waypointStart { get; private set; }
    [field: SerializeField] public Waypoint waypointEnd { get; private set; }
    [field: SerializeField, HideInInspector] public TrajectoryPoint[] trajectoryPoints { get; private set; }
    [SerializeField] public TimeGame startTime { get => waypointStart.time; }
    [SerializeField] public TimeGame endTime { get => waypointEnd.time; }
    [SerializeField, HideInInspector] private bool isReadyToUpdate = false;

    [Header("Containers & Prefabs")]
    [field: SerializeField] public Transform controlPointContainer { get; private set; }
    [field: SerializeField] public Transform tubeContainer { get; private set; }
    [field: SerializeField] public GameObject controlPointPrefab { get; private set; }

    [Header("Appearance")]
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private int segmentCount = 32;
    [SerializeField, HideInInspector] private LineRenderer lineRenderer;

    [Header("Tube")]
    [SerializeField] private bool showTubes = true;
    [SerializeField] private GameObject tubePrefab;
    [SerializeField] private TubeManager[] tubeManagers;


    public void Awake()
    {
        GameEvents.instance.OnWaypointPositionChanged += OnWaypointPositionChanged;
        GameEvents.instance.OnControlPointPositionChanged += OnControlPointPositionChanged;
    }
    public void OnValidate()
    {
        GameEvents.instance.OnWaypointPositionChanged += OnWaypointPositionChanged;
        GameEvents.instance.OnControlPointPositionChanged += OnControlPointPositionChanged;
    }
    public void OnDestroy()
    {
        GameEvents.instance.OnWaypointPositionChanged -= OnWaypointPositionChanged;
        GameEvents.instance.OnControlPointPositionChanged -= OnControlPointPositionChanged;
    }
    public void OnWaypointPositionChanged(Waypoint wp, Vector3 oldPosition)
    {
        if (wp == waypointStart || wp == waypointEnd)
        {
            Tick();
        }
    }
    public void OnControlPointPositionChanged(ControlPoint cp, Vector3 oldPosition)
    {
        if (controlPoints.Contains(cp))
        {
            Tick();
        }
    }

    public void Create()
    {
        bool initCondition = controlPointContainer != null && tubeContainer != null && controlPointPrefab != null;
        initCondition = initCondition && waypointStart != null && waypointEnd != null;
        initCondition = initCondition && waypointStart.time.second < waypointEnd.time.second;
        initCondition = initCondition && tubePrefab != null;
        initCondition = initCondition && segmentCount > 1;

        if (initCondition)
        {
            // Trajectory Points Initialization
            int trajectoryPointCount = segmentCount + 1;
            trajectoryPoints = new TrajectoryPoint[trajectoryPointCount];
            for (int i = 0; i < trajectoryPointCount; i++)
            {
                trajectoryPoints[i] = new TrajectoryPoint(Vector3.zero, new TimeGame(0f));
            }

            // Control Points Initialization
            controlPoints = new List<ControlPoint>();
            for (int i = 0; i < controlPointContainer.childCount; i++)
            {
                Transform child = controlPointContainer.GetChild(i);
                var controlPoint = child.GetComponent<ControlPoint>();
                controlPoints.Add(controlPoint);
            }
            DrawCurve();
            // Line Renderer Initialization
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = segmentCount;
            lineRenderer.colorGradient = new Gradient();
            UpdateLineRenderer();


            // Tube Managers Initialization
            if (showTubes)
            {
                tubeManagers = new TubeManager[controlPoints.Count + 1];
                if (controlPointContainer.childCount != 0)
                {
                    tubeManagers[0] = SpawnTube();
                    tubeManagers[0].SetStartPosition(waypointStart.transform.position);
                    tubeManagers[0].SetEndPosition(controlPoints[0].GetClosestPointToSpline());

                    for (int i = 0; i < controlPoints.Count - 1; i++)
                    {
                        if (showTubes)
                        {
                            tubeManagers[i + 1] = SpawnTube();
                            tubeManagers[i + 1].SetStartPosition(controlPoints[i].GetClosestPointToSpline());
                            tubeManagers[i + 1].SetEndPosition(controlPoints[i + 1].GetClosestPointToSpline());
                        }
                    }

                    tubeManagers[tubeManagers.Length - 1] = SpawnTube();
                    tubeManagers[tubeManagers.Length - 1].SetStartPosition(controlPoints[controlPoints.Count - 1].GetClosestPointToSpline());
                    tubeManagers[tubeManagers.Length - 1].SetEndPosition(waypointEnd.transform.position);
                }
                else
                {
                    tubeManagers[0] = SpawnTube();
                    tubeManagers[0].SetStartPosition(waypointStart.transform.position);
                    tubeManagers[0].SetEndPosition(waypointEnd.transform.position);
                }
            }
            isReadyToUpdate = true;
        }
        else
        {
            isReadyToUpdate = false;
            Debug.LogWarning("BSplineDrawer: Initialization failed! Missing references or invalid segment count.");
        }
    }

    public void Tick()
    {
        CheckReadyToUpdate();
        if (isReadyToUpdate)
        {
            DrawCurve();
            UpdateLineRenderer();
            UpdateTubes();
        }
        else
        {
            Debug.LogWarning("BSplineDrawer: Not ready to update.");
        }
    }

    public bool CheckReadyToUpdate()
    {
        bool waypointCondition = waypointStart != null && waypointEnd != null && waypointStart.time.second < waypointEnd.time.second;
        if (!waypointCondition)
        {
            Debug.LogWarning("BSplineDrawer: Waypoints are not properly set or times are invalid.");
        }
        isReadyToUpdate = waypointCondition;
        return waypointCondition; // 2 adet waypoint var ve başlangıç bitiş zamanları sıralı. 
    }

    public void UpdateTubes()
    {
        if (tubeManagers == null || tubeManagers.Length == 0) return;
        if (showTubes)
        {
            if (controlPointContainer.childCount != 0)
            {

                tubeManagers[0].SetStartPosition(waypointStart.transform.position);
                tubeManagers[0].SetEndPosition(controlPoints[0].GetClosestPointToSpline());

                for (int i = 0; i < controlPoints.Count - 1; i++)
                {
                    if (showTubes)
                    {

                        tubeManagers[i + 1].SetStartPosition(controlPoints[i].GetClosestPointToSpline());
                        tubeManagers[i + 1].SetEndPosition(controlPoints[i + 1].GetClosestPointToSpline());
                    }
                }

                tubeManagers[tubeManagers.Length - 1].SetStartPosition(controlPoints[controlPoints.Count - 1].GetClosestPointToSpline());
                tubeManagers[tubeManagers.Length - 1].SetEndPosition(waypointEnd.transform.position);
            }
            else
            {

                tubeManagers[0].SetStartPosition(waypointStart.transform.position);
                tubeManagers[0].SetEndPosition(waypointEnd.transform.position);
            }
        }
    }
    public void Clear()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
        DeleteTubes();
    }
    public void DeleteTubes()
    {
        if (tubeContainer == null) return;

        // Iterate backwards to safely remove children without skipping
        int currentCount = tubeContainer.childCount;
        for (int i = 0; i < currentCount; i++)
        {
            Transform child = tubeContainer.GetChild(0); // Always get the first child since we are removing them
            if (child == null) continue;
#if UNITY_EDITOR            
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        tubeManagers = new TubeManager[0];
    }


    public void UpdateLineRenderer()
    {

        // Position Update
        if (!lineRenderer) return;
        if (lineRenderer.positionCount != segmentCount) lineRenderer.positionCount = segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, trajectoryPoints[i].position);
        }

        // Color Update 

        int n = 2;
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
        g.SetKeys(cKeys, aKeys);

        lineRenderer.colorGradient = g;
    }

    public void SetStartColor(Color _color)
    {
        if (startColor != _color)
        {
            startColor = _color;
            if (isReadyToUpdate)
            {
                UpdateLineRenderer();
            }
        }
    }
    public void SetEndColor(Color _color)
    {
        if (endColor != _color)
        {
            endColor = _color;
            if (isReadyToUpdate)
            {
                UpdateLineRenderer();
            }
        }
    }
    public void SetStartWaypoint(Waypoint _waypoint)
    {
        if (waypointStart != _waypoint)
        {
            waypointStart = _waypoint;
            if (isReadyToUpdate)
            {
                Tick();
            }
        }
    }
    public void SetEndWaypoint(Waypoint _waypoint)
    {
        if (waypointEnd != _waypoint)
        {
            waypointEnd = _waypoint;
            if (isReadyToUpdate)
            {
                Tick();
            }
        }
    }

    public void AddControlPoint(ControlPoint _controlPoint)
    {
        _controlPoint.transform.parent = controlPointContainer;
        controlPoints.Add(_controlPoint);
        Tick();
    }
    public void RemoveLastControlPoint()
    {
        if (controlPoints.Count > 0)
        {
            ControlPoint lastControlPoint = controlPoints[controlPoints.Count - 1];
            controlPoints.RemoveAt(controlPoints.Count - 1);
            Destroy(lastControlPoint.gameObject);
            if (isReadyToUpdate)
            {
                Tick();
            }
        }
    }

    public void DrawCurve()
    {
        // need at least degree+1 control points for cubic B-spline
        if (isReadyToUpdate)
        {
            Vector3[] points = new Vector3[2 + controlPoints.Count];
            points[0] = waypointStart.transform.position;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                points[i + 1] = controlPoints[i].transform.position;
            }
            points[points.Length - 1] = waypointEnd.transform.position;
            float cumulativeDistance = 0f;

            int cpCt = 0;
            bool changeCpFlag = false;
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                trajectoryPoints[i].position = DeBoorCox(points, t, 2);
                trajectoryPoints[i].time = new TimeGame(Mathf.Lerp(startTime.second, endTime.second, (float)i / (segmentCount - 1)));
                if (i > 0)
                {
                    cumulativeDistance += Vector3.Distance(trajectoryPoints[i].position, trajectoryPoints[i - 1].position);
                    trajectoryPoints[i - 1].distanceToStart = cumulativeDistance;
                }

                // Set closest points to spline for control points
                if (i > 0)
                {
                    if (cpCt <= controlPoints.Count - 1)
                    {
                        float dist1 = Vector3.Distance(controlPoints[cpCt].transform.position, trajectoryPoints[i - 1].position);
                        float dist2 = Vector3.Distance(controlPoints[cpCt].transform.position, trajectoryPoints[i].position);

                        changeCpFlag = dist2 > dist1;
                        if (changeCpFlag)
                        {
                            controlPoints[cpCt].SetClosestPointToSpline(trajectoryPoints[i].position);
                            cpCt++;
                        }
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
    private Vector3 DeBoorCox(Vector3[] _points, float t, int degree)
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
            d[j] = _points[idx];
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








    // public float[] GetDistanceArray()
    // {
    //     return distanceArray;
    // }

    // public Vector3[] GetTrajectoryPositionArray()
    // {
    //     return trajectoryPositions;
    // }


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
                if (Mathf.Abs(traj1Point.time.second - traj2Point.time.second) < timeCollision_s)
                {
                    if ((Vector3.Distance(traj2Point.position, traj1Point.position) < geometricCollisionThreshold_m))
                    {

                        collisionInfoList.Add(new CollisionInfo
                        {
                            objCurrent = gameObject,
                            objCollidedWith = otherSegment.gameObject,
                            point = traj1Point.position,
                            time = traj1Point.time,
                        });

                    }
                }
            }
        }
        return collisionInfoList;
    }
    public TubeManager SpawnTube()
    {
        GameObject go = Instantiate(tubePrefab, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(tubeContainer, true);
        TubeManager _tubeManager = go.GetComponentInChildren<TubeManager>();
        return _tubeManager;
    }

    public void Hidetubes()
    {
        if (tubeManagers == null) return;
        foreach (var tube in tubeManagers)
        {
            tube.gameObject.SetActive(false);
        }
        showTubes = false;
    }
    public void Showtubes()
    {
        if (tubeManagers == null) return;
        foreach (var tube in tubeManagers)
        {
            tube.gameObject.SetActive(true);
        }
        showTubes = true;
    }
    public void SetControlPoints(List<ControlPoint> _controlPoints)
    {
        if (_controlPoints != controlPoints)
        {
            DeleteControlPoints();
            controlPoints.Clear();
            foreach (ControlPoint _controlPoint in _controlPoints)
            {
                _controlPoint.transform.parent = controlPointContainer;
                controlPoints.Add(_controlPoint);
            }
            if (isReadyToUpdate)
            {
                Tick();
            }
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
        controlPoints.Clear();
    }
}

public class TrajectoryPoint
{
    public Vector3 position;
    public TimeGame time;
    public float distanceToStart; //Distance percentage from start to end in percent 0-1. 1 Means end point. 0 Means start point.
    public CollisionInfo collisionInfo;
    public TrajectoryPoint(Vector3 _pos, TimeGame _t)
    {
        position = _pos;
        time = _t;
        collisionInfo = new CollisionInfo();
    }
}
