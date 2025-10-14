using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(MainGameManager))]
[RequireComponent(typeof(UIManager))]
public class WaypointFactory : MonoBehaviour
{   
    public MainGameManager mainGameManager { get; private set; } 
    public UIManager uIManager { get; private set; }
    private AircraftFactory aircraftFactory;


    // Assigned from GameManager
    public Theme theme { get; private set; }
    [HideInInspector]
    public Transform waypointContainer;    
    public Transform waypointContainerForPreCreate;    
    public GameObject waypointPrefab { get; private set; }
    public Waypoint selectedWaypoint { get; private set; }        

    public void Init()
    {
        AssignData();
    }
    private void AssignData()
    {
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        CheckAssignment(mainGameManager);

        if (!uIManager) uIManager = GetComponent<UIManager>();
        CheckAssignment(uIManager);

        if (!theme) theme = mainGameManager.theme;
        CheckAssignment(theme);

        if (!waypointPrefab) waypointPrefab = theme.waypointPrefab;
        CheckAssignment(waypointPrefab);
        
        if (!aircraftFactory) aircraftFactory = mainGameManager.aircraftFactory;
        CheckAssignment(aircraftFactory);

        if (!waypointContainer) waypointContainer = aircraftFactory.selectedAircraft.trajectory.waypointContainer;
        CheckAssignment(waypointContainer);        
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }    


    public virtual Waypoint Spawn(Aircraft aircraft)
    {
        waypointContainer = aircraft.trajectory.transform.Find("WaypointContainer");
        if (waypointContainer == null)
        {
            GameObject waypointContainerObj = new GameObject("WaypointContainer");
            waypointContainerObj.transform.parent = aircraft.trajectory.transform;
            waypointContainerObj.transform.localPosition = Vector3.zero;
            waypointContainer = waypointContainerObj.transform;
        }                

        var go = Instantiate(waypointPrefab, waypointContainer);
        var ctrl = go.GetComponent<Waypoint>();
        CheckAssignment(ctrl);
                        
        return ctrl;
    }
    public virtual Waypoint Spawn(Aircraft aircraft, Vector3 globalPosition, Quaternion globalRotation)
    {
        waypointContainer = aircraft.trajectory.transform.Find("WaypointContainer");
        if (waypointContainer == null)
        {
            GameObject waypointContainerObj = new GameObject("WaypointContainer");
            waypointContainerObj.transform.parent = aircraft.trajectory.transform;
            waypointContainerObj.transform.localPosition = Vector3.zero;
            waypointContainer = waypointContainerObj.transform;
        }                
        
        var go = Instantiate(waypointPrefab, globalPosition, globalRotation, waypointContainer);
        var ctrl = go.GetComponent<Waypoint>();
        CheckAssignment(ctrl);
                        
        return ctrl;
    }

    public virtual Waypoint Spawn(Aircraft aircraft, string parentName)
    {
        Transform waypointContainerForPreCreate = aircraft.trajectory.transform.Find(parentName);
        if (waypointContainerForPreCreate == null)
        {
            GameObject waypointContainerObj = new GameObject(parentName);
            waypointContainerObj.transform.parent = aircraft.trajectory.transform;
            waypointContainerObj.transform.localPosition = Vector3.zero;
            waypointContainer = waypointContainerObj.transform;
        }                
        
        var go = Instantiate(waypointPrefab, waypointContainerForPreCreate);
        var ctrl = go.GetComponent<Waypoint>();
        CheckAssignment(ctrl);
                        
        return ctrl;
    }

    public void SelectWaypoint(Waypoint waypoint)
    {

        selectedWaypoint = waypoint;
        CheckAssignment(selectedWaypoint);        
        // uIManager.UpdateSelectedAircraftInfoPanel(aircraft);
    }
    public void Delete()
    {
        Destroy(waypointContainer.gameObject);        
    }
    public void Clear()
    {
        if (waypointContainer != null)
        {
            for (int i = 0; i < waypointContainer.childCount; i++)
            {
                Destroy(waypointContainer.GetChild(i).gameObject);
            }
        }
    }
}