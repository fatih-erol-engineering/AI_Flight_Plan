using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;


// [RequireComponent(typeof(MainGameManager))]
[RequireComponent(typeof(UIDocument))]
// [ExecuteAlways]
public class UIManager : MonoBehaviour
{

    //     private MainGameManager mainGameManager;
    [field: SerializeField]
    public UIDocument uIDocument { get; private set; }
    public Camera cam;
    [SerializeField] private AircraftPropertiesRegistry aircraftPropertiesRegistry;
    private VisualElement root, mainMenuRoot, createRoot,conflictSolverRoot;
    private CustomToggleButtonGroup mainMenuTBG, createTBG;
    private CustomAircraftDropdownMenu fixedWingDDM, rotorDDM;
    private Toggle createTBtn, listenTBtn, solveTBtn, settingsTBtn, rotorTBtn, fixedWingTBtn;

    public MainGameMode gameModeUI { get; private set; }
    public bool restartRequestUI { get; private set; }


    //     // For Create Mode
    public string selectedAircraftModelName;

    // void OnValidate()
    // {
    //     AssignData();
    // }
    void Awake()
    {
        AssignData();
    }
    private void Update()
    {
        restartRequestUI = false;
        UpdateSelectedAircraft();
    }
    public void UpdateSelectedAircraft()
    {
        if (rotorTBtn.value)
        {
            selectedAircraftModelName = rotorDDM.dropdownField.value;
        }
        if (fixedWingTBtn.value)
        {
            selectedAircraftModelName = fixedWingDDM.dropdownField.value;
        }
    }

    void AssignData()
    {
        // if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        // CheckAssignment(mainGameManager);

        // if (!aircraftSpecRegistry) aircraftSpecRegistry = mainGameManager?.aircraftSpecRegistry;
        // CheckAssignment(aircraftSpecRegistry);

        // Ensure an EventSystem exists if there is any UI in the scene.
        // If none exists and we detect a Canvas or UIDocument, create a simple EventSystem with StandaloneInputModule.
        // if (EventSystem.current == null)
        // {
        //     var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        // }
        if (!uIDocument) uIDocument = GetComponent<UIDocument>();
        CheckAssignment(uIDocument);

        if (!cam) cam = Camera.main;
        CheckAssignment(cam);

        root = uIDocument.rootVisualElement;
        CheckAssignment(root);

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        CheckAssignment(mainMenuRoot, "mainMenuRoot");

        createRoot = root.Q<VisualElement>("createRoot");
        CheckAssignment(createRoot, "createRoot");

        conflictSolverRoot = root.Q<VisualElement>("conflictSolverRoot");
        CheckAssignment(conflictSolverRoot, "conflictSolverRoot");

        DropdownField d = root.Q<DropdownField>("rotorDDM");
        CheckAssignment(d, "rotorDDM");

        rotorDDM = new CustomAircraftDropdownMenu(d, aircraftPropertiesRegistry.rotorAircrafts);
        CheckAssignment(rotorDDM, "aircraftPropertiesRegistry.rotorAircrafts");

        d = root.Q<DropdownField>("fixedWingDDM");
        CheckAssignment(d, "fixedWingDDM");

        fixedWingDDM = new CustomAircraftDropdownMenu(d, aircraftPropertiesRegistry.fixedWingAircrafts);
        CheckAssignment(fixedWingDDM, "aircraftPropertiesRegistry.fixedWingAircrafts");

        createTBtn = root.Q<Toggle>("createTBtn");
        CheckAssignment(createTBtn, "createTBtn");

        listenTBtn = root.Q<Toggle>("listenTBtn");
        CheckAssignment(listenTBtn, "listenTBtn");

        solveTBtn = root.Q<Toggle>("solveTBtn");
        CheckAssignment(solveTBtn, "solveTBtn");

        settingsTBtn = root.Q<Toggle>("settingsTBtn");
        CheckAssignment(settingsTBtn, "settingsTBtn");

        rotorTBtn = root.Q<Toggle>("rotorTBtn");
        CheckAssignment(rotorTBtn, "rotorTBtn");

        fixedWingTBtn = root.Q<Toggle>("fixedWingTBtn");
        CheckAssignment(fixedWingTBtn, "fixedWingTBtn");


        createTBtn.RegisterValueChangedCallback(_ => createTBtnClick());
        solveTBtn.RegisterValueChangedCallback(_ => solveTBtnClick());
        rotorTBtn.RegisterValueChangedCallback(_ => rotorTBtnChange());
        fixedWingTBtn.RegisterValueChangedCallback(_ => fixedWingTBtnChange());

        rotorDDM.dropdownField.RegisterValueChangedCallback(_ => rotorDDMChange());
        fixedWingDDM.dropdownField.RegisterValueChangedCallback(_ => fixedWingDDMChange());


        // Custom Toggle Button Groups must be declared after Registering Value Changed Callbacks
        mainMenuTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("mainMenuTBG"), true);
        createTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("createTBG"), false);
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}, name: {name} )");
    }

    void createTBtnClick()
    {
        if (createTBtn.value)
        {
            createRoot.RemoveFromClassList("submenusHidden");
            gameModeUI = MainGameMode.CreateAircraft;
        }
        else
        {
            createRoot.AddToClassList("submenusHidden");
            gameModeUI = MainGameMode.Free;
        }
    }
    void solveTBtnClick()
    {
        if (solveTBtn.value)
        {
            conflictSolverRoot.RemoveFromClassList("popupHidden");
        }
        else
        {
            conflictSolverRoot.AddToClassList("popupHidden");
        }
    }
    public void SetGameMode(MainGameMode gameMode)
    {
        gameModeUI = gameMode;
        switch (gameMode)
        {
            case MainGameMode.CreateAircraft:
                // Show Create buttons;
                createTBtn.value = true;
                break;
            case MainGameMode.Free:
                // Hide Create buttons;
                createTBtn.value = false;
                break;
        }
    }

    void rotorTBtnChange()
    {
        if (rotorTBtn.value)
        {
            fixedWingDDM.dropdownField.style.display = DisplayStyle.None;
            rotorDDM.dropdownField.style.display = DisplayStyle.Flex;
            UpdateSelectedAircraft();
            restartRequestUI = true;
        }
    }

    void fixedWingTBtnChange()
    {
        if (fixedWingTBtn.value)
        {
            rotorDDM.dropdownField.style.display = DisplayStyle.None;
            fixedWingDDM.dropdownField.style.display = DisplayStyle.Flex;
            UpdateSelectedAircraft();
            restartRequestUI = true;
        }
    }

    void rotorDDMChange()
    {
        UpdateSelectedAircraft();
        restartRequestUI = true;
    }
    void fixedWingDDMChange()
    {
        UpdateSelectedAircraft();
        restartRequestUI = true;
    }
}


