using System.Collections.Generic;
using UnityEngine;

public class WaypointFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject _waypointPrefab;
    public List<Waypoint> waypointList { get; private set; } = new List<Waypoint>();

    // public getter for other scripts to instantiate previews from the same prefab
    public GameObject WaypointPrefab => _waypointPrefab;

    public Waypoint Spawn(Vector3 globalPosition, Quaternion globalRotation, TimeGame time)
    {
        var go = Instantiate(_waypointPrefab, globalPosition, globalRotation,transform);
        var ctrl = go.GetComponent<Waypoint>();
        go.GetComponent<WaypointShow>()?.ShowWaypoint();
        ctrl.SetTime(time);
        CheckAssignment(ctrl);
        waypointList.Add(ctrl);

        GameObject previewLine = new GameObject("PreviewLine");
        previewLine.transform.SetParent(ctrl.transform, false);
        var lr = previewLine.AddComponent<LineRenderer>();                  
        previewLine.transform.position = ctrl.transform.position;                        
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth  = 0.1f;
        lr.endWidth  = 0.1f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.allowOcclusionWhenDynamic = false;

        // fallback simple material
        lr.material = go.GetComponent<MeshRenderer>().material;
        lr.SetPosition(0, ctrl.transform.position);
        lr.SetPosition(1, ctrl.transform.position + Vector3.down * 10000f);
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