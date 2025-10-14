using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flight/Aircraft Spec Registry", fileName = "AircraftSpecRegistry")]
public class AircraftSpecRegistry : ScriptableObject
{
    public List<AircraftProperties> specs = new();

    Dictionary<AircraftModel, AircraftProperties> _map;  
    
    public List<AircraftProperties> rotorAircrafts { get;  private set; }
    public List<AircraftProperties> fixedWingAircrafts { get; private set; }

    void OnEnable()
    {
        _map = new Dictionary<AircraftModel, AircraftProperties>();
        rotorAircrafts = new List<AircraftProperties>();
        fixedWingAircrafts = new List<AircraftProperties>();
        
        rotorAircrafts.Clear();
        fixedWingAircrafts.Clear();
        foreach (var s in specs)
        {
            if (!s) continue;
            _map[s.model] = s; 
            switch (s.type)
            {
                case AircraftType.Rotor:
                    rotorAircrafts.Add(s);
                    break;
                case AircraftType.FixedWing:
                    fixedWingAircrafts.Add(s);
                    break;
                default:
                    break;
            }            
        }
    }

    public bool TryGet(AircraftModel t, out AircraftProperties spec)
    {
        if (_map == null) OnEnable();
        return _map.TryGetValue(t, out spec) && spec != null;
    }

    public AircraftProperties Get(AircraftModel t)
    {
        if (TryGet(t, out var s)) return s;
        Debug.LogError($"[AircraftSpecRegistry] Spec not found for type {t}");
        return null;
    }
    public AircraftProperties Get(string modelName)
    {
        foreach (var s in specs)
        {
            if (!s) continue;
            if (s.model.ToString() == modelName)
            {
                return s;
            }
        }
        Debug.LogError($"[AircraftSpecRegistry] Spec not found for type {modelName}");
        return null;
    }
}
