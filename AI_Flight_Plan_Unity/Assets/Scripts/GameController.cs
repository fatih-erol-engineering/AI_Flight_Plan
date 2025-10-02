using CesiumForUnity;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

[ExecuteAlways]
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

    private void Start()
    {
        cam = Camera.main;
        mode = Mode.Object_Mode;
        objectSelector = gameObject.GetComponent<ObjectSelector>();
        mapPopupSpawner = gameObject.GetComponent<MapPopupSpawner>();
        aircraftFactory = gameObject.GetComponent<AircraftFactory>();

        EnableCesiumControls(false);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {

            nextState = true;
            testState++;

            if (testState > 3)
            {

                testState = 0;

            }

        }

        if (nextState)
        {

            if (testState == 0)
            {

                if (TryScreenToWorld(Input.mousePosition, out var hitPos))
                {
                    selectedAircarft = aircraftFactory.Spawn(AircraftModel.Mavic_Pro, hitPos, Quaternion.Euler(0, 0, 0));
                    selectedAircarft.CreateWaypoint(selectedAircarft.transform.position);
                }
                mapPopupSpawner.StartWaypointInfo(selectedAircarft);

            }


            if (testState == 1)
            {
                
                if (TryScreenToWorld(Input.mousePosition, out var hitPos))
                {
                    selectedAircarft.CreateWaypoint(hitPos); 
                }   
                
            }


            if (testState == 2)
            {


                if (TryScreenToWorld(Input.mousePosition, out var hitPos))
                {
                    selectedAircarft.CreateWaypoint(hitPos);
                }   
                

            }


            if (testState == 3)
            {

                if (TryScreenToWorld(Input.mousePosition, out var hitPos))
                {
                    selectedAircarft.CreateWaypoint(hitPos);
                    selectedAircarft.trajectory.CreateTrajectory();
                }   

            }

        }

        if (testState == 0)
        {
            mapPopupSpawner.UpdateWaypointInfo();
        }












        nextState = false;
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
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

}



public enum Mode
{
    Travel_Mode,
    Object_Mode,
    Create_Trajectory,
    Edit_Trajectory,
    Train_AI
}
