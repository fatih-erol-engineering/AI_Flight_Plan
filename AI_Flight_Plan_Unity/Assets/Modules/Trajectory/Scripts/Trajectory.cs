using UnityEngine;

[ExecuteAlways]
public class Trajectory : MonoBehaviour
{

    public TrajectoryType trajectoryType;



    [field: SerializeField]
    public Transform waypointContainer { get; private set; }        
    [field: SerializeField]
    public Transform controlPointContainer{ get; private set; }
    [field: SerializeField]
    public Transform segmentContainer{ get; private set; }
    [field: SerializeField]
    public Transform restrictedAreaContainer{ get; private set; }



    public Waypoint[] waypoints { get; private set; }
    public ControlPoint[] controlPoints { get; private set; }
    public BSplineSegment[] bSplineSegments { get; private set; }
    public RestrictedArea[] restrictedAreas { get; private set; }



    [SerializeField]
    private float initialControlPointDistance = 3f;
    [SerializeField]
    private float lineWidth = 10f;    
    public Theme theme;

    [Header("Time from Waypoints")]
    public TimeGame startTime;
    public TimeGame endTime;
    void AssignData()
    {        
        if (!waypointContainer) waypointContainer = transform.Find("WaypointContainer");
        if (!controlPointContainer) controlPointContainer = transform.Find("ControlPointContainer");
        if (!segmentContainer) segmentContainer = transform.Find("SegmentContainer");
        waypoints = new Waypoint[waypointContainer.childCount]; 
        controlPoints = new ControlPoint[(waypointContainer.childCount-2)*2 + 2]; 
        bSplineSegments = new BSplineSegment[waypointContainer.childCount-1];
        restrictedAreas = new RestrictedArea[restrictedAreaContainer.childCount];


        for (int i = 0; i < waypointContainer.childCount; i++)
        {
            waypoints[i] = waypointContainer.GetChild(i).GetComponent<Waypoint>();
        }
        
        for (int i = 0; i < restrictedAreaContainer.childCount; i++)
        {         
            restrictedAreas[i] = restrictedAreaContainer.GetChild(i).GetComponent<RestrictedArea>();
        }
    }
    
    // public void Init()
    // {
    //     AssignData();
    //     Create();        
    // }
    public void DeleteAllWaypoints() 
    { 
        waypoints = null;
    }
    // public Waypoint CreateWaypoint(Vector3 globalPosition)
    // {
    //     if (waypointContainer == null)
    //     {
    //         GameObject waypointGO = new GameObject("WaypointContainer");
    //         waypointGO.transform.parent = transform;
    //         waypointGO.transform.localPosition = Vector3.zero;
    //         waypointContainer = waypointGO.transform;
    //     }

    //     GameObject waypoint = Instantiate(theme.waypointPrefab, globalPosition, transform.rotation, waypointContainer);
    //     Waypoint wp = waypoint.GetComponent<Waypoint>();
    //     AddWaypoint(waypoint.transform);
    //     return wp;
    // }
    // public Waypoint CreateWaypoint(Vector3 globalPosition, float time_s)
    // {
    //     if (waypointContainer == null)
    //     {
    //         GameObject waypointGO = new GameObject("WaypointContainer");
    //         waypointGO.transform.parent = transform;
    //         waypointGO.transform.localPosition = Vector3.zero;
    //         waypointContainer = waypointGO.transform;
    //     }

    //     GameObject waypoint = Instantiate(theme.waypointPrefab, globalPosition, transform.rotation, waypointContainer);
    //     Waypoint wp = waypoint.GetComponent<Waypoint>();
    //     if (wp != null) 
    //     {
    //         wp.time.second = time_s;
    //     }

    //     if (startTime == null)
    //     {
    //         startTime.second = time_s;
    //     }
    //     else
    //     {
    //         if (time_s<startTime.second)
    //         {
    //             startTime.second = time_s;
    //         }
    //     }

    //     if (endTime == null)
    //     {
    //         endTime.second = time_s;
    //     }
    //     else
    //     {
    //         if (time_s > endTime.second)
    //         {
    //             endTime.second = time_s;
    //         }
    //     }
        
    //     return wp;
    // }

    public void Clear()
    {
        if (segmentContainer != null)
        {
            for (int i = segmentContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(segmentContainer.GetChild(i).gameObject);
            }
        }
        if (controlPointContainer != null)
        {
            for (int i = controlPointContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(controlPointContainer.GetChild(i).gameObject);
            }
        }
        bSplineSegments = null;
        controlPoints = null ;
    }

    // public void Create()
    // {
    //     AssignData();
    //     int numSegments = waypoints.Length - 1;
    //     if (numSegments <= 0) return;
    //     if (bSplineSegments == null || bSplineSegments.Length != numSegments)
    //         bSplineSegments = new BSplineSegment[numSegments];


    //     if (segmentContainer == null)
    //     {
    //         var parentGO = new GameObject("SegmentContainer");
    //         parentGO.transform.parent = transform;
    //         segmentContainer = parentGO.transform;
    //     }
        
