using UnityEngine;
using UnityEngine.UIElements;

public class SpawnerPropertyPopupUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private Camera cam;
    private VisualElement root, popUpRoot;

    private FloatField northField, eastField, altitudeField;
    public Button createBtn;

    private void Start()
    {
        AssignData();
    }

    private void AssignData()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        popUpRoot = root.Q<VisualElement>("popUpRoot");
        CheckAssignment(popUpRoot, "popUpRoot");

        northField = popUpRoot.Q<FloatField>("northField");
        CheckAssignment(northField, "northField");

        eastField = popUpRoot.Q<FloatField>("eastField");
        CheckAssignment(eastField, "eastField");

        altitudeField = popUpRoot.Q<FloatField>("altitudeField");
        CheckAssignment(altitudeField, "altitudeField");

        createBtn = popUpRoot.Q<Button>("createBtn");
        CheckAssignment(createBtn, "createBtn");

        northField.formatString = "0.00";
        eastField.formatString = "0.00";
        altitudeField.formatString = "0.00";

     

        cam = Camera.main;
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }

    public void ShowPopup()
    {
        popUpRoot.RemoveFromClassList("hidden");
        focusField();
    }
    public void focusField()
    {
        altitudeField.Focus();
    }
    public void HidePopup()
    {
        popUpRoot.AddToClassList("hidden");
    }

    public void SetPositionToUI(Transform _transform)
    {
        if (new Vector3(northField.value, altitudeField.value, eastField.value) != _transform.position)
        {
            northField.value = _transform.position.x;
            eastField.value = _transform.position.z;
            altitudeField.value = _transform.position.y;
        }
    }

    public void ShowPopupOnTransform(Transform _transform)
    {

        Vector3 worldPoint = _transform.position;
        // try to offset above the aircraft using renderer bounds if available
        var rend = _transform.GetComponentInChildren<Renderer>();
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

    }

    public Vector3 GetPositionFromUI()
    {                
        Vector3 _position = Vector3.zero;
        _position.x = northField.value;
        _position.z = eastField.value;
        _position.y = altitudeField.value;        
        return _position;
    }






}
