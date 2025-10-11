using UnityEngine;

public class AircraftFactoryPreCreate : AircraftFactory
{
    public override Aircraft Spawn()
    {
        Aircraft aircraft = base.Spawn("Aircrafts Pre Create");

        aircraft.UpdateMaterial(theme.PreCreate);
        return aircraft;
    }
    
}