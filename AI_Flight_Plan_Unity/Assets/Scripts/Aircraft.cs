using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Aircraft : SelectableMonoBehaviour
{
    public AircraftSpec spec;  // runtime enjekte edilecek    
    public Trajectory trajectoryDrawer  = new Trajectory();

    private void Start()
    {
        Init();
    }
    public void Init()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = spec.color;
    }
    public void Init(AircraftSpec s)
    {
        Renderer rend = GetComponent<Renderer>();
        base.Init(rend.material);
        spec = s;
        rend.material.color = spec.color;        
    }

}

