using UnityEngine;
using UnityEngine.UIElements;

public class SoundLimitedRAPopupUI : MonoBehaviour
{
    public static SoundLimitedRAPopupUI Instance { get; private set; }
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root, popUpRoot;
    private FloatField currentNoiseField, limitNoiseField;

    // void OnValidate()
    // {
    //     AssignData();
    // }
    void Awake()
    {
        AssignData();
    }
    void AssignData()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        CheckAssignment(uiDocument);

        root = uiDocument.rootVisualElement;
        CheckAssignment(root);

        popUpRoot = root.Q<VisualElement>("popUpRoot");
        CheckAssignment(popUpRoot, "popUpRoot");

        // Query fields under the popup root (they are expected to be inside popUpRoot)
        currentNoiseField = popUpRoot.Q<FloatField>("currentNoiseField");
        CheckAssignment(currentNoiseField, "currentNoiseField");
        currentNoiseField.formatString = "0.00";

        limitNoiseField = popUpRoot.Q<FloatField>("limitNoiseField");
        CheckAssignment(limitNoiseField, "limitNoiseField");
        limitNoiseField.formatString = "0.00";


        popUpRoot.AddToClassList("popupHidden");

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("An AircraftPopupUI already exists in the scene. Removing duplicate.", this);
            Instance = this;
        }
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }

    // Show the popup at the current mouse position (editor or play mode)
    public void ShowPopup(SoundLimitedRestrictedArea soundLimitedRestrictedArea)
    {
        if (popUpRoot == null) return;
        currentNoiseField.value = soundLimitedRestrictedArea.currentSound_dBa;
        limitNoiseField.value = soundLimitedRestrictedArea.soundLimit_dBa;

        Camera cam = Camera.main;
        if (cam == null) cam = Camera.current;

        Vector3 worldPoint = soundLimitedRestrictedArea.transform.position;
        // try to offset above the aircraft using renderer bounds if available
        var rend = soundLimitedRestrictedArea.GetComponentInChildren<Renderer>();
        float extraHeight = 1.0f;
        if (rend != null)
        {
            extraHeight = rend.bounds.extents.y + 0.5f;
        }
        worldPoint += Vector3.up * extraHeight;

        Vector3 screenPoint;
        if (cam != null)
            screenPoint = cam.WorldToScreenPoint(worldPoint);
        else
            screenPoint = Input.mousePosition; // fallback

        // UI Toolkit uses top-left origin; we want the popup's BOTTOM-LEFT
        // corner to sit above the aircraft. Convert screen coords (bottom-left origin)
        // to UI top-left coords for the bottom point, then subtract the popup height
        // so style.top represents the correct top position.

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

        // Show by removing the 'hidden' class (assumes USS defines .hidden { display: none; } or similar)
        popUpRoot.RemoveFromClassList("popupHidden");

    }

    public void HidePopup()
    {
        popUpRoot.AddToClassList("popupHidden");
    }

}
