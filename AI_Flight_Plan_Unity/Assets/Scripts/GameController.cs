using CesiumForUnity;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ObjectSelector))]
[RequireComponent(typeof(MapPopupSpawner))]
[RequireComponent(typeof(AircraftFactory))]

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

    private AircraftFactory aircraftFactory;
    public Aircraft selectedAircarft;
    public Waypoint selectedWaypoint;
    private bool flag_Create_Aircraft;
    private bool flag_Create_Waypoint;
    private bool flag_Edit_Waypoint;
    private bool flag_Mode_Change;

    private void Start()
    {
        cam = Camera.main;
        objectSelector = gameObject.GetComponent<ObjectSelector>();
        mapPopupSpawner = gameObject.GetComponent<MapPopupSpawner>();
        aircraftFactory = gameObject.GetComponent<AircraftFactory>();
        mode = Mode.Create_Aircraft;
        EnableCesiumControls(false);
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.Free_Mode:
                Update_in_Free_Mode();
                break;
            case Mode.Create_Aircraft:
                Update_in_Create_Aircraft();
                break;
            case Mode.Create_Waypoint:
                Update_in_Create_Waypoint();
                break;
            case Mode.Edit_Waypoint:
                Update_in_Edit_Waypoint();
                break;
            default:
                break;
        }

        //if (Input.GetMouseButtonDown(0))
        //{

        //    nextState = true;
        //    testState++;

        //}
        //if (Input.GetKeyDown(KeyCode.Return))
        //{

        //    nextState = true;
        //    testState = -1;

        //}

        //if (nextState)
        //{

        //    if (testState == 0)
        //    {

        //        if (TryScreenToWorld(Input.mousePosition, out var hitPos))
        //        {
        //            selectedAircarft = aircraftFactory.Spawn(AircraftModel.Mavic_Pro, hitPos, Quaternion.Euler(0, 0, 0));
        //            selectedWaypoint = selectedAircarft.CreateWaypoint(selectedAircarft.transform.position);
        //        }
        //        mapPopupSpawner.StartWaypointInfo(selectedAircarft);
        //    }


        //    else if (testState == -1) // Finish and Create Traj
        //    {
        //        selectedAircarft.trajectory.CreateTrajectory();                                
        //    }


        //    else // Waypoint Loop
        //    {                
        //        if (createWayPointIdx == 0)
        //        {
        //            Vector3 mousePos = MouseHitPos();
        //            selectedWaypoint = selectedAircarft.trajectory.CreateWaypoint(mousePos);
        //            createWayPointIdx=1;
        //        }
        //        else if(createWayPointIdx == 1)
        //        {
        //            // Edit Popup
        //            selectedWaypoint.setPosition(new Vector3(mapPopupSpawner.fieldX_m.value, mapPopupSpawner.fieldY_m.value, mapPopupSpawner.fieldZ_m.value));
        //            Debug.Log("Noliy");
        //            createWayPointIdx = 0;
        //        }                

        //    }

        //}

        //if (testState != -1)
        //{
        //    if (createWayPointIdx == 0)
        //    {
        //        mapPopupSpawner.UpdateWaypointInfo();
        //    }
        //}












        //nextState = false;
        //if (!cesiumController.enabled)
        //{                
        //    EnableCesiumControls(true);
        //    mode = Mode.Travel_Mode;
        //    Debug.Log("Travel Mode Activated.");
        //}
        //else
        //{
        //    EnableCesiumControls(false);
        //    mode = Mode.Create_Trajectory;
        //    Debug.Log("Create Trajectory Mode Activated.");
        //}


        //switch (mode)
        //{
        //    case Mode.Travel_Mode:                                    
        //        break;
        //    case Mode.Object_Mode:
        //            objectSelector.UpdateCycle();
        //        break;
        //    case Mode.Edit_Trajectory:
        //        break;
        //    case Mode.Create_Trajectory:                
        //        //Control_with_Create_Trajectory_Mode();
        //        break;
        //    case Mode.Train_AI:
        //        break;            
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

    void Update_in_Create_Aircraft()
    {
        flag_Create_Aircraft = false;
        if (Input.GetMouseButtonDown(0))
        {
            flag_Create_Aircraft = true;
        }

        if (flag_Create_Aircraft)
        {
            Vector3 hitPos = MouseHitPos();
            selectedAircarft = aircraftFactory.Spawn(AircraftModel.Mavic_Pro, hitPos, Quaternion.Euler(0, 0, 0));
            Vector3 altitudeOffset = new Vector3(0f, 5f, 0f);
            selectedWaypoint = selectedAircarft.CreateWaypoint(selectedAircarft.transform.position + altitudeOffset);
            mapPopupSpawner.StartWaypointInfo(selectedAircarft);
            mode = Mode.Create_Waypoint;
        }
    }
    void Update_in_Create_Waypoint()
    {
        flag_Create_Waypoint = false;
        if (Input.GetMouseButtonDown(0))
        {
            flag_Create_Waypoint = true;
        }
        mapPopupSpawner.UpdateWaypointInfo();
        if (flag_Create_Waypoint)
        {
            Vector3 hitPos = MouseHitPos();
            Vector3 altitudeOffset = new Vector3(0f, 5f, 0f);
            selectedWaypoint = selectedAircarft.CreateWaypoint(hitPos+ altitudeOffset) ;
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            selectedAircarft.trajectory.CreateTrajectory();
            mode = Mode.Free_Mode;
        }
    }
    void Update_in_Edit_Waypoint()
    {
        //flag_Edit_Waypoint = false;
        //mapPopupSpawner.fieldY_m.Focus();
        //if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        //{            
        //    Vector3 globalPosition = new Vector3(mapPopupSpawner.fieldX_m.value, mapPopupSpawner.fieldY_m.value, mapPopupSpawner.fieldZ_m.value);
        //    selectedWaypoint.setPosition(globalPosition, mapPopupSpawner.fieldTime_s.value);
        //    mode = Mode.Create_Waypoint;
        //}


    }
    void Update_in_Free_Mode()
    {

    }
}



public enum Mode
{
    Free_Mode,
    Create_Aircraft,
    Create_Waypoint,
    Edit_Waypoint,    
    Create_Trajectory
}
