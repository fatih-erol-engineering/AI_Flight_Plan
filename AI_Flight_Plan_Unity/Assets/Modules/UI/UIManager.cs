using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MainGameManager))]
[RequireComponent(typeof(UIDocument))]

public class UIManager : MonoBehaviour
{

    private MainGameManager mainGameManager;
    private UIDocument uiDocument;
    public Camera cam;
    public AircraftSpecRegistry aircraftSpecRegistry { get; private set; }
    private VisualElement root, mainMenuRoot, createRoot;
    private CustomToggleButtonGroup mainMenuTBG, createTBG;
    private CustomAircraftDropdownMenu fixedWingDDM, rotorDDM;
    private Toggle createTBtn, listenTBtn, settingsTBtn, rotorTBtn, fixedWingTBtn;

    public MainGameMode gameModeUI { get; private set; }
    public bool restartRequestUI { get; private set; }


    // For Create Mode
    public string selectedAircraftModelName;

    void Start()
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
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        CheckAssignment(mainGameManager);

        if (!aircraftSpecRegistry) aircraftSpecRegistry = mainGameManager?.aircraftSpecRegistry;
        CheckAssignment(aircraftSpecRegistry);
        

        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        CheckAssignment(uiDocument);

        if (!cam) cam = Camera.main;
        CheckAssignment(cam);

        root = uiDocument.rootVisualElement;
        CheckAssignment(root);

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        CheckAssignment(mainMenuRoot);

        createRoot = root.Q<VisualElement>("createRoot");
        CheckAssignment(createRoot);

        DropdownField d = root.Q<DropdownField>("rotorDDM");
        CheckAssignment(d);

        rotorDDM = new CustomAircraftDropdownMenu(d, aircraftSpecRegistry.rotorAircrafts);
        CheckAssignment(rotorDDM);

        d = root.Q<DropdownField>("fixedWingDDM");
        CheckAssignment(d);
        
        fixedWingDDM = new CustomAircraftDropdownMenu(d, aircraftSpecRegistry.fixedWingAircrafts);
        CheckAssignment(fixedWingDDM);

        createTBtn = root.Q<Toggle>("createTBtn");
        CheckAssignment(createTBtn);

        listenTBtn = root.Q<Toggle>("listenTBtn");
        CheckAssignment(listenTBtn);
        
        settingsTBtn = root.Q<Toggle>("settingsTBtn");
        CheckAssignment(settingsTBtn);

        rotorTBtn = root.Q<Toggle>("rotorTBtn");
        CheckAssignment(rotorTBtn);

        fixedWingTBtn = root.Q<Toggle>("fixedWingTBtn");
        CheckAssignment(fixedWingTBtn);


        createTBtn.RegisterValueChangedCallback(_ => createTBtnClick());

        rotorTBtn.RegisterValueChangedCallback(_ => rotorTBtnChange());
        fixedWingTBtn.RegisterValueChangedCallback(_ => fixedWingTBtnChange());

        rotorDDM.dropdownField.RegisterValueChangedCallback(_ => rotorDDMChange());
        fixedWingDDM.dropdownField.RegisterValueChangedCallback(_ => fixedWingDDMChange());


        // Custom Toggle Button Groups must be declared after Registering Value Changed Callbacks
        mainMenuTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("mainMenuTBG"), true);
        createTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("createTBG"), false);
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }    

    void createTBtnClick()
    {
        if (createTBtn.value)
        {
            createRoot.RemoveFromClassList("submenusHidden");
            gameModeUI = MainGameMode.Create;
        }
        else
        {
            createRoot.AddToClassList("submenusHidden");
            gameModeUI = MainGameMode.Free;
        }
    }
    public void SetGameMode(MainGameMode gameMode)
    {
        gameModeUI = gameMode;
        switch (gameMode)
        {
            case MainGameMode.Create:
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
    public List<AircraftSpec> aircraftList { get; private set; }

    public CustomAircraftDropdownMenu(DropdownField _dropdownField, List<AircraftSpec> _aircraftList)
    {
        dropdownField = _dropdownField;
        aircraftList = _aircraftList;

        List<string> _labels = new List<string>();

        foreach (AircraftSpec aircraft in aircraftList)
        {
            _labels.Add(aircraft.model.ToString());
        }

        dropdownField.choices = _labels;
        dropdownField.index = 0;
    }
}