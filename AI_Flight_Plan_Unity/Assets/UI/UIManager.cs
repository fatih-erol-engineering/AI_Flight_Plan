using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{

    private UIDocument uiDocument;
    public Camera cam;
    public AircraftSpecRegistry aircraftSpecRegistry;
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
    void UpdateSelectedAircraft()
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
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        if (!cam) cam = Camera.main;

        root = uiDocument.rootVisualElement;

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        createRoot = root.Q<VisualElement>("createRoot");

        DropdownField d = root.Q<DropdownField>("rotorDDM");
        rotorDDM = new CustomAircraftDropdownMenu(d, aircraftSpecRegistry.rotorAircrafts);

        d = root.Q<DropdownField>("fixedWingDDM");
        fixedWingDDM = new CustomAircraftDropdownMenu(d, aircraftSpecRegistry.fixedWingAircrafts);

        createTBtn = root.Q<Toggle>("createTBtn");
        listenTBtn = root.Q<Toggle>("listenTBtn");
        settingsTBtn = root.Q<Toggle>("settingsTBtn");
        rotorTBtn = root.Q<Toggle>("rotorTBtn");
        fixedWingTBtn = root.Q<Toggle>("fixedWingTBtn");

        createTBtn.RegisterValueChangedCallback(_ => createTBtnClick());

        rotorTBtn.RegisterValueChangedCallback(_ => rotorTBtnChange());
        fixedWingTBtn.RegisterValueChangedCallback(_ => fixedWingTBtnChange());

        rotorDDM.dropdownField.RegisterValueChangedCallback(_ => rotorDDMChange());
        fixedWingDDM.dropdownField.RegisterValueChangedCallback(_ => fixedWingDDMChange());


        // Custom Toggle Button Groups must be declared after Registering Value Changed Callbacks
        mainMenuTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("mainMenuTBG"), true);
        createTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("createTBG"), false);
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


    void rotorTBtnChange()
    {
        if (rotorTBtn.value)
        {
            fixedWingDDM.dropdownField.style.display = DisplayStyle.None;
            rotorDDM.dropdownField.style.display = DisplayStyle.Flex;
            restartRequestUI = true;
        }
    }

    void fixedWingTBtnChange()
    {
        if (fixedWingTBtn.value)
        {
            rotorDDM.dropdownField.style.display = DisplayStyle.None;
            fixedWingDDM.dropdownField.style.display = DisplayStyle.Flex;
            restartRequestUI = true;
        }
    }

    void rotorDDMChange()
    {
        restartRequestUI = true;
    }
    void fixedWingDDMChange()
    {
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