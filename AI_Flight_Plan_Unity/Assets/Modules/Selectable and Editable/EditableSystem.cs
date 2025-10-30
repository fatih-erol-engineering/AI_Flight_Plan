using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditableSystem : MonoBehaviour
{
    public static EditableSystem Instance { get; private set; }
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask pickMask = ~0; // gerekirse katman daralt
    private float maxDistance = 200f;
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse1;

    private IEditable hitEditable;
    private IEditable hitEditable1;
    private IEditable hitEditable2;
    public GameObject editableObject;

    ////////////////////////////////////////////////

    void OnValidate()
    {
        AssignData();
    }
    void Awake()
    {
        AssignData();
    }
    void OnEnable()
    {
        AssignData();
    }

    ////////////////////////////////////////////////

    void AssignData()
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
                HandleEditable();
                break;
        }
    }
    private void HandleEditable()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape) && hitEditable != null)
        {
            editableObject = null;
            GameEvents.Instance?.EditableExit(hitEditable);
            hitEditable1 = null;
            hitEditable2 = null;
        }

        if (Input.GetKeyDown(selectKey))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask))
            {
                hitEditable1 = hit.collider.GetComponentInParent<IEditable>();
            }
        }

        if (Input.GetKeyUp(selectKey))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask))
            {
                hitEditable2 = hit.collider.GetComponentInParent<IEditable>();
            }
        }

        if (hitEditable1 == hitEditable2 && hitEditable1 != null && hitEditable2 != null)
        {
            hitEditable = hitEditable2;
            editableObject = (hitEditable as MonoBehaviour)?.gameObject;
            GameEvents.Instance?.EditableEnter(hitEditable);
        }
    }
}
