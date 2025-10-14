using System.Collections.Generic;
using UnityEngine;

public class AircraftFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject _aircraftPrefab;
    [SerializeField]
    private AircraftPropertiesRegistry aircraftPropertiesRegistry;
    public List<Aircraft> aircraftList { get; private set; } = new List<Aircraft>();

    // public getter for other scripts to instantiate previews from the same prefab
    public GameObject aircraftPrefab => _aircraftPrefab;

    public Aircraft Spawn(Vector3 globalPosition, Quaternion globalRotation)
    {
        var go = Instantiate(_aircraftPrefab, globalPosition, globalRotation,transform);
        var ctrl = go.GetComponent<Aircraft>();        
        CheckAssignment(ctrl);
        aircraftList.Add(ctrl);

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

}