using UnityEngine;


public class Aircraft : SelectableMonoBehaviour
{
    public AircraftSpec spec;
    public Trajectory trajectory;
    public Theme theme;


    private void Start()
    {
        UpdateColor();
    }

    public void CreateWaypoint(Vector3 globalPosition)
    {
        if (trajectory == null)
        {
            GameObject trajParentGO = Instantiate(theme.trajectoryPrefab, this.transform);
            trajParentGO.transform.localPosition = Vector3.zero;
            trajParentGO.transform.localRotation = Quaternion.identity;
            trajectory = trajParentGO.GetComponent<Trajectory>();
            if (trajectory == null)
            {
                trajectory = trajParentGO.AddComponent<Trajectory>();
                trajectory.theme = theme;
            }
        }        
        trajectory.CreateWaypoint(globalPosition);
    }
    public void CreateWaypoint(Vector3 globalPosition,float time_s)
    {
        if (trajectory == null)
        {
            GameObject trajParentGO = Instantiate(theme.trajectoryPrefab, this.transform);
            trajParentGO.transform.localPosition = Vector3.zero;
            trajParentGO.transform.localRotation = Quaternion.identity;
            trajectory = trajParentGO.GetComponent<Trajectory>();
            if (trajectory == null)
            {
                trajectory = trajParentGO.AddComponent<Trajectory>();
                trajectory.theme = theme;
            }
        }
        trajectory.CreateWaypoint(globalPosition,time_s);
    }
    public void UpdateColor()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        rend.material.color = spec.color;
    }
    public void UpdateColor(AircraftSpec s)
    {
        Renderer rend = GetComponentInChildren<Renderer>();        
        spec = s;
        rend.material.color = spec.color;        
    }

}

