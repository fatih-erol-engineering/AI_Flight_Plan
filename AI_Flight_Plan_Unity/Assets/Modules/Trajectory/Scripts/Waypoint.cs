using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Waypoint : SelectableBehaviour
{

    public TimeGame time { get; private set; }
    private Vector3 oldPos;
    public ControlPoint[] controlPoints;
    private MeshRenderer meshRenderer;
    protected void OnEnable()
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
        Vector3 deltaPos = globalPosition - oldPos; ;
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
    }
    public void SetTime(TimeGame time)
    {
        time.SetTime(time.second);
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
        time.SetTime(time_s) ;
    }
    public void UpdateMaterial(Material material)
    {
        meshRenderer.material = material;  
    }

}
