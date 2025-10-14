using UnityEngine;

public class HoverSelectionSystem : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask pickMask = ~0; // gerekirse katman daralt
    [SerializeField] private float maxDistance = 200f;
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;

    private ISelectable _hovered;
    private ISelectable _selected;
    public GameObject selectedObject;


    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        UpdateHover();
        HandleSelection();
    }

    private void UpdateHover()
    {
        ISelectable hitSelectable = null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask))
        {
            hitSelectable = hit.collider.GetComponentInParent<ISelectable>();
        }

        if (!ReferenceEquals(hitSelectable, _hovered))
        {
            _hovered?.OnHoverExit();
            _hovered = hitSelectable;
            _hovered?.OnHoverEnter();
        }
    }

    private void HandleSelection()
    {
        if (Input.GetKeyDown(selectKey))
        {
            if (_selected != null && !ReferenceEquals(_selected, _hovered))
                _selected.OnDeselect();

            _selected = _hovered;            
            selectedObject = (_selected as SelectableBehaviour)?.gameObject;
    

            if (_selected != null)
                _selected.OnSelect();
        }
    }
}
