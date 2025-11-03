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
    public List<Aircraft> AircraftList;
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

    public Aircraft Spawn(Vector3 globalPosition, Quaternion globalRotation, TimeGame time)
    {
        ChangeAircraftPrefabWithUI();
        var go = Instantiate(aircraftPrefab, Vector3.zero, Quaternion.identity, transform);
        var ctrl = go.GetComponentInChildren<Aircraft>();
        ctrl.transform.position = globalPosition;
        ctrl.transform.rotation = globalRotation;
        CheckAssignment(ctrl);
        AircraftList.Add(ctrl);
        ctrl.id = AircraftList.Count;
        ctrl.SetTime(time);
        selectedAircraft = ctrl;
        aircraftSpawnFlag = true;

        ctrl.trajectory.SetSegmentLength(ctrl.aircraftProperties.nominalVelocity_m_s * 2f); // segment length is 1 second worth of travel at nominal speed
        ctrl.trajectory.SetTubeRadius(ctrl.aircraftProperties.tubeRadius_m); // set tube radius
        ctrl.trajectory.SetAircraft(ctrl);
        GameEvents.Instance.AircraftSpawned(ctrl);
        return ctrl;
    }
    public void Clear()
    {
        foreach (var aircraft in AircraftList)
        {
            if (aircraft != null)
                Destroy(aircraft.gameObject);
        }
        AircraftList.Clear();
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
    public List<TrajectoryDrawer> GetAllTrajectories()
    {
        List<TrajectoryDrawer> trajList = new List<TrajectoryDrawer>();
        foreach (var aircraft in AircraftList)
        {
            trajList.Add(aircraft.trajectory);
        }
        return trajList;
    }
}