    //     startTime = waypoints[0].GetComponent<Waypoint>().time;
    //     endTime = waypoints[waypoints.Length - 1].GetComponent<Waypoint>().time;

    //     for (int i = 0; i < numSegments; i++)
    //     {
    //         GameObject segmentGO;
            
    //         if (i < segmentContainer.childCount)
    //         {
    //             segmentGO = segmentContainer.GetChild(i).gameObject;
    //         }
    //         else
    //         {
    //             segmentGO = Instantiate(theme.BSplineSegmentPrefab,transform.position,transform.rotation, segmentContainer);                
    //         }
            
    //         var seg = segmentGO.GetComponent<BSplineSegment>();
    //         if (seg == null)
    //             seg = segmentGO.AddComponent<BSplineSegment>();
    //         seg.initialControlPointDistance = initialControlPointDistance;
    //         seg.SetStartAndEndWaypoints(waypoints[i].GetComponent<Waypoint>(), waypoints[i + 1].GetComponent<Waypoint>());
            
    //         seg.restrictedAreas = restrictedAreas;
    //         seg.CreateControlPoints();            
    //         seg.GetComponent<LineRenderer>().startWidth = lineWidth;
    //         seg.GetComponent<LineRenderer>().endWidth = lineWidth;
    //         bSplineSegments[i] = seg;   
    //         seg.UpdateColorWithTotalTime(startTime, endTime);
    //     }

    //     for (int i = 0; i < numSegments-1; i++)
    //     {
    //         Vector3 delta1 = bSplineSegments[i].startPoint.transform.localPosition - bSplineSegments[i].endPoint.transform.localPosition;
    //         float len1 = (bSplineSegments[i].endPoint.transform.localPosition - bSplineSegments[i].controlPoint2.transform.localPosition).magnitude;
    //         Vector3 dir1 = delta1.normalized;

    //         Vector3 delta2 = bSplineSegments[i + 1].endPoint.transform.localPosition - bSplineSegments[i + 1].startPoint.transform.localPosition;
    //         float len2 = (bSplineSegments[i + 1].startPoint.transform.localPosition - bSplineSegments[i + 1].controlPoint1.transform.localPosition).magnitude;
    //         Vector3 dir2 = delta2.normalized;

    //         Vector3 dirNet = (dir1 + dir2).normalized;

    //         Vector3 normVec = Vector3.Cross(dir1, dir2).normalized;
    //         Vector3 controlPointDir1 = Vector3.Cross(dirNet, normVec).normalized;
    //         Vector3 controlPointDir2 = controlPointDir1 * (-1f);

    //         bSplineSegments[i].controlPoint2.transform.localPosition = bSplineSegments[i].endPoint.transform.localPosition + len1 * controlPointDir1;
    //         bSplineSegments[i + 1].controlPoint1.transform.localPosition = bSplineSegments[i].endPoint.transform.localPosition + len2 * controlPointDir2;            
    //     }

    //     // Declare Waypoint Relationships
    //     for (int i = 0; i < waypoints.Length; i++)
    //     {
    //         if (i == 0)
    //         {
    //             waypoints[i].GetComponent<Waypoint>().type = WaypointType.Open;
    //             waypoints[i].GetComponent<Waypoint>().controlPoints = new ControlPoint[1];
    //             waypoints[i].GetComponent<Waypoint>().controlPoints[0] = segmentContainer.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1;
    //             segmentContainer.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();
    //         }
    //         else if (i == waypoints.Length - 1)
    //         {
    //             waypoints[i].GetComponent<Waypoint>().type = WaypointType.Open;
    //             waypoints[i].GetComponent<Waypoint>().controlPoints = new ControlPoint[1];
    //             waypoints[i].GetComponent<Waypoint>().controlPoints[0] = segmentContainer.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2;
    //             segmentContainer.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();
    //         }
    //         else
    //         {
    //             waypoints[i].GetComponent<Waypoint>().type = WaypointType.Close;
    //             waypoints[i].GetComponent<Waypoint>().controlPoints = new ControlPoint[2];
    //             waypoints[i].GetComponent<Waypoint>().controlPoints[0] = segmentContainer.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2;
    //             waypoints[i].GetComponent<Waypoint>().controlPoints[1] = segmentContainer.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1;

    //             segmentContainer.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();
    //             segmentContainer.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();

    //             segmentContainer.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>().pairCP = segmentContainer.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>();
    //             segmentContainer.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>().pairCP = segmentContainer.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>();
    //         }
    //     }
        
    // }
#if UNITY_EDITOR
    void Update()
    {
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            var go = UnityEditor.Selection.activeGameObject;
            if (go != null)
            {
                if (go.GetComponent<ControlPoint>() != null)
                {
                    go.GetComponent<ControlPoint>().setPosition(go.transform.position);
                }
                if (go.GetComponent<Waypoint>() != null)
                {
                    go.GetComponent<Waypoint>().setPosition(go.transform.position);
                }
            }
        }
    }

#endif


}


public enum TrajectoryType
{
    Fixed,
    Travel
}