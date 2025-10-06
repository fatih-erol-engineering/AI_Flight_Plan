
using UnityEngine.UIElements;
using UnityEngine;


public class UIManager : MonoBehaviour
{

    public UIDocument uiDocument;
    public Camera cam;

    public VisualElement root, mainMenuRoot, addRoot;
    public ToggleButtonGroup mainMenuTBG, addTBG;
    public DropdownField aircraftListDDM;
    public Button addBtn, editBtn, listenBtn, settingsBtn, rotorBtn, fixedWingBtn;    
    public Slider timeSlider;


    void Start()
    {
        startData();
        ResetToDefault();
    }
    void Update()
    {
        ResetToDefault_Check();
        testCheck();        
    }
    private void testCheck()
    {
        bool idx = mainMenuTBG.value[0];
        Debug.Log(idx);
    }

    void ResetToDefault_Check()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ResetToDefault();
        }
    }
    void startData()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        if (!cam) cam = Camera.main;

        root = uiDocument.rootVisualElement;

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        addRoot = root.Q<VisualElement>("addRoot");

        mainMenuTBG = root.Q<ToggleButtonGroup>("mainTBG");
        addTBG = root.Q<ToggleButtonGroup>("addTBG");

        aircraftListDDM = root.Q<DropdownField>("aircraftListDDM");

        addBtn = root.Q<Button>("addBtn");        
        editBtn = root.Q<Button>("editBtn");
        listenBtn = root.Q<Button>("listenBtn");
        settingsBtn = root.Q<Button>("settingsBtn");
        rotorBtn = root.Q<Button>("rotorBtn");
        fixedWingBtn = root.Q<Button>("fixedWingBtn");
    }
    void ResetToDefault()
    {
        mainMenuRoot.style.display = DisplayStyle.Flex;
        addRoot.style.display = DisplayStyle.None;
    }
 




}