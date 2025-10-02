using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[ExecuteAlways]
public class Trajectory : MonoBehaviour
{

    public TimeGame startTime;
    public TimeGame endTime;
    public TrajectoryType trajectoryType;
    public Transform[] waypoints;    
    public Transform[] restrictedAreas;
    public float initialControlPointDistance=3f;
    public float lineWidth = 10f;
    public Theme theme;
    private Transform segmentParent;
    private Transform waypointParent;
    private BSplineSegment[] bSplineSegments;            

    public void DeleteAllWaypoints() 
    { 
        waypoints = null;
    }
    public void CreateWaypoint(Vector3 globalPosition)
    {
        if (waypointParent == null)
        {
            GameObject waypointGO = new GameObject("Waypoints");
            waypointGO.transform.parent = transform;
            waypointGO.transform.localPosition = Vector3.zero;
            waypointParent = waypointGO.transform;
        }

        GameObject waypoint = Instantiate(theme.waypointPrefab, globalPosition, Quaternion.identity, waypointParent);        
        AddWaypoint(waypoint.transform);
    }
    public void CreateWaypoint(Vector3 globalPosition, float time_s)
    {
        if (waypointParent == null)
        {
            GameObject waypointGO = new GameObject("Waypoints");
            waypointGO.transform.parent = transform;
            waypointGO.transform.localPosition = Vector3.zero;
            waypointParent = waypointGO.transform;
        }

        GameObject waypoint = Instantiate(theme.waypointPrefab, globalPosition, Quaternion.identity, waypointParent);
        Waypoint wp = waypoint.GetComponent<Waypoint>();
        if (wp != null) 
        {
            wp.time.second = time_s;
        }

        if (startTime == null)
        {
            startTime.second = time_s;
        }
        else
        {
            if (time_s<startTime.second)
            {
                startTime.second = time_s;
            }
        }

        if (endTime == null)
        {
            endTime.second = time_s;
        }
        else
        {
            if (time_s > endTime.second)
            {
                endTime.second = time_s;
            }
        }

        AddWaypoint(waypoint.transform);
    }
    private void AddWaypoint(Transform waypoint)
    {
        Transform[] newWaypoints = new Transform[waypoints.Length + 1];
        for (int i = 0; i < waypoints.Length; i++)          
            newWaypoints[i] = waypoints[i];
        newWaypoints[newWaypoints.Length - 1] = waypoint;       
        waypoints = newWaypoints;
    }
    public void DeleteTrajectory()
    {
        DestroyImmediate(segmentParent.gameObject);
        bSplineSegments = null ;
    }

    public void CreateTrajectory()
    {
        int numSegments = waypoints.Length - 1;
        if (numSegments <= 0) return;
        if (bSplineSegments == null || bSplineSegments.Length != numSegments)
            bSplineSegments = new BSplineSegment[numSegments];

        
        if (segmentParent == null)
        {
            var parentGO = new GameObject("BSplineSegments");
            parentGO.transform.SetParent(transform,false);
            segmentParent = parentGO.transform;
        }

        for (int i = 0; i < numSegments; i++)
        {
            GameObject segmentGO;

            // Varsa mevcut child'ý kullan, yoksa oluþtur
            if (i < segmentParent.childCount)
            {
                segmentGO = segmentParent.GetChild(i).gameObject;
            }
            else
            {
                segmentGO = Instantiate(theme.BSplineSegmentPrefab, segmentParent);                
            }

            // Component’i al, yoksa ekle
            var seg = segmentGO.GetComponent<BSplineSegment>();
            if (seg == null)
                seg = segmentGO.AddComponent<BSplineSegment>();
            seg.initialControlPointDistance = initialControlPointDistance;
            seg.SetStartAndEndWaypoints(waypoints[i].GetComponent<Waypoint>(), waypoints[i + 1].GetComponent<Waypoint>());
            
            seg.restrictedAreas = restrictedAreas;
            seg.CreateControlPoints();            
            seg.GetComponent<LineRenderer>().startWidth = lineWidth;
            seg.GetComponent<LineRenderer>().endWidth = lineWidth;
            bSplineSegments[i] = seg; // burada artýk null olmaz
        }

        for (int i = 0; i < numSegments-1; i++)
        {
            Vector3 delta1 = bSplineSegments[i].startPoint.transform.localPosition - bSplineSegments[i].endPoint.transform.localPosition;
            float len1 = (bSplineSegments[i].endPoint.transform.localPosition - bSplineSegments[i].controlPoint2.transform.localPosition).magnitude;
            Vector3 dir1 = delta1.normalized;

            Vector3 delta2 = bSplineSegments[i + 1].endPoint.transform.localPosition - bSplineSegments[i + 1].startPoint.transform.localPosition;
            float len2 = (bSplineSegments[i + 1].startPoint.transform.localPosition - bSplineSegments[i + 1].controlPoint1.transform.localPosition).magnitude;
            Vector3 dir2 = delta2.normalized;

            Vector3 dirNet = (dir1 + dir2).normalized;

            Vector3 normVec = Vector3.Cross(dir1, dir2).normalized;
            Vector3 controlPointDir1 = Vector3.Cross(dirNet, normVec).normalized;
            Vector3 controlPointDir2 = controlPointDir1 * (-1f);

            bSplineSegments[i].controlPoint2.transform.localPosition = bSplineSegments[i].endPoint.transform.localPosition + len1 * controlPointDir1;
            bSplineSegments[i + 1].controlPoint1.transform.localPosition = bSplineSegments[i].endPoint.transform.localPosition + len2 * controlPointDir2;            
        }

        // Declare Waypoint Relationships
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (i == 0)
            {
                waypoints[i].GetComponent<Waypoint>().type = WaypointType.Open;
                waypoints[i].GetComponent<Waypoint>().controlPoints = new ControlPoint[1];
                waypoints[i].GetComponent<Waypoint>().controlPoints[0] = segmentParent.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1;
                segmentParent.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();
            }
            else if (i == waypoints.Length-1)
            {
                waypoints[i].GetComponent<Waypoint>().type = WaypointType.Open;
                waypoints[i].GetComponent<Waypoint>().controlPoints = new ControlPoint[1];
                waypoints[i].GetComponent<Waypoint>().controlPoints[0] = segmentParent.GetChild(i-1).gameObject.GetComponent<BSplineSegment>().controlPoint2;
                segmentParent.GetChild(i-1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();
            }
            else
            {
                waypoints[i].GetComponent<Waypoint>().type = WaypointType.Close;
                waypoints[i].GetComponent<Waypoint>().controlPoints = new ControlPoint[2];
                waypoints[i].GetComponent<Waypoint>().controlPoints[0] = segmentParent.GetChild(i-1).gameObject.GetComponent<BSplineSegment>().controlPoint2;
                waypoints[i].GetComponent<Waypoint>().controlPoints[1] = segmentParent.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1;

                segmentParent.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();
                segmentParent.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>().waypoint = waypoints[i].GetComponent<Waypoint>();

                segmentParent.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>().pairCP = segmentParent.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>();
                segmentParent.GetChild(i).gameObject.GetComponent<BSplineSegment>().controlPoint1.GetComponent<ControlPoint>().pairCP = segmentParent.GetChild(i - 1).gameObject.GetComponent<BSplineSegment>().controlPoint2.GetComponent<ControlPoint>();
            }
        }      
    }
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