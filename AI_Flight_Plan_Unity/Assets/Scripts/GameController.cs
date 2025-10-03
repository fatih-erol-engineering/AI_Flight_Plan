using CesiumForUnity;
using System;
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

    private void Start()
    {
        cam = Camera.main;
        objectSelector = gameObject.GetComponent<ObjectSelector>();
        mapPopupSpawner = gameObject.GetComponent<MapPopupSpawner>();
        aircraftFactory = gameObject.GetComponent<AircraftFactory>();
        mode = Mode.Free_Mode;
        EnableCesiumControls(false);
    }
    public void Deneme()
    {
        Debug.Log("Denendi.");
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

    void Control_with_Create_Trajectory_Mode()
    {
        //if (Input.GetMouseButtonDown(0)) 
        //{
        //    if (activeTrajectory == null)
        //    {
        //        activeTrajectory = Spawn_Prefab_with_Raycast(trajectoryPrefab);
        //    }
        //    activeWaypoint = Spawn_Prefab_with_Raycast(waypointPrefab, activeTrajectory.transform);
        //    activeTrajectory.GetComponent<Trajectory>().AddWaypoint(activeWaypoint.transform);
        //}
        //if (Input.GetMouseButtonDown(1)) 
        //{
        //    activeTrajectory =  Spawn_Prefab_with_Raycast(trajectoryPrefab);
        //}
        //if (Input.GetKey(KeyCode.Space)) 
        //{
        //    activeTrajectory.GetComponent<Trajectory>().CreateTrajectory();
        //}
    }



    public void EnableCesiumControls(bool on)
    {
        if (cesiumController) cesiumController.enabled = on;

        // (Ýsteðe baðlý) imleç kilidini de yönetmek isteyebilirsiniz:
        UnityEngine.Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        UnityEngine.Cursor.visible = !on;
    }

    //void Update_in_Create_Aircraft()
    //{
    //    flag_Create_Aircraft = false;
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        flag_Create_Aircraft = true;
    //    }

    //    if (flag_Create_Aircraft)
    //    {
    //        Vector3 hitPos = MouseHitPos();
    //        selectedAircarft = aircraftFactory.Spawn(AircraftModel.Mavic_Pro, hitPos, Quaternion.Euler(0, 0, 0));
    //        Vector3 altitudeOffset = new Vector3(0f, 5f, 0f);
    //        selectedWaypoint = selectedAircarft.CreateWaypoint(selectedAircarft.transform.position + altitudeOffset);
    //        mapPopupSpawner.StartWaypointInfo(selectedAircarft);
    //        mode = Mode.Create_Waypoint;
    //    }
    //}
    //void Update_in_Create_Waypoint()
    //{
    //    flag_Create_Waypoint = false;
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        flag_Create_Waypoint = true;
    //    }
    //    mapPopupSpawner.UpdateWaypointInfo();
    //    if (flag_Create_Waypoint)
    //    {
    //        Vector3 hitPos = MouseHitPos();
    //        Vector3 altitudeOffset = new Vector3(0f, 5f, 0f);
    //        selectedWaypoint = selectedAircarft.CreateWaypoint(hitPos+ altitudeOffset) ;
    //    }
    //    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
    //    {
    //        selectedAircarft.trajectory.CreateTrajectory();
    //        mode = Mode.Free_Mode;
    //    }
    //}

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
