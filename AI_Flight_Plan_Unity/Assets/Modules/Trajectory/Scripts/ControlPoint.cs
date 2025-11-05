using UnityEngine;
using UnityEditor;

//[ExecuteAlways]
public class ControlPoint : MonoBehaviour, ISelectable, IEditable
{
    [SerializeField] private Vector3 closestPointToSpline;
    private Vector3 prev_position = Vector3.zero;


    [Header("Selection Appearance")]
    [SerializeField] private MeshRenderer[] highlightMeshRenderer;
    private bool isSelected = false;
    private MaterialPropertyBlock mpb;
    private Color highlightColor;
    private Color highlightEmissionColor;
    private Color originalColor;
    private Color originalEmissionColor;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor"); // URP Lit
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor"); // URP Lit
    private bool isInitialized = false;

    void OnEnable()
    {
        if (!isInitialized)
        {
            AssignData();
        }
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
        originalColor = highlightMeshRenderer[0].sharedMaterial.GetColor(BaseColorID);
        originalEmissionColor = highlightMeshRenderer[0].sharedMaterial.GetColor(EmissionColorID);

        GameEvents.Instance.OnEditableEnter -= OnEditableEnter;
        GameEvents.Instance.OnEditableExit -= OnEditableExit;
        GameEvents.Instance.OnEditableEnter += OnEditableEnter;
        GameEvents.Instance.OnEditableExit += OnEditableExit;
        isInitialized = true;
    }
    void OnDestroy()
    {
        GameEvents.Instance.OnEditableEnter -= OnEditableEnter;
        GameEvents.Instance.OnEditableExit -= OnEditableExit;
    }


#if UNITY_EDITOR
    void Update()
    {

        var go = UnityEditor.Selection.activeGameObject;
        if (go != null)
        {
            if (go.GetComponent<ControlPoint>() != null && transform.position != prev_position)
            {
                GameEvents.Instance.ControlPointPositionChanged(this, prev_position);
                prev_position = transform.position;
            }
        }

    }
#endif
    public void SetPosition(Vector3 _position)
    {
        if (_position == transform.position) return;
        Vector3 oldposition = transform.position;
        transform.position = _position;
        GameEvents.Instance.ControlPointPositionChanged(this, oldposition);
    }
    public void SetClosestPointToSpline(Vector3 point)
    {
        closestPointToSpline = point;
    }
    public Vector3 GetClosestPointToSpline()
    {
        return closestPointToSpline;
    }

    public void OnHoverEnter()
    {
        if (isSelected) return;
        SetHightlightColor(HDR2Normal(ThemeManager.Instance.theme.Hover));
        SetHightlightEmissionColor(ThemeManager.Instance.theme.Hover);
    }
    public void OnHoverExit()
    {
        if (isSelected) return;
        SetHightlightColor(originalColor);
        SetHightlightEmissionColor(originalEmissionColor);

    }
    public void OnSelect()
    {
        SetHightlightColor(HDR2Normal(ThemeManager.Instance.theme.Select));
        SetHightlightEmissionColor(ThemeManager.Instance.theme.Select);

        SetIsSelected(true);
    }
    public void OnDeselect()
    {
        SetHightlightColor(originalColor);
        SetHightlightEmissionColor(originalEmissionColor);

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

    public void OnEditableEnter(IEditable _editable)
    {
        if (_editable == (this as IEditable))
        {
            ControlPointPopupUI.Instance.ShowPopup(this);
        }

    }
    public void OnEditableExit()
    {
        ControlPointPopupUI.Instance.HidePopup();
    }
}