public class CustomToggleButtonGroup
{
    public VisualElement toggleButtonGroupVE { get; private set; }
    public List<Toggle> toggleButtonList { get; private set; }
    public bool allowEmptySelection { get; private set; }
    public CustomToggleButtonGroup(VisualElement _toggleButtonGroupVE, bool _allowEmptySelection)
    {
        allowEmptySelection = _allowEmptySelection;
        toggleButtonGroupVE = _toggleButtonGroupVE;
        toggleButtonList = _toggleButtonGroupVE.Query<Toggle>().ToList();
        AssignCustomToggleClickToButtons();
        if (!allowEmptySelection)
        {
            toggleButtonList[0].value = true;
        }
    }

    public void AssignCustomToggleClickToButtons()
    {
        foreach (Toggle toggleButton in toggleButtonList)
        {
            toggleButton.RegisterCallback<ClickEvent>(CustomToggleClick);
        }
    }

    public void CustomToggleClick(ClickEvent evt)
    {
        // Uncheck Others
        Toggle currentToggle = (Toggle)evt.currentTarget;
        foreach (Toggle toggleButton in toggleButtonList)
        {
            if (currentToggle != toggleButton)
            {
                toggleButton.value = false;
            }
        }


        // Check for Emptiness
        bool isNotEmpty = false;
        bool clickedForChecking = currentToggle.value;
        if (clickedForChecking)
        {
            isNotEmpty = true;
        }

        bool isEmpty = !isNotEmpty;
        bool clickedForUnchecking = !clickedForChecking;
        bool notAllowEmptySelection = !allowEmptySelection;
        if (isEmpty && clickedForUnchecking && notAllowEmptySelection)
        {
            currentToggle.value = true;
        }
    }
}


public class CustomAircraftDropdownMenu
{
    public DropdownField dropdownField;
    public List<AircraftProperties> aircraftList { get; private set; }

    public CustomAircraftDropdownMenu(DropdownField _dropdownField, List<AircraftProperties> _aircraftList)
    {
        dropdownField = _dropdownField;
        aircraftList = _aircraftList;

        List<string> _labels = new List<string>();

        foreach (AircraftProperties aircraft in aircraftList)
        {
            _labels.Add(aircraft.model.ToString());
        }

        dropdownField.choices = _labels;
        dropdownField.index = 0;
    }
}