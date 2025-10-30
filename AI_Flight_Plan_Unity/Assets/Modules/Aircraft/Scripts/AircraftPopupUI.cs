using UnityEngine;
using UnityEngine.UIElements;

public class AircraftPopupUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root, popUpRoot;
    private TextField modelField, typeField;
    private FloatField northField, eastField, altitudeField, noiseField, velocityField;

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
        modelField = popUpRoot.Q<TextField>("modelField");
        CheckAssignment(modelField, "modelField");

        typeField = popUpRoot.Q<TextField>("typeField");
        CheckAssignment(typeField, "typeField");

        northField = popUpRoot.Q<FloatField>("northField");
        CheckAssignment(northField, "northField");

        eastField = popUpRoot.Q<FloatField>("eastField");
        CheckAssignment(eastField, "eastField");

        altitudeField = popUpRoot.Q<FloatField>("altitudeField");
        CheckAssignment(altitudeField, "altitudeField");

        noiseField = popUpRoot.Q<FloatField>("noiseField");
        CheckAssignment(noiseField, "noiseField");

        velocityField = popUpRoot.Q<FloatField>("velocityField");
        CheckAssignment(velocityField, "velocityField");


        GameEvents.Instance.OnEditableEnter -= ShowPopup;
        GameEvents.Instance.OnEditableExit -= HidePopup;
        GameEvents.Instance.OnEditableEnter += ShowPopup;
        GameEvents.Instance.OnEditableExit += HidePopup;
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }

    // Show the popup at the current mouse position (editor or play mode)
    public void ShowPopup(IEditable _editable)
    {
        if (_editable is Aircraft aircraft)
        {

            if (popUpRoot == null) return;

            modelField.value = aircraft.aircraftProperties.model.ToString();
            typeField.value = aircraft.aircraftProperties.type.ToString();
            northField.value = aircraft.transform.position.x;
            eastField.value = aircraft.transform.position.y;
            altitudeField.value = aircraft.transform.position.z;
            noiseField.value = aircraft.aircraftProperties.noise_dBA;
            velocityField.value = aircraft.aircraftProperties.nominalVelocity_m_s;

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
    }

    public void HidePopup(IEditable _editable)
    {
        if (_editable is Aircraft aircraft)
        {
            if (popUpRoot == null) return;
            popUpRoot.AddToClassList("hidden");
        }
    }


}
