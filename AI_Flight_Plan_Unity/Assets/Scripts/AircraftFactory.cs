using System.Collections.Generic;
using UnityEngine;

public class AircraftFactory : MonoBehaviour
{
    public AircraftSpecRegistry registry;    
    private Transform aircraftParent;
    public List<Aircraft> aircraftList;


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

        aircraftList.Add(ctrl);
        return ctrl;
    }
    
}
