using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.InputSystem;

public class AircraftFactory : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private UIManager uIManager;

    public AircraftSpecRegistry registry;
    [SerializeField]
    private Theme theme;
    
    public AircraftSpec aircraftSpecToSpawn { get; private set; }
    private Transform aircraftParent;

    public List<Aircraft> aircraftList;
    

    // PRE SHOW PROPERTIES
    public AircraftPreShow aircraftPreShow { get; private set;}
    public GameObject aircrafPreShowParent { get; private set;}

    private string prev_selectedAircraftModelName;

    private void Awake()
    {
        if(gameManager == null) gameManager = GetComponent<GameManager>();
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



    public Aircraft Spawn(AircraftModel type, Vector3 globalPosition, Quaternion globalRotation)
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
        ctrl.UpdateColor();
        if (aircraftList == null) 
        {
            aircraftList = new();
        }
        aircraftList.Add(ctrl);
        return ctrl;
    }

    public Aircraft Spawn()
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
        ctrl.UpdateColor();
        if (aircraftList == null)
        {
            aircraftList = new();
        }
        aircraftList.Add(ctrl);
        return ctrl;
    }









    public void DeleteAircraftPreShow()
    {
        Destroy(aircrafPreShowParent);
        aircraftPreShow = null;
        aircrafPreShowParent = null;        
    }
    public AircraftPreShow SpawnForPreShow()
    {
        AircraftPreShow ctrl = null;
        ChangeAircraftSpecToSpawnWithUI();
        if ((aircraftPreShow == null))
        {
            if (aircraftSpecToSpawn != null)
            {
                if (aircrafPreShowParent == null)
                {
                    aircrafPreShowParent = new GameObject("Pre Show Aircraft");
                    aircrafPreShowParent.transform.parent = this.transform;
                    aircrafPreShowParent.transform.localPosition = Vector3.zero;                    
                }
                DeleteAllChild(aircrafPreShowParent);                    
                var go = Instantiate(aircraftSpecToSpawn.prefab, aircrafPreShowParent.transform);
                ctrl = go.AddComponent<AircraftPreShow>();
                ctrl.spec = aircraftSpecToSpawn;
                ctrl.preShowMaterial = theme.PreShow;
                ctrl.UpdateColor();
                aircraftPreShow = ctrl;
            }
        }
        return ctrl;
    }

    public AircraftSpec Get(string modelName)
    {
        return registry.Get(modelName);
    }

    private void DeleteAllChild(GameObject go)
    {
        for (int i = go.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = go.transform.GetChild(i);
            DestroyImmediate(child);
        }
    }
}