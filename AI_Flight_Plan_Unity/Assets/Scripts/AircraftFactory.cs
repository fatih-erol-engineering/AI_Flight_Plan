using UnityEngine;

public class AircraftFactory : MonoBehaviour
{
    public AircraftSpecRegistry registry;

    public AircraftController Spawn(AircraftModel type, Vector3 pos, Quaternion rot)
    {
        var spec = registry.Get(type);
        if (spec == null || spec.prefab == null)
        {
            Debug.LogError($"[AircraftFactory] Missing spec/prefab for {type}");
            return null;
        }

        var go = Instantiate(spec.prefab, pos, rot,transform);
        var ctrl = go.GetComponent<AircraftController>();
        if (!ctrl) ctrl = go.AddComponent<AircraftController>();
        ctrl.Init(spec);
        return ctrl;
    }
}
