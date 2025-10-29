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

    private IEditable _editable;
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
        IEditable hitEditable = null;
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Escape) && _editable != null)
        {
            editableObject = null;
            GameEvents.Instance?.EditableExit(_editable);
            return;
        }

        if (Input.GetKeyDown(selectKey))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {

            }
            else
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask))
                {
                    hitEditable = hit.collider.GetComponentInParent<IEditable>();
                }

                if (!ReferenceEquals(hitEditable, _editable))
                {
                    editableObject = (_editable as MonoBehaviour)?.gameObject;
                    GameEvents.Instance?.EditableEnter(hitEditable);
                }
            }
        }
    }






}
