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
    private VisualElement mainMenuTBG, createTBG; // As Toggle Group
    private Toggle createTBtn, listenTBtn, settingsTBtn, rotorTBtn, fixedWingTBtn;    
    private DropdownField createDDM;    

    public GameControllerMode gameControllerMode;

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

        
        
        // Custom Toggle Button Group Ataması
        mainMenuTBG = root.Q<VisualElement>("mainMenuTBG");
        createTBG = root.Q<VisualElement>("createTBG");

        List<Toggle> allMainMenuTBtn = mainMenuTBG.Query<Toggle>().ToList();
        List<Toggle> allCreateTBtn = createTBG.Query<Toggle>().ToList();


        // Toggle Button Ataması
        createTBtn = root.Q<Toggle>("createTBtn");
        listenTBtn = root.Q<Toggle>("listenTBtn");
        settingsTBtn = root.Q<Toggle>("settingsTBtn");
        rotorTBtn = root.Q<Toggle>("rotorTBtn");
        fixedWingTBtn = root.Q<Toggle>("fixedWingTBtn");

        createDDM = root.Q<DropdownField>("createDDM");

        // Genel Toggle Group Controlu Fonksiyonu
        createTBtn.RegisterCallback<ClickEvent>(evt => { toggleButtonClick(evt, allMainMenuTBtn, true); });
        listenTBtn.RegisterCallback<ClickEvent>(evt => { toggleButtonClick(evt, allMainMenuTBtn, true); });
        settingsTBtn.RegisterCallback<ClickEvent>(evt => { toggleButtonClick(evt, allMainMenuTBtn, true); });

        rotorTBtn.RegisterCallback<ClickEvent>(evt => { toggleButtonClick(evt, allCreateTBtn, false); });
        fixedWingTBtn.RegisterCallback<ClickEvent>(evt => { toggleButtonClick(evt, allCreateTBtn, false); });

        //rotorTBtn.RegisterCallback<ClickEvent, List<Toggle>>(toggleButtonClick, allCreateTBtn);
        //fixedWingTBtn.RegisterCallback<ClickEvent, List<Toggle>>(toggleButtonClick, allCreateTBtn);




        //createBtn.RegisterCallback<ClickEvent>(OnCreateBtnClicked);
        //rotorBtn.RegisterCallback<ClickEvent>(OnRotorBtnClicked);
        //fixedWingBtn.RegisterCallback<ClickEvent>(OnFixedWingBtnClicked);
        //aircraftListDDM.RegisterValueChangedCallback(evt =>
        //{
        //    selectedAircraftModelName = evt.newValue;                 // seçilen metin                        
        //});
        //ResetToDefault();

        //aircraftSpecRegistry = aircraftFactory.registry;
    }


    private void Update()
    {
        
    }

    private void ToggleControlTick()
    {
        
    }
    private void toggleButtonClick(ClickEvent evt,List<Toggle> toggleButtonGroup,bool allowEmptySelection)
    {
        // Uncheck Others
        Toggle currentToggle = (Toggle)evt.currentTarget;
        foreach (Toggle toggleButton in toggleButtonGroup)
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







    //void ResetToDefault()
    //{
    //    mainMenuRoot.style.display = DisplayStyle.Flex;

    //    createRoot.AddToClassList("submenusHidden");        

    //    ResetToggleButtonGroups(mainMenuTBG);
    //    ResetToggleButtonGroups(createTBG);

    //    isFreeModeActive = true;
    //    isCreateModeActive = false;
    //}
    //void ResetToggleButtonGroups(ToggleButtonGroup tbg) 
    //{
    //    if (tbg.allowEmptySelection) 
    //    { 
    //        var st = tbg.value;
    //        st.ResetAllOptions();
    //        tbg.value = st;
    //    }
    //}

    //void OnCreateBtnClicked(ClickEvent clickEvent)
    //{
    //    bool isSelected = mainMenuTBG.value[0];
    //    if (isSelected)
    //    {
    //        createRoot.RemoveFromClassList("submenusHidden");            
    //        List<string> _labels = new List<string>();
    //        foreach (AircraftSpec rotorAircrafts in aircraftSpecRegistry.rotorAircrafts)
    //        {
    //            _labels.Add(rotorAircrafts.model.ToString());
    //        }
    //        aircraftListDDM.choices = _labels;
    //        aircraftListDDM.index = 0;

    //        isFreeModeActive = false;
    //        isCreateModeActive = true;
    //    }
    //    else
    //    {
    //        ResetToDefault();
    //    }
    //}
    //void OnRotorBtnClicked(ClickEvent clickEvent)
    //{
    //    bool isSelected = createTBG.value[0];

    //    List<string> _labels = new List<string>();

    //    foreach (AircraftSpec rotorAircrafts in aircraftSpecRegistry.rotorAircrafts)
    //    {
    //        _labels.Add(rotorAircrafts.model.ToString());
    //    }
    //    aircraftListDDM.choices = _labels;
    //    aircraftListDDM.index = 0;        
    //    //aircraftListDDM.value = _labels[aircraftListDDM.index];

    //}
    //void OnFixedWingBtnClicked(ClickEvent clickEvent)
    //{
    //    bool isSelected = createTBG.value[1];
    //    List<string> _labels = new List<string>();

    //    foreach (AircraftSpec fixedWingAircrafts in aircraftSpecRegistry.fixedWingAircrafts)
    //    {
    //        _labels.Add(fixedWingAircrafts.model.ToString());
    //    }
    //    aircraftListDDM.choices = _labels;
    //    aircraftListDDM.index = 0;
    //    //aircraftListDDM.value = _labels[aircraftListDDM.index];
    //}

    //void OnAircraftListChanged(string label)
    //{
    //    selectedAircraftModelName = label;
    //}
}
