using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[ExecuteAlways]
public class TrajectoryDrawer : MonoBehaviour
{
    
    public Transform[] waypoints;    
    public Transform[] restrictedAreas;
    public float initialControlPointDistance=3f;
    private Transform segmentParent;
    private BSplineSegment[] bSplineSegments;
    private Transform[,] controlPointPairs;
    private Transform selectedControlPoint;
    private Transform prev_selectedControlPoint;
    private Transform pairOfSelectedControlPoint;
    private Transform selectedWaypoint;
    private Transform prev_SelectedWaypoint;
    private Vector3 prev_WaypointPos;
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
                segmentGO = new GameObject($"B Segment {i}");
                segmentGO.transform.SetParent(segmentParent, false);
            }

            // Component’i al, yoksa ekle
            var seg = segmentGO.GetComponent<BSplineSegment>();
            if (seg == null)
                seg = segmentGO.AddComponent<BSplineSegment>();
            seg.initialControlPointDistance = initialControlPointDistance;
            seg.startPoint = waypoints[i].GetComponent<Waypoint>();
            seg.endPoint = waypoints[i+1].GetComponent<Waypoint>();

            seg.restrictedAreas = restrictedAreas;
            seg.CreateControlPoints();
 
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
    void MoveWaypoint(Transform waypoint)
    {
        selectedWaypoint = waypoint;
        if (prev_SelectedWaypoint != selectedWaypoint) 
        { 
            for (int i = 0; i < controlPointPairs.GetLength(0); i++)
            {
                if (waypoint == controlPointPairs[i, 2])
                {
                    pairOfSelectedControlPoint = controlPointPairs[i, 1];
                    selectedControlPoint = controlPointPairs[i, 0];
                    break;
                }
            }
        }
        if ((prev_WaypointPos != null) && (prev_SelectedWaypoint == selectedWaypoint))
        {            
            Vector3 deltaPos = waypoint.localPosition - prev_WaypointPos;
            selectedControlPoint.localPosition += deltaPos;
            pairOfSelectedControlPoint.localPosition += deltaPos;
        }

        prev_WaypointPos = waypoint.localPosition;
        prev_SelectedWaypoint = waypoint;
    }

    void MoveControlPoint(Transform controlPoint)
    {
        selectedControlPoint = controlPoint;
        if (prev_selectedControlPoint != selectedControlPoint)
        {                    
            selectedControlPoint = controlPoint;            
                if (prev_selectedControlPoint != selectedControlPoint)
                {
                    for (int i = 0; i < controlPointPairs.GetLength(0); i++)
                    {
                        if (selectedControlPoint == controlPointPairs[i, 0])
                        {
                            pairOfSelectedControlPoint = controlPointPairs[i, 1];
                            selectedWaypoint = controlPointPairs[i, 2];
                            break;
                        }
                        else if (selectedControlPoint == controlPointPairs[i, 1])
                        {
                            pairOfSelectedControlPoint = controlPointPairs[i, 0];
                            selectedWaypoint = controlPointPairs[i, 2];
                            break;
                        }
                        else
                        {
                            pairOfSelectedControlPoint = null;
                        }
                    }
                }
        }
        if (pairOfSelectedControlPoint != null)
        {
            Vector3 dir = selectedControlPoint.localPosition - selectedWaypoint.localPosition;
            pairOfSelectedControlPoint.localPosition = selectedWaypoint.localPosition + dir.normalized * (-1f) * (selectedWaypoint.localPosition - pairOfSelectedControlPoint.localPosition).magnitude;            
        }
        prev_selectedControlPoint = selectedControlPoint;
    }
}
