using Unity.VisualScripting;
using UnityEngine;

public class Waypoint : SelectableBehaviour
{
    [field:SerializeField]
    public TimeGame time { get; private set; }
    private Vector3 oldPos;
    public ControlPoint[] controlPoints;
    [SerializeField]
    private MeshRenderer[] meshRenderers;
    private Material baseMaterial;
    
    protected void OnEnable()
    {
        oldPos = transform.position;
        AssignData();
    }
    void AssignData()
    {
        time = new TimeGame();        
        baseMaterial = meshRenderers[0].material;
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
    public void SetTime(TimeGame _time)
    {
        time.SetTime(_time.second);
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
        foreach (MeshRenderer mr in meshRenderers)
        {            
            mr.material = material;  
        }
    }
    public override void OnHoverExit()
    {
        base.OnHoverExit();
        UpdateMaterial(baseMaterial);
        
    }
    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        UpdateMaterial(theme.Hover);
    }
}
