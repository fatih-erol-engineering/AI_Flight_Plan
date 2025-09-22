using UnityEngine;

public class TrajectoryDrawer : MonoBehaviour
{
    
    public Transform[] waypoints;    
    public Transform[] restrictedAreas;
    private Transform segmentParent;
    private BSplineSegment[] bSplineSegments;
    
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

            seg.startPoint = waypoints[i];
            seg.endPoint = waypoints[i+1];
            seg.restrictedAreas = restrictedAreas;
            seg.CreateControlPoints();            
            bSplineSegments[i] = seg; // burada artýk null olmaz
        }
        for (int i = 0; i < numSegments; i++)
        {
            Vector3 delta1 = bSplineSegments[i].startPoint.localPosition - bSplineSegments[i].endPoint.localPosition;
            float len1 = (bSplineSegments[i].endPoint.localPosition - bSplineSegments[i].controlPoint2.localPosition).magnitude;
            Vector3 dir1 = delta1.normalized;

            Vector3 delta2 = bSplineSegments[i + 1].endPoint.localPosition - bSplineSegments[i + 1].startPoint.localPosition;
            float len2 = (bSplineSegments[i + 1].startPoint.localPosition - bSplineSegments[i + 1].controlPoint1.localPosition).magnitude;
            Vector3 dir2 = delta2.normalized;

            Vector3 dirNet = (dir1 + dir2).normalized;

            Vector3 normVec = Vector3.Cross(dir1, dir2).normalized;
            Vector3 controlPointDir1 = Vector3.Cross(dirNet, normVec).normalized;
            Vector3 controlPointDir2 = controlPointDir1 * (-1f);

            bSplineSegments[i].controlPoint2.localPosition = bSplineSegments[i].endPoint.localPosition + len1 * controlPointDir1;
            bSplineSegments[i + 1].controlPoint1.localPosition = bSplineSegments[i].endPoint.localPosition + len2 * controlPointDir2;
        }

    }

}
