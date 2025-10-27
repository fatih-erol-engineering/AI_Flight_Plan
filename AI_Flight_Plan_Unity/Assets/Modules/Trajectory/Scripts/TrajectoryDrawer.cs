using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class TrajectoryDrawer : MonoBehaviour
{
    [SerializeField] private float segmentLength_m = 10f;
    [field: SerializeField] public TimeGame startTime { get; private set; }
    [field: SerializeField] public TimeGame endTime { get; private set; }
    [field: SerializeField] public WaypointFactory waypointFactory { get; private set; }
    [field: SerializeField] public CreateModeWaypointManager createModeWaypointManager { get; private set; }
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private Transform waypointContainer;
    [SerializeField] private Transform SegmentContainer;
    [SerializeField] private GameObject bSplineDrawerPrefab;
    [SerializeField] private GameObject controlPointPrefab;
    [SerializeField, HideInInspector] private BSplineDrawer[] bSplineDrawerArray;
    [SerializeField, HideInInspector] private bool isReadyToUpdate = false;

    [SerializeField, HideInInspector] private Vector3[] waypointPositions_AfterCreation;

    public void AssignData()
    {
        startTime = waypointContainer.GetChild(0).GetComponent<Waypoint>().time;
        endTime = waypointContainer.GetChild(waypointContainer.childCount - 1).GetComponent<Waypoint>().time;
        int waypointCount = waypointContainer.childCount;
        waypointPositions_AfterCreation = new Vector3[waypointCount];
        for (int i = 0; i < waypointCount; i++)
        {
            waypointPositions_AfterCreation[i] = waypointContainer.GetChild(i).position;
        }
    }
    // void Awake()
    // {
    //     Clear();
    //     AssignData();
    //     Create();
    // }
    void Update()
    {
        if (isReadyToUpdate)
        {
            if (CheckForRecreationNeed())
            {
                Clear();
                Create();
            }

            if (TimeManager.Instance.isUpdated)
            {
                TimeGame startTimeFromTimeManager = new TimeGame(TimeManager.Instance.startTime_s);
                TimeGame endTimeFromTimeManager = new TimeGame(TimeManager.Instance.endTime_s);

                UpdateColorWithTotalTime(startTimeFromTimeManager, endTimeFromTimeManager);
            }
        }
    }
    public void UpdateColorWithTotalTime(TimeGame totalStartTime, TimeGame totalEndTime)
    {

        for (int i = 0; i < bSplineDrawerArray.Length; i++)
        {
            BSplineDrawer bSplineDrawer = bSplineDrawerArray[i];
            float startVal = Mathf.Lerp(0, 1, (bSplineDrawer.startTime.second - startTime.second) / (endTime.second - startTime.second));
            Color _startColor = Color.Lerp(startColor, endColor, startVal);

            float endVal = Mathf.Lerp(0, 1, (bSplineDrawer.endTime.second - startTime.second) / (endTime.second - startTime.second));
            Color _endColor = Color.Lerp(startColor, endColor, endVal);
            bSplineDrawer.SetStartColor(_startColor);
            bSplineDrawer.SetEndColor(_endColor);
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
    public void Tick()
    {
        foreach (BSplineDrawer bSplineDrawer in bSplineDrawerArray)
        {
            bSplineDrawer.Tick();
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

            List<ControlPoint> controlPoints = new List<ControlPoint>();
            for (int j = 0; j < segmentCount - 1; j++)
            {
                // Set waypoints for the segment
                float t = ((float)j + 1f) / segmentCount;
                Vector3 position = Vector3.Lerp(startWaypoint.transform.position, endWaypoint.transform.position, t);
                ControlPoint newControlPoint = Instantiate(controlPointPrefab, position, Quaternion.identity).GetComponent<ControlPoint>();
                controlPoints.Add(newControlPoint);
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
        Tick();
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


    public BSplineDrawer[] GetSegmentDrawers()
    {
        return bSplineDrawerArray;
    }










    public List<CollisionInfo> CheckCollisionWithAnotherTrajectory(TrajectoryDrawer otherTrajectory, float geometricCollisionThreshold_m, float timeCollision_s)
    {
        List<CollisionInfo> totalCollisionInfoList = new List<CollisionInfo>();
        foreach (var segment1 in bSplineDrawerArray)
        {
            foreach (BSplineDrawer segment2 in otherTrajectory.bSplineDrawerArray)
            {
                List<CollisionInfo> currentCollisionInfoList = segment1.CheckCollisionWithAnotherSegment(segment2, geometricCollisionThreshold_m, timeCollision_s);
                totalCollisionInfoList.AddRange(currentCollisionInfoList);
            }
        }
        return totalCollisionInfoList;
    }



}
