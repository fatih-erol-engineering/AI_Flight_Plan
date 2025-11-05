using System.Collections.Generic;
using UnityEngine;

public class WaypointFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject _waypointPrefab;
    public List<Waypoint> waypointList = new List<Waypoint>();

    // public getter for other scripts to instantiate previews from the same prefab
    public GameObject WaypointPrefab => _waypointPrefab;

    public Waypoint Spawn(Vector3 globalPosition, Quaternion globalRotation, TimeGame time)
    {
        var go = Instantiate(_waypointPrefab, globalPosition, globalRotation, transform);
        var ctrl = go.GetComponent<Waypoint>();
        go.GetComponent<WaypointShow>()?.AssignData();        
        ctrl.SetOriginalTime(time);
        CheckAssignment(ctrl);
        waypointList.Add(ctrl);
        GameEvents.Instance.WaypointSpawned(ctrl);
        return ctrl;
    }
    public void Clear()
    {
        foreach (var waypoint in waypointList)
        {
            if (waypoint != null)
                Destroy(waypoint.gameObject);
        }
        waypointList.Clear();
    }

    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"(type: {typeof(T).Name}) is null at [{GetType().Name}]");
    }

}