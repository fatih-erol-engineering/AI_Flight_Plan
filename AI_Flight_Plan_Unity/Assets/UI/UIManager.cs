
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;




public class UIManager : MonoBehaviour
{

    public UIDocument uiDocument;
    public Camera cam;
    public AircraftSpecRegistry aircraftSpecRegistry;

    private VisualElement root, mainMenuRoot, addRoot;
    private ToggleButtonGroup mainMenuTBG, addTBG;
    private DropdownField aircraftListDDM;
    private Button addBtn, editBtn, listenBtn, settingsBtn, rotorBtn, fixedWingBtn;    
    private Slider timeSlider;
    

    void Start()
    {
        BindData();        
    }
    void Update()
    {
        ResetToDefault_Check();      
    }


    void ResetToDefault_Check()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ResetToDefault();
        }
    }
    void BindData()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        if (!cam) cam = Camera.main;

        root = uiDocument.rootVisualElement;

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        addRoot = root.Q<VisualElement>("addRoot");

        mainMenuTBG = root.Q<ToggleButtonGroup>("mainTBG");
        addTBG = root.Q<ToggleButtonGroup>("addTBG");
        
        addBtn = root.Q<Button>("addBtn");        
        editBtn = root.Q<Button>("editBtn");
        listenBtn = root.Q<Button>("listenBtn");
        settingsBtn = root.Q<Button>("settingsBtn");
        rotorBtn = root.Q<Button>("rotorBtn");
        fixedWingBtn = root.Q<Button>("fixedWingBtn");

        aircraftListDDM = root.Q<DropdownField>("aircraftListDDM");

        addBtn.RegisterCallback<ClickEvent>(OnAddBtnClicked);
        rotorBtn.RegisterCallback<ClickEvent>(OnRotorBtnClicked);
        fixedWingBtn.RegisterCallback<ClickEvent>(OnFixedWingBtnClicked);        
        editBtn.RegisterCallback<ClickEvent>(OnEditBtnClicked);
        ResetToDefault();




        

        // 
    }
    void ResetToDefault()
    {
        mainMenuRoot.style.display = DisplayStyle.Flex;

        addRoot.AddToClassList("submenusHidden");
        aircraftListDDM.AddToClassList("submenusHidden");

        ResetToggleButtonGroups(mainMenuTBG);
        ResetToggleButtonGroups(addTBG);        
    }
    void ResetToggleButtonGroups(ToggleButtonGroup tbg) 
    {
        var st = tbg.value;
        st.ResetAllOptions();
        tbg.value = st;
    }

    void OnAddBtnClicked(ClickEvent clickEvent)
    {
        bool isSelected = mainMenuTBG.value[0];
        if (isSelected)
        {
            addRoot.RemoveFromClassList("submenusHidden");                        
        }
        else
        {
            addRoot.AddToClassList("submenusHidden");
            aircraftListDDM.AddToClassList("submenusHidden");
            ResetToggleButtonGroups(addTBG);
        }
    }
    void OnRotorBtnClicked(ClickEvent clickEvent)
    {
        bool isSelected = addTBG.value[0];
        if (isSelected)
        {
            aircraftListDDM.RemoveFromClassList("submenusHidden");
        }
        else
        {
            aircraftListDDM.AddToClassList("submenusHidden");
        }

        List<string> _labels = new List<string>();

        foreach (AircraftSpec rotorAircrafts in aircraftSpecRegistry.rotorAircrafts)
        {
            _labels.Add(rotorAircrafts.model.ToString());
        }
        aircraftListDDM.choices = _labels;
        aircraftListDDM.index = 0;
        //aircraftListDDM.value = _labels[aircraftListDDM.index];

    }
    void OnFixedWingBtnClicked(ClickEvent clickEvent)
    {
        bool isSelected = addTBG.value[1];
        if (isSelected)
        {
            aircraftListDDM.RemoveFromClassList("submenusHidden");
        }
        else
        {
            aircraftListDDM.AddToClassList("submenusHidden");
        }
        List<string> _labels = new List<string>();

        foreach (AircraftSpec fixedWingAircrafts in aircraftSpecRegistry.fixedWingAircrafts)
        {
            _labels.Add(fixedWingAircrafts.model.ToString());
        }
        aircraftListDDM.choices = _labels;
        aircraftListDDM.index = 0;
        //aircraftListDDM.value = _labels[aircraftListDDM.index];
    }

    void OnEditBtnClicked(ClickEvent clickEvent)
    {
        bool isSelected = mainMenuTBG.value[1];
        if (isSelected)
        {
            addRoot.AddToClassList("submenusHidden");
            aircraftListDDM.AddToClassList("submenusHidden");
            ResetToggleButtonGroups(addTBG);
        }
    }

}
