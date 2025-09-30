using UnityEngine;

public class AircraftController : MonoBehaviour
{
    public AircraftSpec spec { get; private set; }   // runtime enjekte edilecek    
    public TrajectoryDrawer trajectoryDrawer { get; private set; } = new TrajectoryDrawer();


    public void Init(AircraftSpec s)
    {
        spec = s;
        GetComponent<Renderer>().material.color = spec.color;
    }

}

