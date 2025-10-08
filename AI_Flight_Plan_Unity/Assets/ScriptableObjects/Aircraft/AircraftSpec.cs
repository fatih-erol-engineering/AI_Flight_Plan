using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "AircraftSpec", menuName = "Flight/AircraftSpec")]
public class AircraftSpec : ScriptableObject
{
    public AircraftModel model;
    public AircraftType type;   
    public float minVelocity_m_s;
    [Range(0,500)]
    public float nominalVelocity_m_s;
    [Range(0, 500)]
    public float maxVelocity_m_s;
    [Range(0,200)]
    public float minTurnRadious_m;    
    public float mass_kg;
    public Color color;
    public GameObject prefab;
    public float noise_dBA;
}
public enum AircraftModel
{
    Ehang,
    Mavic_Pro,
    Hawk,
    Parrot,                                        
}
public enum AircraftType
{
    Rotor,
    FixedWing,
}



