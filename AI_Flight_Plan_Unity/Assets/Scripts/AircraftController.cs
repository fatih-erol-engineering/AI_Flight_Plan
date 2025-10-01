using UnityEngine;

public class AircraftController : MonoBehaviour
{
    public AircraftSpec spec;  // runtime enjekte edilecek    
    public Trajectory trajectoryDrawer  = new Trajectory();


    public void Init(AircraftSpec s)
    {
        spec = s;
        GetComponent<Renderer>().material.color = spec.color;
    }

}

