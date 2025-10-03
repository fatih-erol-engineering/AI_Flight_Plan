using CesiumForUnity;
using System;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ObjectSelector))]
[RequireComponent(typeof(MapPopupSpawner))]
[RequireComponent(typeof(AircraftFactory))]
[RequireComponent(typeof(TimeManager))]

public class GameController : MonoBehaviour
{

    public Mode mode;
    [Tooltip("Týklanýnca çýkacak obje (Prefab)")]
    public GameObject waypointPrefab;
    public GameObject trajectoryPrefab;
    private GameObject activeWaypoint;
    private GameObject activeTrajectory;

    [Tooltip("Hangi katmanlara raycast yapýlsýn")]
    public LayerMask hitMask = ~0; // default: her þey

    public CesiumCameraController cesiumController; // Kameradaki bileþeni atayýn
    [HideInInspector]
    public ObjectSelector objectSelector;
    public MapPopupSpawner mapPopupSpawner;

    private Camera cam;
    private bool nextState;
    private int testState = -1;

    public AircraftFactory aircraftFactory;
    public Aircraft selectedAircarft;
    public Waypoint selectedWaypoint;
    public AircraftModel selectedAircraftModel;

    [Header("Time")]
    public TimeManager timeManager;
    public bool colorTrajWithTime = true;

    private void Start()
    {
        cam = Camera.main;
        objectSelector = gameObject.GetComponent<ObjectSelector>();
        mapPopupSpawner = gameObject.GetComponent<MapPopupSpawner>();
        aircraftFactory = gameObject.GetComponent<AircraftFactory>();
        mode = Mode.Free_Mode;
        EnableCesiumControls(false);
    }

    void Update()
    {
        //mapPopupSpawner.ctxRoot.style.visibility = Visibility.Visible;
        //mapPopupSpawner.ctxRoot.style.display = DisplayStyle.Flex;
        //mapPopupSpawner.ctxRoot.style.position = Position.Absolute;
        switch (mode)
        {
            case Mode.Free_Mode:
                mapPopupSpawner.Update_in_Free_Mode();
                break;
            case Mode.Select_Aircraft_Projected_Position:
                mapPopupSpawner.Update_in_Select_Aircraft_Projected_Position();
                break;
            case Mode.Select_Aircraft_Altitude_and_Time:
                mapPopupSpawner.Update_in_Select_Aircraft_Altitude_and_Time();
                break;
            case Mode.Create_Aircraft:
                mapPopupSpawner.Update_in_Create_Aircraft();
                break;
            case Mode.Select_Waypoint_Projected_Position:
                mapPopupSpawner.Update_in_Select_Waypoint_Projected_Position();
                break;
            case Mode.Select_Waypoint_Altitude_and_Time:
                mapPopupSpawner.Update_in_Select_Waypoint_Altitude_and_Time();
                break;
            case Mode.Create_Waypoint:
                //mapPopupSpawner.Update_in_Create_Waypoint();
                break;
            case Mode.Create_Trajectory:
                mapPopupSpawner.Update_in_Create_Trajectory();
                break;
            case Mode.Edit_Aircraft:
                //mapPopupSpawner.Update_in_Edit_Aircraft();
                break;
            case Mode.Edit_Waypoint:
                //mapPopupSpawner.Update_in_Edit_Waypoint();
                break;
            case Mode.Edit_Trajectory:
                //mapPopupSpawner.Update_in_Edit_Trajectory();
                break;
        }

        // Update Trajectory Colors
        //if (colorTrajWithTime)
        //{
        //    if (aircraftFactory.aircraftList != null)
        //    {                            
        //        foreach (Aircraft aircraft in aircraftFactory.aircraftList)
        //        {
        //            if ((aircraft.trajectory!=null)&& (aircraft.trajectory.bSplineSegments != null))
        //            { 
        //                foreach (BSplineSegment segment in aircraft.trajectory.bSplineSegments)
        //                {
        //                    float globalTimeInterval_s = (timeManager.endTime_s - timeManager.startTime_s);

        //                    float lerpValStart = segment.startTime.second / globalTimeInterval_s;
        //                    Color segmentStartColor = Color.Lerp(Color.blue, Color.red, lerpValStart);
        //                    segment.lr.startColor = segmentStartColor;

        //                    float lerpValEnd = segment.endTime.second / globalTimeInterval_s;
        //                    Color segmentEndColor = Color.Lerp(Color.blue, Color.red, lerpValEnd);
        //                    segment.lr.endColor = segmentEndColor;
        //                }
        //            }
        //        }
        //    }
        //}

    }
    public Vector3 MouseHitPos()
    {
        TryScreenToWorld(Input.mousePosition, out Vector3 hitPos);
        return hitPos;
    }
    bool TryScreenToWorld(Vector2 screen, out Vector3 world)
    {
        var ray = cam.ScreenPointToRay(screen);
        if (Physics.Raycast(ray, out var hit, 100000f, hitMask, QueryTriggerInteraction.Collide))
        {
            world = hit.point;
            return true;
        }
        else
        {
            var plane = new Plane(Vector3.up, new Vector3(0, 0, 0));
            if (plane.Raycast(ray, out float enter))
            {
                world = ray.GetPoint(enter);
                return true;
            }
        }
        world = default;
        return false;
    }

 

    public void EnableCesiumControls(bool on)
    {
        if (cesiumController) cesiumController.enabled = on;

        // (Ýsteðe baðlý) imleç kilidini de yönetmek isteyebilirsiniz:
        UnityEngine.Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        UnityEngine.Cursor.visible = !on;
    }
    
}



public enum Mode
{
    Free_Mode,

    Select_Aircraft_Projected_Position,
    Select_Aircraft_Altitude_and_Time,
    Create_Aircraft,

    Select_Waypoint_Projected_Position,
    Select_Waypoint_Altitude_and_Time,
    Create_Waypoint,

    Create_Trajectory,

    Edit_Aircraft,
    Edit_Waypoint,    
    Edit_Trajectory,
    
}
