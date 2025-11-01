using UnityEngine;
using UnityEngine.UIElements;
public class PropertySelectionSpawnerState : MonoBehaviour, ISpawnerState
{

    [SerializeField] private Camera cam;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] { KeyCode.Mouse0, KeyCode.Return };
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };
    private VisualElement root, popUpRoot;
    private Button createBtn;
    private FloatField northField, eastField, altitudeField, timeField;
    private Vector2 mousePosition;
    private Vector3 prev_spawnedObjectPosition;
    private Vector3 prev_camPosition; // Daha yazmadim game event ile halledecegim


    void AssignData()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        CheckAssignment(uiDocument);

        root = uiDocument.rootVisualElement;
        CheckAssignment(root);

        popUpRoot = root.Q<VisualElement>("popUpRoot");
        CheckAssignment(popUpRoot, "popUpRoot");

        createBtn = popUpRoot.Q<Button>("createBtn");
        CheckAssignment(createBtn, "createBtn");

        northField = popUpRoot.Q<FloatField>("northField");
        CheckAssignment(northField, "northField");
        northField.formatString = "0.00";

        eastField = popUpRoot.Q<FloatField>("eastField");
        CheckAssignment(eastField, "eastField");
        eastField.formatString = "0.00";

        altitudeField = popUpRoot.Q<FloatField>("altitudeField");
        CheckAssignment(altitudeField, "altitudeField");
        altitudeField.formatString = "0.00";

        timeField = popUpRoot.Q<FloatField>("timeField");
        CheckAssignment(timeField, "timeField");
        timeField.formatString = "0.00";

        if (cam == null) cam = Camera.current;
        createBtn.clicked += () =>
        {
            Debug.Log($"Create button clicked with values - North: {northField.value}, East: {eastField.value}, Altitude: {altitudeField.value}, Time: {timeField.value}");
            // Here you can add logic to finalize the object creation with the specified properties
        };
    }

    void ShowPopup(Spawner spawner)
    {
        UpdatePopupPosition(spawner.spawnedObject, true);
        popUpRoot.RemoveFromClassList("hidden");
    }
    void UpdatePopupPosition(GameObject spawnedObject, bool isImmediate = false)
    {
        // UI Toolkit uses top-left origin; we want the popup's BOTTOM-LEFT
        // corner to sit above the aircraft. Convert screen coords (bottom-left origin)
        // to UI top-left coords for the bottom point, then subtract the popup height
        // so style.top represents the correct top position.
        // if (prev_spawnedObjectPosition != spawnedObject.transform.position || isImmediate)
        // {
        Vector3 worldPoint = spawnedObject.transform.position;
        var rend = spawnedObject.GetComponentInChildren<Renderer>();
        float extraHeight = 1.0f;
        if (rend != null)
        {
            extraHeight = rend.bounds.extents.y + 0.5f;
        }
        worldPoint += Vector3.up * extraHeight;

        Vector3 screenPoint;
        // if (cam != null)
        screenPoint = cam.WorldToScreenPoint(worldPoint);
        // else
        //     screenPoint = Input.mousePosition; // fallback
        float bottomX = screenPoint.x;
        float bottomY = Screen.height - screenPoint.y; // bottom coordinate in UI top-left system

        // small pixel offset so popup doesn't overlap the object (applied to bottom coord)
        const float pixelOffsetBottom = -10f; // move a bit upward
        bottomY += pixelOffsetBottom;

        // Try to read popup size. Prefer layout values; fall back to resolvedStyle; then defaults.
        float popupWidth = popUpRoot.layout.width;
        float popupHeight = popUpRoot.layout.height;
        if (popupWidth <= 0f) popupWidth = popUpRoot.resolvedStyle.width;
        if (popupHeight <= 0f) popupHeight = popUpRoot.resolvedStyle.height;
        // sensible defaults if measurement isn't available yet
        if (popupWidth <= 0f) popupWidth = 200f;
        if (popupHeight <= 0f) popupHeight = 100f;

        // Compute top-left from bottom-left
        float left = bottomX;
        float top = bottomY - popupHeight;

        // clamp to screen so popup stays visible (account for popup size)
        left = Mathf.Clamp(left, 5f, Screen.width - popupWidth - 5f);
        top = Mathf.Clamp(top, 5f, Screen.height - popupHeight - 5f);

        // Make sure popup uses absolute positioning and place it
        popUpRoot.style.position = Position.Absolute;
        popUpRoot.style.left = new StyleLength(new Length(left, LengthUnit.Pixel));
        popUpRoot.style.top = new StyleLength(new Length(top, LengthUnit.Pixel));

        prev_spawnedObjectPosition = spawnedObject.transform.position;
        // }
    }

    public void OnEnter(Spawner spawner)
    {
        AssignData();
        ShowPopup(spawner);
    }

    public void OnExit(Spawner spawner, bool isCancelled)
    {
        if (isCancelled)
        {
            spawner.CancelSpawning();
        }
        else
        {
            Debug.Log("Apllied Succesfuly");
        }
        popUpRoot.AddToClassList("hidden");

    }

    public void Tick(Spawner spawner)
    {

        UpdatePopupPosition(spawner.spawnedObject);

        foreach (var key in selectionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log("Apllied Spawning");
                OnExit(spawner, false);
                spawner.SetCurrentState(spawner.idleSpawnerState);
            }
        }

        foreach (var key in cancelKeys)
        {
            if (Input.GetKeyDown(key))
            {
                OnExit(spawner, true);
                Debug.Log("Cancelled Spawning");
                spawner.SetCurrentState(spawner.idleSpawnerState);
            }
        }
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }



}