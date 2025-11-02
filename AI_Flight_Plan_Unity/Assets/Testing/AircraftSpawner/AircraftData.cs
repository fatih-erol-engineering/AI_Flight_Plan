using UnityEngine;

[CreateAssetMenu(fileName = "AircraftData", menuName = "Flight/AircraftData", order = 1)]
public class AircraftData : ScriptableObject
{
    public AircraftType type;
    public float minVelocity_m_s;
    public float nominalVelocity_m_s;
    public float maxVelocity_m_s;
    public float minTurnRadius_m;
    public float mass_kg;
    public float noise_dBA;
    public float tubeRadius_m;
}
public enum AircraftType
{
    Rotor,
    FixedWing,
}



