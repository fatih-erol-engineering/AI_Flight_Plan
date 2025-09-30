using UnityEngine;

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

    private Camera cam;
    
    private void Awake()
    {
        cam = Camera.main;
        mode = Mode.Create_Trajectory;
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.Object_Mode:
                break;
            case Mode.Edit_Trajectory:
                break;
            case Mode.Create_Trajectory:
                Control_with_Create_Trajectory_Mode();
                break;
            case Mode.Train_AI:
                break;            
        }

    }

    void Control_with_Create_Trajectory_Mode()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            if (activeTrajectory == null)
            {
                activeTrajectory = Spawn_Prefab_with_Raycast(trajectoryPrefab);
            }
            activeWaypoint = Spawn_Prefab_with_Raycast(waypointPrefab, activeTrajectory.transform);
            activeTrajectory.GetComponent<TrajectoryDrawer>().AddWaypoint(activeWaypoint.transform);
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            activeTrajectory =  Spawn_Prefab_with_Raycast(trajectoryPrefab);
        }
        if (Input.GetKey(KeyCode.Space)) 
        {
            activeTrajectory.GetComponent<TrajectoryDrawer>().CreateTrajectory();
        }

    }


    private GameObject Spawn_Prefab_with_Raycast(GameObject prefab)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject instantiatedGO = null;
        if (Physics.Raycast(ray, out hit, 1000f, hitMask))
        {
            // Týklanan noktada prefab spawn et
            instantiatedGO = Instantiate(prefab, hit.point, Quaternion.identity);
        }
        return instantiatedGO;
    }

    private GameObject Spawn_Prefab_with_Raycast(GameObject prefab,Transform parent)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject instantiatedGO = null;
        if (Physics.Raycast(ray, out hit, 1000f, hitMask))
        {
            // Týklanan noktada prefab spawn et
            instantiatedGO = Instantiate(prefab, hit.point, Quaternion.identity, parent);
        }        
        return instantiatedGO;
    }
}



public enum Mode
{
    Object_Mode,
    Create_Trajectory,
    Edit_Trajectory,
    Train_AI
}
