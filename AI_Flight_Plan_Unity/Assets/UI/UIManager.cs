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
    private Toggle createTBtn, listenTBtn, settingsTBtn, rotorTBtn, fixedWingTBtn;    
    private DropdownField createDDM;    

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

        createDDM = root.Q<DropdownField>("createDDM");

        mainMenuTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("mainMenuTBG"), true);
        createTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("createTBG"), false);

        createTBtn = root.Q<Toggle>("createTBtn");
        listenTBtn = root.Q<Toggle>("listenTBtn");
        settingsTBtn = root.Q<Toggle>("settingsTBtn");
        rotorTBtn = root.Q<Toggle>("rotorTBtn");
        fixedWingTBtn = root.Q<Toggle>("fixedWingTBtn");


        // Ozel Toggle Button Fonksiyonlari

        //createTBtn.RegisterCallback<ClickEvent>(Click_createTBtn);





        // Dropdown Menulerin Ilk Degerlerinin Belirlenmesi
        




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







    void Click_createTBtn(ClickEvent evt)
    {
        Toggle currentToggle = (Toggle)evt.currentTarget;
        bool isSelected = currentToggle.value;
        if (isSelected)
        {
            createRoot.RemoveFromClassList("submenusHidden");
            List<string> _labels = new List<string>();
            foreach (AircraftSpec rotorAircrafts in aircraftSpecRegistry.rotorAircrafts)
            {
                _labels.Add(rotorAircrafts.model.ToString());
            }
            createDDM.choices = _labels;
            createDDM.index = 0;
            gameControllerModeFromUI = GameControllerMode.Create;
        }
        else
        {
            createRoot.AddToClassList("submenusHidden");
            gameControllerModeFromUI = GameControllerMode.Free;            
        }
    }


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

    //void InitializeCreateList()
    //{
    //    createList
    //}

    void AssignDefaultToggleButtonFunction(List<Toggle> toggleButtonList, bool allowEmptySelection)
    {        

    }

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


public class CustomToggleButtonGroup
{
    public VisualElement toggleButtonGroupVE;
    public List<Toggle> toggleButtonList;
    public bool allowEmptySelection;
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
}