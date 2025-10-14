using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(MainGameManager))]
[RequireComponent(typeof(UIManager))]
public class AircraftFactory : MonoBehaviour
{   
    public MainGameManager mainGameManager { get; private set; } 
    public UIManager uIManager { get; private set; }



    // Assigned from GameManager
    public AircraftSpecRegistry aircraftSpecRegistry { get; private set; }

    public Theme theme { get; private set; }

    public AircraftSpec aircraftSpecToSpawn { get; private set; }
    protected Transform aircraftParent;

    public List<Aircraft> aircraftList;
    [field: SerializeField ]
    public Aircraft selectedAircraft { get; private set; }
    

    private string prev_selectedAircraftModelName;

    public void Awake()
    {
        AssignData();
    }
    private void AssignData()
    {
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        CheckAssignment(mainGameManager);

        if (!uIManager) uIManager = GetComponent<UIManager>();
        CheckAssignment(uIManager);

        if (!aircraftSpecRegistry) aircraftSpecRegistry = mainGameManager.aircraftSpecRegistry;        
        CheckAssignment(aircraftSpecRegistry);

        if (!theme) theme = mainGameManager.theme;        
        CheckAssignment(theme);

        aircraftSpecToSpawn = aircraftSpecRegistry.rotorAircrafts[0];
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }    

    private void ChangeAircraftSpecToSpawnWithUI()
    {
        if (prev_selectedAircraftModelName != null)
        {
            if ((prev_selectedAircraftModelName != uIManager.selectedAircraftModelName) && (uIManager.selectedAircraftModelName != null))
            {
                aircraftSpecToSpawn = Get(uIManager.selectedAircraftModelName);
            }
        }
        prev_selectedAircraftModelName = uIManager.selectedAircraftModelName;
    }

    public virtual Aircraft Spawn()
    {
        ChangeAircraftSpecToSpawnWithUI();
        if (aircraftParent == null)
        {
            GameObject aircraftParentObj = new GameObject("Aircrafts");
            aircraftParentObj.transform.parent = this.transform;
            aircraftParentObj.transform.localPosition = Vector3.zero;
            aircraftParent = aircraftParentObj.transform;
        }
        var spec = aircraftSpecRegistry.Get(aircraftSpecToSpawn.model);
        if (spec == null || spec.prefab == null)
        {
            Debug.LogError($"[AircraftFactory] Missing spec/prefab for {aircraftSpecToSpawn.model}");
            return null;
        }

        var go = Instantiate(spec.prefab, aircraftParent);
        var ctrl = go.GetComponent<Aircraft>();
        ctrl.spec = spec;
        if (!ctrl) ctrl = go.AddComponent<Aircraft>();
        ctrl.UpdateColor(spec.color);
        if (aircraftList == null)
        {
            aircraftList = new();
        }
        aircraftList.Add(ctrl);
        return ctrl;
    }
    public virtual Aircraft Spawn(AircraftModel type, Vector3 globalPosition, Quaternion globalRotation)
    {
        if (aircraftParent == null)
        {
            GameObject aircraftParentObj = new GameObject("Aircrafts");
            aircraftParentObj.transform.parent = this.transform;
            aircraftParentObj.transform.localPosition = Vector3.zero;
            aircraftParent = aircraftParentObj.transform;
        }
        var spec = aircraftSpecRegistry.Get(type);
        if (spec == null || spec.prefab == null)
        {
            Debug.LogError($"[AircraftFactory] Missing spec/prefab for {type}");
            return null;
        }

        var go = Instantiate(spec.prefab, globalPosition, globalRotation, aircraftParent);
        var ctrl = go.GetComponent<Aircraft>();
        ctrl.spec = spec;
        if (!ctrl) ctrl = go.AddComponent<Aircraft>();
        ctrl.UpdateColor(spec.color);
        if (aircraftList == null)
        {
            aircraftList = new();
        }
        aircraftList.Add(ctrl);
        return ctrl;
    }
    public virtual Aircraft Spawn(string parentName)
    {
        ChangeAircraftSpecToSpawnWithUI();
        if (aircraftParent == null)
        {
            GameObject aircraftParentObj = new GameObject(parentName);
            aircraftParentObj.transform.parent = this.transform;
            aircraftParentObj.transform.localPosition = Vector3.zero;
            aircraftParent = aircraftParentObj.transform;
        }
        var spec = aircraftSpecRegistry.Get(aircraftSpecToSpawn.model);
        if (spec == null || spec.prefab == null)
        {
            Debug.LogError($"[AircraftFactory] Missing spec/prefab for {aircraftSpecToSpawn.model}");
            return null;
        }

        var go = Instantiate(spec.prefab, aircraftParent);
        var ctrl = go.GetComponent<Aircraft>();
        ctrl.spec = spec;
        if (!ctrl) ctrl = go.AddComponent<Aircraft>();
        ctrl.UpdateColor(spec.color);
        if (aircraftList == null)
        {
            aircraftList = new();
        }
        aircraftList.Add(ctrl);        
        return ctrl;
    }

    public AircraftSpec Get(string modelName)
    {
        return aircraftSpecRegistry.Get(modelName);
    }
    public void SelectAircraft(Aircraft aircraft)
    {
        selectedAircraft = null;
        foreach (Aircraft _aircraft in aircraftList)
        {
            if (_aircraft == aircraft)
            {
                selectedAircraft = aircraft;
            }
        }
        CheckAssignment(selectedAircraft);        
        // uIManager.UpdateSelectedAircraftInfoPanel(aircraft);
    }
    public void Delete()
    {
        Destroy(aircraftParent.gameObject);
        aircraftList.Clear();
    }
    public void Clear()
    {        
        foreach (Aircraft aircraft in aircraftList)
        {
            Destroy(aircraft.gameObject);            
        }
        aircraftList.Clear();
    }
}