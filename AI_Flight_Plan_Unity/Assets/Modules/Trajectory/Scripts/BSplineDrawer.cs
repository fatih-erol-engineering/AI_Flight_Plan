using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BSplineDrawer : MonoBehaviour
{
    [Header("Waypoints & Control Points & Curve Segments")]
    [SerializeField, HideInInspector] private List<ControlPoint> controlPoints;
    [field: SerializeField] public Waypoint waypointStart { get; private set; }
    [field: SerializeField] public Waypoint waypointEnd { get; private set; }
    [field: SerializeField, HideInInspector] public TrajectoryPoint[] trajectoryPoints { get; private set; }
    [SerializeField] public TimeGame startTime { get => waypointStart.time; }
    [SerializeField] public TimeGame endTime { get => waypointEnd.time; }
    [SerializeField] private CurveSegment[] curveSegments;
    [SerializeField] private bool isCreated = false;


    [Header("Containers & Prefabs")]
    [field: SerializeField] public Transform controlPointContainer { get; private set; }
    [field: SerializeField] public Transform tubeContainer { get; private set; }
    [field: SerializeField] public GameObject controlPointPrefab { get; private set; }

    [Header("Appearance")]
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private int linePointNumber = 32;
    [SerializeField, HideInInspector] private LineRenderer lineRenderer;

    [Header("Tube")]
    [SerializeField] private bool showTubes = true;
    [SerializeField] private float tubeRadius = 10f;
    [SerializeField] private GameObject tubePrefab;
    [SerializeField] private TubeManager[] tubeManagers;


    public void Awake()
    {
        GameEvents.Instance.OnWaypointPositionChanged += OnWaypointPositionChanged;
        GameEvents.Instance.OnControlPointPositionChanged += OnControlPointPositionChanged;
    }
    public void OnValidate()
    {
        GameEvents.Instance.OnWaypointPositionChanged += OnWaypointPositionChanged;
        GameEvents.Instance.OnControlPointPositionChanged += OnControlPointPositionChanged;
    }
    public void OnDestroy()
    {
        GameEvents.Instance.OnWaypointPositionChanged -= OnWaypointPositionChanged;
        GameEvents.Instance.OnControlPointPositionChanged -= OnControlPointPositionChanged;
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
        initCondition = initCondition && linePointNumber > 1;

        if (initCondition)
        {
            // Trajectory Points Initialization
            int trajectoryPointCount = linePointNumber;
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
            lineRenderer.positionCount = linePointNumber;
            lineRenderer.colorGradient = new Gradient();
            UpdateLineRenderer();


            // Tube Managers Initialization
            if (showTubes)
            {
                tubeManagers = new TubeManager[curveSegments.Length];
                for (int i = 0; i < curveSegments.Length; i++)
                {
                    tubeManagers[i] = SpawnTube();
                    tubeManagers[i].SetStartPosition(curveSegments[i].startPoint.position);
                    tubeManagers[i].SetEndPosition(curveSegments[i].endPoint.position);
                }
            }

            Tick();
            isCreated = true;
        }
        else
        {
            isCreated = false;
            Debug.LogWarning("BSplineDrawer: Initialization failed! Missing references or invalid segment count.");
        }
    }


    public void Tick()
    {
        CheckReadyToUpdate();
        if (CheckReadyToUpdate())
        {
            DrawCurve();
            UpdateLineRenderer();
            UpdateTubes();
            GameEvents.Instance.SplineChanged(this);
        }
        else
        {
            Debug.LogWarning("BSplineDrawer: Not ready to update.");
        }
    }

    public bool CheckReadyToUpdate()
    {
        bool waypointCondition = waypointStart != null && waypointEnd != null && waypointStart.time.second < waypointEnd.time.second;
        bool isReadyToUpdate = waypointCondition && isCreated;
        if (!isReadyToUpdate)
        {
            Debug.LogWarning("BSplineDrawer: Waypoints are not properly set or times are invalid.");
        }
        return isReadyToUpdate; // 2 adet waypoint var ve başlangıç bitiş zamanları sıralı. 
    }

    public void UpdateTubes()
    {
        if (tubeManagers == null || tubeManagers.Length == 0) return;
        if (showTubes)
        {
            for (int i = 0; i < curveSegments.Length; i++)
            {
                tubeManagers[i].SetStartPosition(curveSegments[i].startPoint.position);
                tubeManagers[i].SetEndPosition(curveSegments[i].endPoint.position);
                tubeManagers[i].SetRadius(tubeRadius);
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
        Debug.ClearDeveloperConsole();
        isCreated = false;
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
        if (lineRenderer.positionCount != linePointNumber) lineRenderer.positionCount = linePointNumber;
        for (int i = 0; i < linePointNumber; i++)
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
            if (CheckReadyToUpdate())
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
            if (CheckReadyToUpdate())
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
            if (CheckReadyToUpdate())
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
            if (CheckReadyToUpdate())
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
            if (CheckReadyToUpdate())
            {
                Tick();
            }
        }
    }

    public void DrawCurve()
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
        curveSegments = new CurveSegment[controlPoints.Count + 1];
        int minIdx = 0;
        int maxIdx = 0;
        for (int i = 0; i < linePointNumber; i++)
        {
            float t = (float)i / (linePointNumber - 1);
            trajectoryPoints[i].position = DeBoorCox(points, t, 2);
            trajectoryPoints[i].time = new TimeGame(Mathf.Lerp(startTime.second, endTime.second, (float)i / (linePointNumber - 1)));
            if (i > 0)
            {
                cumulativeDistance += Vector3.Distance(trajectoryPoints[i].position, trajectoryPoints[i - 1].position);
            }
            trajectoryPoints[i].distanceToStart = cumulativeDistance;

            // Set closest points to spline for control points
            if (i > 0)
            {
                if (cpCt < controlPoints.Count)
                {
                    float dist1 = Vector3.Distance(controlPoints[cpCt].transform.position, trajectoryPoints[i - 1].position);
                    float dist2 = Vector3.Distance(controlPoints[cpCt].transform.position, trajectoryPoints[i].position);

                    changeCpFlag = dist2 > dist1;
                    maxIdx = i;
                    if (changeCpFlag)
                    {
                        curveSegments[cpCt] = new CurveSegment(trajectoryPoints[minIdx], trajectoryPoints[maxIdx], tubeRadius);
                        controlPoints[cpCt].SetClosestPointToSpline(trajectoryPoints[i].position);
                        minIdx = i;
                        cpCt++;
                    }
                }
            }
        }
        {
            curveSegments[curveSegments.Length - 1] = new CurveSegment(trajectoryPoints[minIdx], trajectoryPoints[trajectoryPoints.Length - 1], tubeRadius); // Handle case when all control points are assigned
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


    public List<CollisionInfo> CheckCollisionWithAnotherSpline(BSplineDrawer otherSpline)
    {
        List<CollisionInfo> collisionInfoList = new List<CollisionInfo>();
        for (int i = 0; i < tubeManagers.Length; i++)
        {
            tubeManagers[i].SetIsCollided(false);
        }
        for (int j = 0; j < otherSpline.tubeManagers.Length; j++)
        {
            otherSpline.tubeManagers[j].SetIsCollided(false);
        }
        for (int i = 0; i < curveSegments.Length; i++)
        {
            CurveSegment curveSegment = curveSegments[i];
            for (int j = 0; j < otherSpline.curveSegments.Length; j++)
            {
                CurveSegment otherCurveSegment = otherSpline.curveSegments[j];
                // Timecheck
                bool a = (curveSegment.startPoint.time.second > otherCurveSegment.startPoint.time.second);
                bool b = (curveSegment.startPoint.time.second <= otherCurveSegment.endPoint.time.second);
                bool c = (curveSegment.endPoint.time.second > otherCurveSegment.startPoint.time.second);
                bool d = (curveSegment.endPoint.time.second <= otherCurveSegment.endPoint.time.second);
                bool timeFlag = (a && b) || (c && d);
                if (timeFlag)
                {
                    // There is a time overlap, proceed to geometric check
                    // Implement geometric collision detection between curveSegment and otherCurveSegment here
                    // If collision detected, create CollisionInfo and add to collisionInfoList

                    bool collisionFlag = AreCylindersIntersecting(curveSegment.startPoint.position, curveSegment.endPoint.position, curveSegment.radious,
                                            otherCurveSegment.startPoint.position, otherCurveSegment.endPoint.position, otherCurveSegment.radious);
                    if (collisionFlag)
                    {
                        CollisionInfo collisionInfo = new CollisionInfo
                        {
                            segment1 = curveSegment,
                            segment2 = otherCurveSegment
                        };
                        collisionInfoList.Add(collisionInfo);
                        if (showTubes)
                        {
                            tubeManagers[i].SetIsCollided(true);
                            otherSpline.tubeManagers[j].SetIsCollided(true);
                        }
                    }
                }

            }
        }
        return collisionInfoList;
    }
    public static bool AreCylindersIntersecting(
    Vector3 startA, Vector3 endA, float radiusA,
    Vector3 startB, Vector3 endB, float radiusB)
    {
        // --- 1. Eksen yön vektörlerini ve uzunlukları hesapla
        Vector3 uA = (endA - startA);
        Vector3 uB = (endB - startB);
        float lenA = uA.magnitude;
        float lenB = uB.magnitude;
        uA.Normalize();
        uB.Normalize();

        // --- 2. Eksenler arası en kısa mesafeyi bul
        Vector3 n = Vector3.Cross(uA, uB);
        float nMag = n.magnitude;

        float distance;
        if (nMag < 1e-6f) // Neredeyse paralel doğrular
        {
            Vector3 diff = startB - startA;
            distance = Vector3.Magnitude(diff - Vector3.Dot(diff, uA) * uA);
        }
        else
        {
            distance = Mathf.Abs(Vector3.Dot((startB - startA), n.normalized));
        }

        // --- 3. Eğer eksenler çok uzaktaysa zaten çakışmazlar
        if (distance > radiusA + radiusB)
            return false;

        // --- 4. En yakın noktaları bul (parametrik çözüm)
        // Line-line closest points
        Vector3 w0 = startA - startB;
        float a = Vector3.Dot(uA, uA);
        float b = Vector3.Dot(uA, uB);
        float c = Vector3.Dot(uB, uB);
        float d = Vector3.Dot(uA, w0);
        float e = Vector3.Dot(uB, w0);

        float denom = a * c - b * b;
        float tA, tB;

        if (denom < 1e-6f)
        {
            // Neredeyse paralel
            tA = 0f;
            tB = (b > c ? d / b : e / c);
        }
        else
        {
            tA = (b * e - c * d) / denom;
            tB = (a * e - b * d) / denom;
        }

        // --- 5. Parametreleri [0, length] aralığına projekte et
        tA = Mathf.Clamp(tA, 0f, lenA);
        tB = Mathf.Clamp(tB, 0f, lenB);

        // --- 6. Eksenler üzerindeki en yakın noktaları bul
        Vector3 closestA = startA + uA * tA;
        Vector3 closestB = startB + uB * tB;

        // --- 7. Noktalar arası mesafe
        float centerDist = Vector3.Distance(closestA, closestB);

        // --- 8. Çakışma kontrolü
        return centerDist <= (radiusA + radiusB);
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
            if (CheckReadyToUpdate())
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
    public TrajectoryPoint(Vector3 _pos, TimeGame _t)
    {
        position = _pos;
        time = _t;
    }
}
public class CurveSegment
{
    public TrajectoryPoint startPoint;
    public TrajectoryPoint endPoint;
    public Vector3 midPoint
    {
        get
        {
            return (startPoint.position + endPoint.position) / 2f;
        }
    }
    public float radious;
    public CurveSegment(TrajectoryPoint _startPoint, TrajectoryPoint _endPoint, float _radious)
    {
        startPoint = _startPoint;
        endPoint = _endPoint;
        radious = _radious;
    }
}