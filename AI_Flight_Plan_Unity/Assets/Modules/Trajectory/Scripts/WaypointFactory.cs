using System.Collections.Generic;
using UnityEngine;

public class WaypointFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject waypointPrefab;
    public List<Waypoint> waypointList { get; private set; } = new List<Waypoint>();

    // public getter for other scripts to instantiate previews from the same prefab
    public GameObject WaypointPrefab => waypointPrefab;

    public Waypoint Spawn(Vector3 globalPosition, Quaternion globalRotation, float time_s)
    {
        var go = Instantiate(waypointPrefab, globalPosition, globalRotation,transform);
        var ctrl = go.GetComponent<Waypoint>();
        ctrl.SetTime(time_s);
        CheckAssignment(ctrl);
        waypointList.Add(ctrl);

        return ctrl;
    }
    public void Clear()
    {
        foreach (var wp in waypointList)
        {
            if (wp != null)
                Destroy(wp.gameObject);
        }
        waypointList.Clear();
    }

    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"(type: {typeof(T).Name}) is null at [{GetType().Name}]");
    } 

}