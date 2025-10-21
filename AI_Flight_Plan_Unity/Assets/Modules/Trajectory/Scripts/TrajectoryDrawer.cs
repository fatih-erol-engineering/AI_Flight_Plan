using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class TrajectoryDrawer : MonoBehaviour
{
    [SerializeField] private float segmentLength_m = 10f;
    [field: SerializeField] public TimeGame startTime { get; private set; }
    [field: SerializeField] public TimeGame endTime { get; private set; }
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private Transform waypointContainer;
    [SerializeField] private Transform SegmentContainer;
    [SerializeField] private GameObject bSplineDrawerPrefab;
    [SerializeField] private GameObject controlPointPrefab;
    [SerializeField, HideInInspector] private BSplineDrawer[] bSplineDrawerArray;
    [SerializeField, HideInInspector] private bool isReadyToUpdate = false;

    [SerializeField] private Vector3[] waypointPositions_AfterCreation;    

    public void AssignData()
    {
        startTime = waypointContainer.GetChild(0).GetComponent<Waypoint>().time;
        endTime = waypointContainer.GetChild(waypointContainer.childCount - 1).GetComponent<Waypoint>().time;        
    }
    void OnEnable()
    {
        Create();
    }
    void Update()
    {        
        if ( CheckForRecreationNeed())
        {
            Clear();
            Create();
        }
        if (isReadyToUpdate)
        {
            UpdateTrajectory();
        }
    }
    public bool CheckForRecreationNeed()
    {
        int waypointCount = waypointContainer.childCount;        
        for (int i = 0; i < waypointCount; i++)
        {
            float dist = Vector3.Distance(waypointPositions_AfterCreation[i], waypointContainer.GetChild(i).position);
            if (dist > segmentLength_m)
            {
                return true;
            }
        }
        return false;
    }
    public void UpdateTrajectory()
    {
        foreach (BSplineDrawer bSplineDrawer in bSplineDrawerArray)
        {
            bSplineDrawer.UpdateCurve();            
        }
        UpdateColor();
    }
    public void UpdateColor()
    {
        if (bSplineDrawerArray.Length == 0) Debug.LogWarning("No B-Spline drawers available to update color.");
        foreach (BSplineDrawer bSplineDrawer in bSplineDrawerArray)
        {

            float startVal = Mathf.Lerp(0, 1, (bSplineDrawer.startTime.second - startTime.second) / (endTime.second - startTime.second));
            Color _startColor = Color.Lerp(startColor, endColor, startVal);

            float endVal = Mathf.Lerp(0, 1, (bSplineDrawer.endTime.second - startTime.second) / (endTime.second - startTime.second));
            Color _endColor = Color.Lerp(startColor, endColor, endVal);
            bSplineDrawer.SetStartColor(_startColor);
            bSplineDrawer.SetEndColor(_endColor);
        }

    }
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
        startTime = waypointContainer.GetChild(0).GetComponent<Waypoint>().time;
        endTime = waypointContainer.GetChild(waypointCount - 1).GetComponent<Waypoint>().time;
        
        bSplineDrawerArray = new BSplineDrawer[waypointCount - 1];
        for (int i = 0; i < waypointCount - 1; i++)
        {
            Waypoint startWaypoint = waypointContainer.GetChild(i).GetComponent<Waypoint>();
            Waypoint endWaypoint = waypointContainer.GetChild(i + 1).GetComponent<Waypoint>();

            float distance = Vector3.Distance(startWaypoint.transform.position, endWaypoint.transform.position);
            int segmentCount = Mathf.CeilToInt(distance / segmentLength_m);

            GameObject segmentObj = Instantiate(bSplineDrawerPrefab, SegmentContainer);
            BSplineDrawer bSplineDrawer = segmentObj.GetComponent<BSplineDrawer>();

            ControlPoint[] controlPoints = new ControlPoint[segmentCount - 1];
            for (int j = 0; j < segmentCount - 1; j++)
            {
                // Set waypoints for the segment
                float t = ((float)j + 1f) / segmentCount;
                Vector3 position = Vector3.Lerp(startWaypoint.transform.position, endWaypoint.transform.position, t);
                ControlPoint newControlPoint = Instantiate(controlPointPrefab, position, Quaternion.identity).GetComponent<ControlPoint>();
                controlPoints[j] = newControlPoint;
            }

            bSplineDrawer.SetStartWaypoint(startWaypoint);
            bSplineDrawer.SetEndWaypoint(endWaypoint);
            bSplineDrawer.SetControlPoints(controlPoints);
            bSplineDrawer.Create();
            bSplineDrawerArray[i] = bSplineDrawer;
        }

        waypointPositions_AfterCreation = new Vector3[waypointCount];
        for (int i = 0; i < waypointCount; i++)
        {
            waypointPositions_AfterCreation[i] = waypointContainer.GetChild(i).position;
        }

        isReadyToUpdate = true;
    }

    // Update is called once per frame
    public void Clear()
    {
        bSplineDrawerArray = new BSplineDrawer[0];
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
        isReadyToUpdate = false;
    }

}
