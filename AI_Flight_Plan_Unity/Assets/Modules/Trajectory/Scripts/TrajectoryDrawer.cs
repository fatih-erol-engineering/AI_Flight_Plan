using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class TrajectoryDrawer : MonoBehaviour
{
    [SerializeField, HideInInspector] private float segmentLength_m = 10f;
    [SerializeField] private Transform waypointContainer;
    [SerializeField] private Transform SegmentContainer;
    [SerializeField] private GameObject bSplineDrawerPrefab;
    [SerializeField] private GameObject waypointPrefab;

    public void Create()
    {
        if (waypointContainer == null || bSplineDrawerPrefab == null)
        {
            Debug.LogError("WaypointContainer or BSplineDrawerPrefab is not assigned.");
            return;
        }

        int waypointCount = waypointContainer.childCount;
        if (waypointCount < 2)
        {
            Debug.LogError(" At least 2 waypoints are required to create trajectory.");
            return;
        }

        // her waypoint arasını eğer segmentLength_m ten büyükse böl
        for (int i = 0; i < waypointCount - 1; i++)
        {
            Transform startWaypoint = waypointContainer.GetChild(i);
            Transform endWaypoint = waypointContainer.GetChild(i + 1);

            float distance = Vector3.Distance(startWaypoint.position, endWaypoint.position);
            int segmentCount = Mathf.CeilToInt(distance / segmentLength_m);

            List<Waypoint> segmentWaypoints = new List<Waypoint>();
            GameObject segmentObj = Instantiate(bSplineDrawerPrefab, SegmentContainer);
            BSplineDrawer bSplineDrawer = segmentObj.GetComponent<BSplineDrawer>();
            Waypoint _startWaypoint = Instantiate(startWaypoint.GetComponent<Waypoint>(), startWaypoint.position, startWaypoint.rotation, bSplineDrawer.waypointContainer);
            segmentWaypoints.Add(_startWaypoint);
            for (int j = 1; j < segmentCount; j++)
            {
                // Set waypoints for the segment
                float t = (float)j / segmentCount;
                Vector3 position = Vector3.Lerp(startWaypoint.position, endWaypoint.position, t);
                Waypoint newWaypoint = Instantiate(waypointPrefab, position, Quaternion.identity, bSplineDrawer.waypointContainer).GetComponent<Waypoint>();
                segmentWaypoints.Add(newWaypoint);
                
            }
            Waypoint _endWaypoint = Instantiate(endWaypoint.GetComponent<Waypoint>(), endWaypoint.position, endWaypoint.rotation, bSplineDrawer.waypointContainer);
            segmentWaypoints.Add(_endWaypoint);
            bSplineDrawer.SetWaypoints(segmentWaypoints);
            bSplineDrawer.Create();
        }             
    }

    // Update is called once per frame
    public void Clear()
    {
        for (int i = SegmentContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = SegmentContainer.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

}
