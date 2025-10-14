using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class SelectableBehaviour : MonoBehaviour, ISelectable
{
    [Header("Selection Settings")]
    // [SerializeField] private float hoverScaleMultiplier = 1.15f;
    // [SerializeField] private float selectScaleMultiplier = 1.30f;
    // [SerializeField] private float tweenDuration = 0.12f;
    [SerializeField] protected Theme theme;
    private Material hoverMaterial;
    private Material selectMaterial;
    private MeshRenderer _baseMeshRenderer;
    private Material _baseMaterial;

    private Vector3 _baseScale;
    private Coroutine _tween;
    protected bool _selected;

    void Awake()
    {
        _baseScale = transform.localScale;
        _baseMeshRenderer = GetComponent<MeshRenderer>();
        hoverMaterial = theme?.Hover;
        selectMaterial = theme?.Select;
        if (_baseMeshRenderer == null)
        {
            if (_baseMaterial == null)
            Debug.LogWarning("[SelectableBehaviour] No MeshRenderer found on " + name);            
        }
        _baseMaterial = _baseMeshRenderer?.material;
        if (_baseMaterial == null)
            Debug.LogWarning("[SelectableBehaviour] No Material found on " + name);        
    }
    public virtual void OnHoverEnter()
    {
        if (_selected) return;
        // TweenTo(_baseScale * hoverScaleMultiplier);
        _baseMeshRenderer.material = hoverMaterial ? hoverMaterial : _baseMaterial;
    }

    public virtual void OnHoverExit()
    {
        if (_selected) return;
        TweenTo(_baseScale);
        _baseMeshRenderer.material = _baseMaterial;
    }

    public virtual void OnSelect()
    {
        _selected = true;
        // TweenTo(_baseScale * selectScaleMultiplier);
        _baseMeshRenderer.material = selectMaterial ? selectMaterial : _baseMaterial;
    }

    public virtual void OnDeselect()
    {
        _selected = false;
        TweenTo(_baseScale);
        _baseMeshRenderer.material = _baseMaterial;
    }

    private void TweenTo(Vector3 target)
    {
        if (_tween != null) StopCoroutine(_tween);
        // _tween = StartCoroutine(ScaleTween(target, tweenDuration));
    }

    // private System.Collections.IEnumerator ScaleTween(Vector3 target, float dur)
    // {
    //     Vector3 start = transform.localScale;
    //     float t = 0f;
    //     while (t < 1f)
    //     {
    //         t += Time.deltaTime / dur;            
    //         float s = t * t * (3f - 2f * t);
    //         transform.localScale = Vector3.LerpUnclamped(start, target, s);
    //         yield return null;
    //     }
    //     transform.localScale = target;
    // }
}
