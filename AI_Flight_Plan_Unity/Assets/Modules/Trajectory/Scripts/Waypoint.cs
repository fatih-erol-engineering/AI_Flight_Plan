
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class Waypoint : SelectableBehaviour
{
    [field: SerializeField] public TimeGame time { get; private set; }
    [SerializeField] private MeshRenderer[] meshRenderers;
    private Material baseMaterial;

    // #if UNITY_EDITOR
    void Update()
    {
        if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<Waypoint>() != null)
        {
            if (Selection.activeTransform.GetComponent<Waypoint>() == this)
            {
                GameEvents.instance.WaypointPositionChanged(this, transform.position);
            }
        }
    }
    // #endif
    public void Awake()
    {
        AssignData();
    }
    void AssignData()
    {
        baseMaterial = meshRenderers[0].material;
    }

    public void SetPosition(Vector3 _position)
    {
        if (transform.position == _position) return;
        Vector3 oldPosition = transform.position;
        GameEvents.instance.WaypointPositionChanged(this, oldPosition);
        transform.position = _position;
    }
    public void SetTime(TimeGame _time)
    {
        TimeGame oldTime = time;
        GameEvents.instance.WaypointTimeChanged(this, oldTime);
        time.SetTime(_time.second);
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
        if (!base._selected)
        {
            UpdateMaterial(baseMaterial);
        }
    }
    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        if (!base._selected)
        {
            UpdateMaterial(theme.Hover);
        }
    }

    public override void OnSelect()
    {
        base.OnSelect();
        UpdateMaterial(theme.Select);
    }
    public override void OnDeselect()
    {
        base.OnSelect();
        UpdateMaterial(baseMaterial);
    }



}
