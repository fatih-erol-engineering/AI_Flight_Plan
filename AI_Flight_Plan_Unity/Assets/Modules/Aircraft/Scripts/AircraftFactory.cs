using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AircraftFactory : MonoBehaviour
{        
    [SerializeField]
    private UIManager uIManager;
    [SerializeField]
    private Theme theme;
    [SerializeField]
    private AircraftPropertiesRegistry aircraftPropertiesRegistry;
    [SerializeField]
    public GameObject aircraftPrefab;
    [SerializeField]
    public List<Aircraft> aircraftList { get; private set; } = new List<Aircraft>();
    public Aircraft selectedAircraft;
    [SerializeField]
    public bool aircraftSpawnFlag { get; private set; } = false;

    private string prev_selectedAircraftModelName;
    public void Awake()
    {
        AssignData();
    }
    private void AssignData()
    {
        if (!uIManager) uIManager = GetComponent<UIManager>();
        CheckAssignment(uIManager);

        aircraftPrefab = aircraftPropertiesRegistry.rotorAircrafts[0].GameObject();
    }
    void Update()
    {
        aircraftSpawnFlag = false;
        ChangeAircraftPrefabWithUI();
    }
    private void ChangeAircraftPrefabWithUI()
    {
        if ((prev_selectedAircraftModelName != uIManager.selectedAircraftModelName) && (uIManager.selectedAircraftModelName != null))
        {
            aircraftPrefab = Get(uIManager.selectedAircraftModelName);
        }
        prev_selectedAircraftModelName = uIManager.selectedAircraftModelName;
    }

    public Aircraft Spawn(Vector3 globalPosition, Quaternion globalRotation,TimeGame time)
    {
        ChangeAircraftPrefabWithUI();
        var go = Instantiate(aircraftPrefab, globalPosition, globalRotation, transform);
        var ctrl = go.GetComponentInChildren<Aircraft>();
        CheckAssignment(ctrl);
        aircraftList.Add(ctrl);
        ctrl.SetTime(time);
        selectedAircraft = ctrl;
        aircraftSpawnFlag = true;
        return ctrl;
    }
    public void Clear()
    {
        foreach (var aircraft in aircraftList)
        {
            if (aircraft != null)
                Destroy(aircraft.gameObject);
        }
        aircraftList.Clear();
    }

    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"(type: {typeof(T).Name}) is null at [{GetType().Name}]");
    } 
    public GameObject Get(string modelName)
    {
        return aircraftPropertiesRegistry.Get(modelName).aircraftPrefab;
    }
}