using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;


public class UIManager : MonoBehaviour
{

    public UIDocument uiDocument;
    public Camera cam;
    public AircraftFactory aircraftFactory;
    private AircraftSpecRegistry aircraftSpecRegistry;
    [SerializeField]
    private GameManager gameManager;

    private VisualElement root, mainMenuRoot, createRoot;
    private CustomToggleButtonGroup mainMenuTBG, createTBG;
    private CustomAircraftDropdownMenu fixedWingDDM, rotorDDM;    
    private Toggle createTBtn, listenTBtn, settingsTBtn, rotorTBtn, fixedWingTBtn;    

    public GameControllerMode gameControllerModeFromUI;

    // Aircraft Factory Icin
    public string selectedAircraftModelName;


    void Start()
    {
        AssignData();
    }
    //void Update()
    //{
    //    ResetToDefault_Check();
    //    mainMenuTBG.value = 1;
    //}


    //void ResetToDefault_Check()
    //{
    //    if (Input.GetKey(KeyCode.Escape))
    //    {
    //        ResetToDefault();
    //    }
    //}
    void AssignData()
    {   
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        if (!cam) cam = Camera.main;
        
        root = uiDocument.rootVisualElement;

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        createRoot = root.Q<VisualElement>("createRoot");

        mainMenuTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("mainMenuTBG"), true);
        createTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("createTBG"), false);

        DropdownField d = root.Q<DropdownField>("rotorDDM");
        rotorDDM = new CustomAircraftDropdownMenu(d, aircraftFactory.registry.rotorAircrafts);

        d = root.Q<DropdownField>("fixedWingDDM");
        fixedWingDDM = new CustomAircraftDropdownMenu(d, aircraftFactory.registry.fixedWingAircrafts);

        createTBtn = root.Q<Toggle>("createTBtn");
        listenTBtn = root.Q<Toggle>("listenTBtn");
        settingsTBtn = root.Q<Toggle>("settingsTBtn");
        rotorTBtn = root.Q<Toggle>("rotorTBtn");
        fixedWingTBtn = root.Q<Toggle>("fixedWingTBtn");

        createTBtn.RegisterValueChangedCallback(_ => createTBtnClick());

        rotorTBtn.RegisterValueChangedCallback(_ => rotorTBtnChange());
        fixedWingTBtn.RegisterValueChangedCallback(_ => fixedWingTBtnChange());

    }

    void createTBtnClick()
    {
        if (createTBtn.value)
        {
            createRoot.RemoveFromClassList("submenusHidden");
            gameControllerModeFromUI = GameControllerMode.Create;
            rotorTBtn.value = true;            
        }
        else
        {
            createRoot.AddToClassList("submenusHidden");
            gameControllerModeFromUI = GameControllerMode.Free;
        }
    }


    void rotorTBtnChange()
    {        
        if (rotorTBtn.value)
        {
            fixedWingDDM.dropdownField.style.display = DisplayStyle.None;
            rotorDDM.dropdownField.style.display = DisplayStyle.Flex;
        }
    }

    void fixedWingTBtnChange()
    {        
        if (fixedWingTBtn.value)
        {
            rotorDDM.dropdownField.style.display = DisplayStyle.None;
            fixedWingDDM.dropdownField.style.display = DisplayStyle.Flex;
        }
    }    
}


public class CustomToggleButtonGroup
{
    public VisualElement toggleButtonGroupVE {get; private set;}
    public List<Toggle> toggleButtonList { get; private set; }
    public bool allowEmptySelection { get; private set; }
    public CustomToggleButtonGroup(VisualElement _toggleButtonGroupVE,bool _allowEmptySelection)
    {
        allowEmptySelection = _allowEmptySelection;
        toggleButtonGroupVE = _toggleButtonGroupVE;
        toggleButtonList = _toggleButtonGroupVE.Query<Toggle>().ToList();
        AssignCustomToggleClickToButtons();
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

    public void Init()
    {
        using (var evt = ClickEvent.GetPooled())
        {
            toggleButtonList[0]?.SendEvent(evt); // Toggle kendi iç mantığıyla değerini değiştirir
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