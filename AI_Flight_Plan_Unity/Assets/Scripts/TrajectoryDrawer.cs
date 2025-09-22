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


    }
}
