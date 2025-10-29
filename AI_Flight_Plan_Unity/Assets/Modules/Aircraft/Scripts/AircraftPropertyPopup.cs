using UnityEngine;
using UnityEngine.UIElements;

public class AircraftPropertyPopup : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root, popUpRoot;
    private TextField modelField, typeField;
    private FloatField northField, eastField, altitudeField, noiseField;
    private Slider velocitySlider;

    void OnValidate()
    {
        AssignData();
    }
    void Start()
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
        CheckAssignment(popUpRoot);

        modelField = root.Q<TextField>("modelField");
        CheckAssignment(modelField);

        typeField = root.Q<TextField>("typeField");
        CheckAssignment(typeField);

        northField = root.Q<FloatField>("northField");
        CheckAssignment(northField);

        eastField = root.Q<FloatField>("eastField");
        CheckAssignment(eastField);

        altitudeField = root.Q<FloatField>("altitudeField");
        CheckAssignment(altitudeField);

        noiseField = root.Q<FloatField>("noiseField");
        CheckAssignment(noiseField);

        velocitySlider = root.Q<Slider>("velocitySlider");
        CheckAssignment(velocitySlider);

        GameEvents.Instance.OnEditableEnter += ShowPopup;
        GameEvents.Instance.OnEditableExit += HidePopup;
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
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
            velocitySlider.value = aircraft.aircraftProperties.nominalVelocity_m_s;

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
