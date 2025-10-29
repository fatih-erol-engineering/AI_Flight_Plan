using UnityEngine;
using UnityEngine.EventSystems;

// [ExecuteAlways]
// [DefaultExecutionOrder(-999)] // 
public class HoverSelectionSystem : MonoBehaviour
{
    public static HoverSelectionSystem Instance { get; private set; }
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask pickMask = ~0; // gerekirse katman daralt
    private float maxDistance = 200f;
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;

    private ISelectable _hovered;
    private ISelectable _selected;
    public GameObject selectedObject;


    // void OnValidate()
    // {
    //     if (!mainCamera) mainCamera = Camera.main;
    //     maxDistance = mainCamera ? mainCamera.farClipPlane : 1000f;

    //     // Ensure a single instance
    //     if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    //     Instance = this;
    // }

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
        switch (mainGameManager.currentMode)
        {
            case MainGameMode.Free:
                UpdateHover();
                HandleSelection();
                break;
        }
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
        if (Input.GetKeyDown(selectKey))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {

            }
            else
            {
                if (_selected != null && !ReferenceEquals(_selected, _hovered))
                    _selected.OnDeselect();

                _selected = _hovered;
                selectedObject = (_selected as MonoBehaviour)?.gameObject;

                if (_selected != null)
                    _selected.OnSelect();

                // selectedObject = (_selected as IEditable)?.gameObject;

                // GameEvents.Instance.SelectionChanged(selectedObject);
            }
        }
    }
}
