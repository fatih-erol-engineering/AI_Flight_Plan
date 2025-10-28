using UnityEngine;

// [RequireComponent(typeof(Collider))]
// [RequireComponent(typeof(MeshRenderer))]
public class Selectable : MonoBehaviour, ISelectable
{
    [Header("Selection Settings")]
    [SerializeField] protected Theme theme;
    [SerializeField] private MeshRenderer[] _baseMeshRendererArray;
    [SerializeField] private Material _baseMaterial;

    protected bool _selected;

    void OnValidate()
    {

        if (_baseMeshRendererArray == null)
        {
            Debug.LogWarning("[Selectable] No MeshRenderer found on " + name);
        }
        if (_baseMaterial == null)
        {
            Debug.LogWarning("[Selectable] No Material found on " + name);
        }
        ;

    }
    public virtual void OnHoverEnter()
    {
        if (_selected) return;
        foreach (var _baseMeshRenderer in _baseMeshRendererArray)
        {
            _baseMeshRenderer.material = theme.Hover;
        }
    }

    public virtual void OnHoverExit()
    {
        if (_selected) return;

        foreach (var _baseMeshRenderer in _baseMeshRendererArray)
        {
            _baseMeshRenderer.material = _baseMaterial;
        }
    }

    public virtual void OnSelect()
    {
        _selected = true;
        foreach (var _baseMeshRenderer in _baseMeshRendererArray)
        {
            _baseMeshRenderer.material = theme.Select;
        }

    }

    public virtual void OnDeselect()
    {
        _selected = false;
        foreach (var _baseMeshRenderer in _baseMeshRendererArray)
        {
            _baseMeshRenderer.material = _baseMaterial;
        }
    }

}
