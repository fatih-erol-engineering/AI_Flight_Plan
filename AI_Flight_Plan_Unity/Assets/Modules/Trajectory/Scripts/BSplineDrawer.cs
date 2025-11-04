using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BSplineDrawer : MonoBehaviour
{
    [SerializeField] private Aircraft aircraft;

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
    [SerializeField] private int linePointNumber = 100;
    [SerializeField, HideInInspector] private LineRenderer lineRenderer;

    [Header("Tube")]
    [SerializeField] private bool showTubes = true;
    [SerializeField] private float tubeRadius = 10f;
    [SerializeField] private GameObject tubePrefab;
    [SerializeField] private TubeManager[] tubeManagers;

    public void SetIsCollided(bool _isCollided)
    {
        for (int i = 0; i < tubeManagers.Length; i++)
        {
            tubeManagers[i].SetIsCollided(_isCollided);
        }
    }
    void AssignData()
    {
        GameEvents.Instance.OnWaypointPositionChanged -= OnWaypointPositionChanged;
        GameEvents.Instance.OnWaypointTimeChanged -= OnWaypointTimeChanged;
        GameEvents.Instance.OnControlPointPositionChanged -= OnControlPointPositionChanged;

        GameEvents.Instance.OnWaypointPositionChanged += OnWaypointPositionChanged;
        GameEvents.Instance.OnWaypointTimeChanged += OnWaypointTimeChanged;
        GameEvents.Instance.OnControlPointPositionChanged += OnControlPointPositionChanged;

        SetLinePointNumber(linePointNumber, true);
        SetTubeRadius(tubeRadius, true);
    }
    public void SetAircraft(Aircraft _aircraft)
    {
        aircraft = _aircraft;
    }

    public void Awake()
    {
        AssignData();
    }

    public void OnDestroy()
    {
        GameEvents.Instance.OnWaypointPositionChanged -= OnWaypointPositionChanged;
        GameEvents.Instance.OnWaypointTimeChanged -= OnWaypointTimeChanged;
        GameEvents.Instance.OnControlPointPositionChanged -= OnControlPointPositionChanged;
    }
    public void OnWaypointPositionChanged(Waypoint wp, Vector3 oldPosition)
    {
        if (wp == waypointStart || wp == waypointEnd)
        {
            Tick();
        }
    }
    public void OnWaypointTimeChanged(Waypoint wp, TimeGame oldTime)
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
            curveSegments = new CurveSegment[controlPoints.Count + 1];
            for (int i = 0; i < curveSegments.Length; i++)
            {
                curveSegments[i] = new CurveSegment(); // Temporary initialization
                curveSegments[i].SetAircraft(aircraft);
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
                    curveSegments[i].SetTubeManager(tubeManagers[i]);
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
        bool isReadyToUpdate = waypointCondition && isCreated && curveSegments != null;
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
    public void SetLinePointNumber(int _pointNumber, bool isImmediate = false)
    {
        if (_pointNumber != linePointNumber || isImmediate)
        {
            linePointNumber = Mathf.Max(2, _pointNumber); // Minimum 2 points
            trajectoryPoints = new TrajectoryPoint[linePointNumber];
            for (int i = 0; i < linePointNumber; i++)
            {
                trajectoryPoints[i] = new TrajectoryPoint(Vector3.zero, new TimeGame(0f));
            }
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = linePointNumber;
            }
            if (CheckReadyToUpdate())
            {
                DrawCurve();
                UpdateLineRenderer();
            }
        }
    }
    public void SetTubeRadius(float _radius, bool isImmediate = false)
    {
        if (_radius != tubeRadius || isImmediate)
        {
            tubeRadius = _radius;
            foreach (var tubeManager in tubeManagers)
            {
                tubeManager.SetRadius(tubeRadius, isImmediate);
            }
        }
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
                        curveSegments[cpCt].SetStartAndEndPoint(trajectoryPoints[minIdx], trajectoryPoints[maxIdx]);
                        curveSegments[cpCt].SetRadius(tubeRadius);
                        controlPoints[cpCt].SetClosestPointToSpline(trajectoryPoints[i].position);
                        minIdx = i;
                        cpCt++;
                    }
                }
            }
        }
        {
            for (int i = 0; i < controlPoints.Count + 1; i++)
            {
                if (i == 0)
                {
                    curveSegments[i].controlPoint1 = controlPoints[i];
                }
                else if (i == controlPoints.Count)
                {
                    curveSegments[i].controlPoint1 = controlPoints[i - 1];
                }
                else
                {
                    curveSegments[i].controlPoint1 = controlPoints[i - 1];
                    curveSegments[i].controlPoint2 = controlPoints[i];
                }
            }
            curveSegments[curveSegments.Length - 1].SetStartAndEndPoint(trajectoryPoints[minIdx], trajectoryPoints[trajectoryPoints.Length - 1]);
            curveSegments[curveSegments.Length - 1].SetRadius(tubeRadius);
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

                    }
                }

            }
        }
        return collisionInfoList;
    }

    public List<CollisionInfoRestrictedArea> CheckCollisionWithRestrictedArea(AbsoluteRestrictedArea restrictedArea)
    {
        List<CollisionInfoRestrictedArea> collisionInfoList = new List<CollisionInfoRestrictedArea>();

        for (int i = 0; i < curveSegments.Length; i++)
        {
            CurveSegment curveSegment = curveSegments[i];
            // Check if the curve segment intersects with the restricted area
            if (IsCurveSegmentIntersectingWithRestrictedArea(curveSegment, restrictedArea))
            {
                CollisionInfoRestrictedArea collisionInfo = new CollisionInfoRestrictedArea
                {
                    segment = curveSegment,
                    restrictedArea = restrictedArea
                };
                collisionInfoList.Add(collisionInfo);
            }
        }

        return collisionInfoList;
    }
    private bool IsCurveSegmentIntersectingWithRestrictedArea(CurveSegment segment, AbsoluteRestrictedArea area)
    {
        Vector3 endA = segment.endPoint.position;
        Vector3 startA = segment.startPoint.position;
        Vector3 posB = area.transform.position;
        float radiusB = area.radius;
        bool _isCollided = CylinderSphereIntersect(startA, endA, segment.tubeManager.GetRadius(), posB, radiusB);
        return _isCollided;
    }

    public static bool CylinderSphereIntersect(
    Vector3 cylinderStart, Vector3 cylinderEnd, float cylinderRadius,
    Vector3 sphereCenter, float sphereRadius)
    {
        Vector3 axis = cylinderEnd - cylinderStart;
        float height = axis.magnitude;
        if (height < 1e-6f)
            return false; // çok kısa, güvenlik

        Vector3 dir = axis / height;
        Vector3 v = sphereCenter - cylinderStart;

        // Küre merkezinin silindir eksenine izdüşüm oranı
        float t = Vector3.Dot(v, dir);

        // Eksen dışına çıkarsa, uç kapaklara göre kontrol
        if (t < 0f)
        {
            float dist = Vector3.Distance(sphereCenter, cylinderStart);
            return dist <= (sphereRadius + cylinderRadius);
        }
        else if (t > height)
        {
            float dist = Vector3.Distance(sphereCenter, cylinderEnd);
            return dist <= (sphereRadius + cylinderRadius);
        }
        else
        {
            // Dik uzaklık
            Vector3 closestPoint = cylinderStart + t * dir;
            float distToAxis = Vector3.Distance(sphereCenter, closestPoint);
            return distToAxis <= (sphereRadius + cylinderRadius);
        }
    }

    static bool AreCylindersIntersecting(
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
public class CurveSegment : ICollidable
{
    public TrajectoryPoint startPoint;
    public TrajectoryPoint endPoint;
    public ControlPoint controlPoint1;
    public ControlPoint controlPoint2;
    public TubeManager tubeManager;
    public Aircraft aircraft;
    public Vector3 midPoint
    {
        get
        {
            return (startPoint.position + endPoint.position) / 2f;
        }
    }

    public bool isCollided { get; set; }

    public float radious;

    public void SetStartAndEndPoint(TrajectoryPoint _startPoint, TrajectoryPoint _endPoint)
    {
        startPoint = _startPoint;
        endPoint = _endPoint;
    }
    public void SetRadius(float _radious)
    {
        radious = _radious;
    }
    public void SetAircraft(Aircraft _aircraft)
    {
        aircraft = _aircraft;
    }
    public void SetTubeManager(TubeManager _tubeManager)
    {
        tubeManager = _tubeManager;
    }

    public void SetIsCollided(bool _val, bool isImmediate = false)
    {
        if (isCollided != _val || isImmediate)
        {
            tubeManager.SetIsCollided(_val, isImmediate);
            isCollided = _val;
        }
    }
}