using UnityEngine;
using UnityEngine.EventSystems;

// [ExecuteAlways]
// [DefaultExecutionOrder(-999)] // 
public class SelectionSystem : MonoBehaviour
{
    public static SelectionSystem Instance { get; private set; }
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask pickMask = ~0; // gerekirse katman daralt
    private float maxDistance = 200f;
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode selectKey2 = KeyCode.Mouse1;

    private ISelectable _hovered;
    private ISelectable _selected;
    public GameObject selectedObject;

    private IEditable hitEditable1;
    private IEditable hitEditable2;
    private IEditable hitEditable;
    void OnValidate()
    {
        if (!mainCamera) mainCamera = Camera.main;
        maxDistance = mainCamera ? mainCamera.farClipPlane : 1000f;

        // Ensure a single instance
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;
        maxDistance = mainCamera ? mainCamera.farClipPlane : 1000f;

        // Ensure a single instance
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        UpdateHover();
        UpdateEditable();
        HandleSelection();
    }

    private void UpdateHover()
    {
        ISelectable hitSelectable = null;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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
        if (Input.GetKeyDown(selectKey) || Input.GetKeyDown(selectKey2))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {

            }
            else
            {
                if (_selected != null && !ReferenceEquals(_selected, _hovered))
                {
                    _selected.OnDeselect();
                    hitEditable1 = null;
                    hitEditable2 = null;
                    hitEditable = null;
                    GameEvents.Instance.EditableExit();

                }

                _selected = _hovered;
                selectedObject = (_selected as MonoBehaviour)?.gameObject;

                if (_selected != null)
                {
                    _selected.OnSelect();
                }

            }
        }



    }
    void UpdateEditable()
    {

        if (Input.GetKeyDown(selectKey2))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask))
            {
                hitEditable1 = hit.collider.GetComponentInParent<IEditable>();
            }
        }
        if (Input.GetKeyUp(selectKey2))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask))
            {
                hitEditable2 = hit.collider.GetComponentInParent<IEditable>();
            }
        }

        if (hitEditable1 == hitEditable2 && hitEditable1 != null)
        {
            hitEditable = hitEditable1;
            GameEvents.Instance.EditableEnter(hitEditable);
        }


    }
}
