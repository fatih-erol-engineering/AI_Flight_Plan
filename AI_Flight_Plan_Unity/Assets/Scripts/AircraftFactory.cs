using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(UIManager))]
public class AircraftFactory : MonoBehaviour
{
    [SerializeField]
    public UIManager uIManager { get; private set; }

    [SerializeField]
    private AircraftSpecRegistry registry;

    public Theme theme;

    public AircraftSpec aircraftSpecToSpawn { get; private set; }
    protected Transform aircraftParent;

    public List<Aircraft> aircraftList;

    private string prev_selectedAircraftModelName;

    public void OnEnable()
    {
        AssignData();
    }
    private void AssignData()
    {
        uIManager = GetComponent<UIManager>();
    }
    public void Update()
    {
        ChangeAircraftSpecToSpawnWithUI();
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
        if (aircraftParent == null)
        {
            GameObject aircraftParentObj = new GameObject("Aircrafts");
            aircraftParentObj.transform.parent = this.transform;
            aircraftParentObj.transform.localPosition = Vector3.zero;
            aircraftParent = aircraftParentObj.transform;
        }
        var spec = registry.Get(aircraftSpecToSpawn.model);
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
        var spec = registry.Get(type);
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
        if (aircraftParent == null)
        {
            GameObject aircraftParentObj = new GameObject(parentName);
            aircraftParentObj.transform.parent = this.transform;
            aircraftParentObj.transform.localPosition = Vector3.zero;
            aircraftParent = aircraftParentObj.transform;
        }
        var spec = registry.Get(aircraftSpecToSpawn.model);
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
        return registry.Get(modelName);
    }
    public void Delete()
    {
        Destroy(aircraftParent.gameObject);
        aircraftList.Clear();
    }

}