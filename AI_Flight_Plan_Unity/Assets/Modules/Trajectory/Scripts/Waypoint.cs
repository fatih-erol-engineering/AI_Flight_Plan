
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class Waypoint : MonoBehaviour, IEditable, ISelectable
{
    [field: SerializeField] public TimeGame time { get; private set; }
    private Vector3 prev_position = Vector3.zero;

    [Header("Selection Appearance")]
    [SerializeField] private MeshRenderer[] highlightMeshRenderer;
    [SerializeField] private LineRenderer lineRenderer;
    private bool isSelected = false;
    private MaterialPropertyBlock mpb;
    private Color highlightColor;
    private Color highlightEmissionColor;
    private Color originalColor;
    private Color originalEmissionColor;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor"); // URP Lit
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor"); // URP Lit

    void OnValidate()
    {
        AssignData();
    }

    void Awake()
    {
        AssignData();
    }
    void AssignData()
    {
        mpb = new MaterialPropertyBlock();
        for (int i = 0; i < highlightMeshRenderer.Length; i++)
        {
            highlightMeshRenderer[i].GetPropertyBlock(mpb);
        }
        originalColor = highlightMeshRenderer[0].material.GetColor(BaseColorID);
        originalEmissionColor = highlightMeshRenderer[0].material.GetColor(EmissionColorID);
    }

#if UNITY_EDITOR
    void Update()
    {
        var go = UnityEditor.Selection.activeGameObject;
        if (go != null)
        {
            if (go.GetComponent<Waypoint>() != null && transform.position != prev_position)
            {
                GameEvents.Instance.WaypointPositionChanged(this, prev_position);
                prev_position = transform.position;
            }
        }
    }
#endif


    public void SetPosition(Vector3 _position)
    {
        if (transform.position == _position) return;
        Vector3 oldPosition = transform.position;
        GameEvents.Instance.WaypointPositionChanged(this, oldPosition);
        transform.position = _position;
    }
    public void SetTime(TimeGame _time)
    {
        TimeGame oldTime = time;
        GameEvents.Instance.WaypointTimeChanged(this, oldTime);
        time.SetTime(_time.second);
    }

    public void ShowEditableProperties()
    {
        Debug.Log("x: " + transform.position.x);
        Debug.Log("y: " + transform.position.y);
        Debug.Log("z: " + transform.position.z);
        Debug.Log("time: " + time.second);
    }

    public void OnHoverEnter()
    {
        if (isSelected) return;
        SetHightlightColor(HDR2Normal(ThemeManager.Instance.theme.Hover));
        SetHightlightEmissionColor(ThemeManager.Instance.theme.Hover);
        SetLineRendererColor(HDR2Normal(ThemeManager.Instance.theme.Hover));
        SetLineRendererEmissionColor(ThemeManager.Instance.theme.Hover);
    }
    public void OnHoverExit()
    {
        if (isSelected) return;
        SetHightlightColor(originalColor);
        SetHightlightEmissionColor(originalEmissionColor);
        SetLineRendererColor(originalColor);
        SetLineRendererEmissionColor(originalEmissionColor);
    }
    public void OnSelect()
    {
        SetHightlightColor(HDR2Normal(ThemeManager.Instance.theme.Select));
        SetHightlightEmissionColor(ThemeManager.Instance.theme.Select);
        SetLineRendererColor(HDR2Normal(ThemeManager.Instance.theme.Select));
        SetLineRendererEmissionColor(ThemeManager.Instance.theme.Select);
        SetIsSelected(true);
    }
    public void OnDeselect()
    {
        SetHightlightColor(originalColor);
        SetHightlightEmissionColor(originalEmissionColor);
        SetLineRendererColor(originalColor);
        SetLineRendererEmissionColor(originalEmissionColor);
        SetIsSelected(false);
    }
    public void SetHightlightColor(Color _color, bool isImmediate = false)
    {
        if (highlightColor != _color || isImmediate)
        {
            highlightColor = _color;
            for (int i = 0; i < highlightMeshRenderer.Length; i++)
            {
                mpb.SetColor(BaseColorID, _color);
                highlightMeshRenderer[i].SetPropertyBlock(mpb);
            }
        }
    }
    public void SetHightlightEmissionColor(Color _color, bool isImmediate = false)
    {
        if (highlightEmissionColor != _color || isImmediate)
        {
            highlightEmissionColor = _color;
            for (int i = 0; i < highlightMeshRenderer.Length; i++)
            {
                mpb.SetColor(EmissionColorID, _color);
                highlightMeshRenderer[i].SetPropertyBlock(mpb);
            }
        }
    }
    public void SetLineRendererColor(Color _color, bool isImmediate = false)
    {
        mpb.SetColor(BaseColorID, _color);
        lineRenderer.SetPropertyBlock(mpb);
    }
    public void SetLineRendererEmissionColor(Color _color, bool isImmediate = false)
    {
        mpb.SetColor(EmissionColorID, _color);
        lineRenderer.SetPropertyBlock(mpb);
    }

    public void SetIsSelected(bool _isSelected, bool isImmediate = false)
    {
        if (isSelected != _isSelected || isImmediate)
        {
            isSelected = _isSelected;
        }
    }
    public Color HDR2Normal(Color _color)
    {
        float maxComponent = _color.maxColorComponent;
        if (maxComponent > 1f)
        {
            _color.r = _color.r / maxComponent;
            _color.g = _color.g / maxComponent;
            _color.b = _color.b / maxComponent;
        }
        return _color;
    }
}
