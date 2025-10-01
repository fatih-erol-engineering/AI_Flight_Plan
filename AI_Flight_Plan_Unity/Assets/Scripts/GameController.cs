using CesiumForUnity;
using UnityEditor.SearchService;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ObjectSelector))]
[RequireComponent(typeof(MapPopupSpawner))]

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
    
    private void Awake()
    {
        cam = Camera.main;
        mode = Mode.Object_Mode;
        objectSelector = gameObject.GetComponent<ObjectSelector>();
        mapPopupSpawner = gameObject.GetComponent<MapPopupSpawner>();
        EnableCesiumControls(false);

    }

    void Update()
    {

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


        switch (mode)
        {
            case Mode.Travel_Mode:                                    
                break;
            case Mode.Object_Mode:
                    objectSelector.UpdateCycle();
                break;
            case Mode.Edit_Trajectory:
                break;
            case Mode.Create_Trajectory:                
                //Control_with_Create_Trajectory_Mode();
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
            activeTrajectory.GetComponent<Trajectory>().AddWaypoint(activeWaypoint.transform);
        }
        if (Input.GetMouseButtonDown(1)) 
        {
            activeTrajectory =  Spawn_Prefab_with_Raycast(trajectoryPrefab);
        }
        if (Input.GetKey(KeyCode.Space)) 
        {
            activeTrajectory.GetComponent<Trajectory>().CreateTrajectory();
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
            Vector3 pos = hit.point + new Vector3(0f, 5f, 0f);
            instantiatedGO = Instantiate(prefab, pos, Quaternion.identity);
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
            Vector3 pos = hit.point + new Vector3(0f, 5f, 0f);
            instantiatedGO = Instantiate(prefab, pos, Quaternion.identity, parent);
        }        
        return instantiatedGO;
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
