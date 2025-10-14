using UnityEngine;

public class WaypointFactory : MonoBehaviour
{                       
    public GameObject waypointPrefab { get; private set; }    
    public void Init()
    {
        AssignData();
    }
    private void AssignData()
    {
                
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }    


    // public virtual Waypoint Spawn(Aircraft aircraft)
    // {
    //     waypointContainer = aircraft.trajectory.transform.Find("WaypointContainer");
    //     if (waypointContainer == null)
    //     {
    //         GameObject waypointContainerObj = new GameObject("WaypointContainer");
    //         waypointContainerObj.transform.parent = aircraft.trajectory.transform;
    //         waypointContainerObj.transform.localPosition = Vector3.zero;
    //         waypointContainer = waypointContainerObj.transform;
    //     }                

    //     var go = Instantiate(waypointPrefab, waypointContainer);
    //     var ctrl = go.GetComponent<Waypoint>();
    //     CheckAssignment(ctrl);
                        
    //     return ctrl;
    // }
    // public virtual Waypoint Spawn(Aircraft aircraft, Vector3 globalPosition, Quaternion globalRotation)
    // {
    //     waypointContainer = aircraft.trajectory.transform.Find("WaypointContainer");
    //     if (waypointContainer == null)
    //     {
    //         GameObject waypointContainerObj = new GameObject("WaypointContainer");
    //         waypointContainerObj.transform.parent = aircraft.trajectory.transform;
    //         waypointContainerObj.transform.localPosition = Vector3.zero;
    //         waypointContainer = waypointContainerObj.transform;
    //     }                
        
    //     var go = Instantiate(waypointPrefab, globalPosition, globalRotation, waypointContainer);
    //     var ctrl = go.GetComponent<Waypoint>();
    //     CheckAssignment(ctrl);
                        
    //     return ctrl;
    // }

    // public virtual Waypoint Spawn(Aircraft aircraft, string parentName)
    // {
    //     Transform waypointContainerForPreCreate = aircraft.trajectory.transform.Find(parentName);
    //     if (waypointContainerForPreCreate == null)
    //     {
    //         GameObject waypointContainerObj = new GameObject(parentName);
    //         waypointContainerObj.transform.parent = aircraft.trajectory.transform;
    //         waypointContainerObj.transform.localPosition = Vector3.zero;
    //         waypointContainer = waypointContainerObj.transform;
    //     }                
        
    //     var go = Instantiate(waypointPrefab, waypointContainerForPreCreate);
    //     var ctrl = go.GetComponent<Waypoint>();
    //     CheckAssignment(ctrl);
                        
    //     return ctrl;
    // }

    // public void SelectWaypoint(Waypoint waypoint)
    // {

    //     selectedWaypoint = waypoint;
    //     CheckAssignment(selectedWaypoint);        
    //     // uIManager.UpdateSelectedAircraftInfoPanel(aircraft);
    // }
    // public void Delete()
    // {
    //     Destroy(waypointContainer.gameObject);        
    // }
    // public void Clear(Transform waypointContainer)
    // {
    //     if (waypointContainer != null)
    //     {
    //         for (int i = 0; i < waypointContainer.childCount; i++)
    //         {
    //             Destroy(waypointContainer.GetChild(i).gameObject);
    //         }
    //     }
    // }
}