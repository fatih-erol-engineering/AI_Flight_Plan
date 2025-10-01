using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flight/Aircraft Spec Registry", fileName = "AircraftSpecRegistry")]
public class AircraftSpecRegistry : ScriptableObject
{
    public List<AircraftSpec> specs = new();

    Dictionary<AircraftModel, AircraftSpec> _map;

    void OnEnable()
    {
        _map = new Dictionary<AircraftModel, AircraftSpec>();
        foreach (var s in specs)
        {
            if (!s) continue;
            _map[s.model] = s; // son eklenen kazanýr (bilinçli)
        }
    }

    public bool TryGet(AircraftModel t, out AircraftSpec spec)
    {
        if (_map == null) OnEnable();
        return _map.TryGetValue(t, out spec) && spec != null;
    }

    public AircraftSpec Get(AircraftModel t)
    {
        if (TryGet(t, out var s)) return s;
        Debug.LogError($"[AircraftSpecRegistry] Spec not found for type {t}");
        return null;
    }
}
