using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class Waypoint : SelectableMonoBehaviour
{
    public TimeGame time;
    public WaypointType type = WaypointType.Open;
    public ControlPoint[] controlPoints;
    private Vector3 oldPos;
    private MeshRenderer meshRenderer;
    void Awake()
    {
        oldPos = transform.position;
        AssignData();
    }
    void AssignData()
    {
        time = new TimeGame();

        if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
        CheckAssignment(meshRenderer);

    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }
    public void setPosition(Vector3 globalPosition)
    {
        transform.position = globalPosition;  
        Vector3 deltaPos = globalPosition - oldPos;;
        if(controlPoints != null) 
        {            
            foreach (ControlPoint controlPoint in controlPoints)
            {
                if (controlPoint != null)
                {
                    controlPoint.transform.position += deltaPos;
                }
            }
        }
        oldPos = globalPosition;
    }
    public void setPosition(Vector3 globalPosition, float time_s)
    {
        transform.position = globalPosition;
        Vector3 deltaPos = globalPosition - oldPos;
        if (controlPoints != null)
        {
            foreach (ControlPoint controlPoint in controlPoints)
            {
                if (controlPoint != null)
                {
                    controlPoint.transform.position += deltaPos;
                }
            }
        }
        oldPos = globalPosition;
        time.second = time_s;
    }
    public void UpdateMaterial(Material material)
    {
        meshRenderer.material = material;  
    }

}


public enum WaypointType
{
    Open,
    Close
}
