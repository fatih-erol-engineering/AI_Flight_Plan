using UnityEngine;
using UnityEngine.UIElements;

public class ControlPointPopupUI : MonoBehaviour
{
    public static ControlPointPopupUI Instance { get; private set; }
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root, popUpRoot;
    private FloatField northField, eastField, altitudeField;

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

        northField = popUpRoot.Q<FloatField>("northField");
        CheckAssignment(northField, "northField");

        eastField = popUpRoot.Q<FloatField>("eastField");
        CheckAssignment(eastField, "eastField");

        altitudeField = popUpRoot.Q<FloatField>("altitudeField");
        CheckAssignment(altitudeField, "altitudeField");

        popUpRoot.AddToClassList("hidden");


        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("An AircraftPopupUI already exists in the scene. Removing duplicate.", this);
            Destroy(Instance.gameObject);
            Instance = this;
        }
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }

    // Show the popup at the current mouse position (editor or play mode)
    public void ShowPopup(ControlPoint controlPoint)
    {
        northField.value = controlPoint.transform.position.x;
        eastField.value = controlPoint.transform.position.y;
        altitudeField.value = controlPoint.transform.position.z;


        // UI Toolkit uses top-left origin; Input.mousePosition is bottom-left — convert Y
        Vector2 mp = Input.mousePosition;
        float x = mp.x;
        float y = Screen.height - mp.y;

        // Make sure popup uses absolute positioning and place it
        popUpRoot.style.position = Position.Absolute;
        popUpRoot.style.left = new StyleLength(new Length(x, LengthUnit.Pixel));
        popUpRoot.style.top = new StyleLength(new Length(y, LengthUnit.Pixel));

        // Show by removing the 'hidden' class (assumes USS defines .hidden { display: none; } or similar)
        popUpRoot.RemoveFromClassList("hidden");
    }

    public void HidePopup()
    {
        popUpRoot.AddToClassList("hidden");
    }


}
