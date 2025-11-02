using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "AircraftSpec", menuName = "Flight/AircraftSpec")]
public class AircraftProperties : ScriptableObject
{
    public AircraftModel model;
    public AircraftType type;
    public float minVelocity_m_s;
    [Range(0, 500)]
    public float nominalVelocity_m_s;
    [Range(0, 500)]
    public float maxVelocity_m_s;
    [Range(0, 200)]
    public float minTurnRadious_m;
    public float mass_kg;
    public float noise_dBA;
    public GameObject aircraftPrefab;
    public float tubeRadius_m;
}
public enum AircraftModel
{
    Taxi_Drone,
    Cine_Drone,
    Racer_Drone,
    Survaillance_Drone,
}


