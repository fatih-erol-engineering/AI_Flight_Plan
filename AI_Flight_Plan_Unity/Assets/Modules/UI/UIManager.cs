using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

// [RequireComponent(typeof(MainGameManager))]
[RequireComponent(typeof(UIDocument))]
// //[ExecuteAlways]
public class UIManager : MonoBehaviour
{
    [SerializeField] private ConflictChecker conflictChecker;
    public UIDocument uIDocument;
    [SerializeField] private AircraftPropertiesRegistry aircraftPropertiesRegistry;
    private VisualElement root, mainMenuRoot, createRoot, conflictSolverRoot, conflictAircraftListRoot;
    [SerializeField] private VisualTreeAsset aircraftConflictFoldoutTemplate;
    private CustomToggleButtonGroup mainMenuTBG, createTBG;
    private CustomAircraftDropdownMenu fixedWingDDM, rotorDDM;
    private Toggle createTBtn, listenTBtn, solveTBtn, settingsTBtn, rotorTBtn, fixedWingTBtn;
    private Button solveConflictBtn;
    public MainGameMode gameModeUI { get; private set; }
    public bool restartRequestUI { get; private set; }
    private List<AircraftConflictManager> aircraftConflictManagers;


    //     // For Create Mode
    public string selectedAircraftModelName;
    private int updateCount = 0;

    void Awake()
    {
        AssignData();
    }

    private void Update()
    {
        if (updateCount < 4)
        {
            if (updateCount == 3)
            {
                selectedAircraftModelName = rotorDDM.dropdownField.value;
                GameEvents.Instance.ChangeAircraftPrefabWithUI(selectedAircraftModelName);
            }
            updateCount++;
        }
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

        if (!uIDocument) uIDocument = GetComponent<UIDocument>();
        CheckAssignment(uIDocument);

        root = uIDocument.rootVisualElement;
        CheckAssignment(root);

        mainMenuRoot = root.Q<VisualElement>("mainMenuRoot");
        CheckAssignment(mainMenuRoot, "mainMenuRoot");

        createRoot = root.Q<VisualElement>("createRoot");
        CheckAssignment(createRoot, "createRoot");

        conflictSolverRoot = root.Q<VisualElement>("conflictSolverRoot");
        CheckAssignment(conflictSolverRoot, "conflictSolverRoot");

        conflictAircraftListRoot = root.Q<VisualElement>("conflictAircraftListRoot");
        CheckAssignment(conflictAircraftListRoot, "conflictAircraftListRoot");

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

        solveConflictBtn = root.Q<Button>("solveConflictBtn");
        CheckAssignment(solveConflictBtn, "solveConflictBtn");

        createTBtn.RegisterValueChangedCallback(_ => createTBtnClick());
        solveTBtn.RegisterValueChangedCallback(_ => solveTBtnClick());
        rotorTBtn.RegisterValueChangedCallback(_ => rotorTBtnChange());
        fixedWingTBtn.RegisterValueChangedCallback(_ => fixedWingTBtnChange());

        rotorDDM.dropdownField.RegisterValueChangedCallback(_ => rotorDDMChange());
        fixedWingDDM.dropdownField.RegisterValueChangedCallback(_ => fixedWingDDMChange());
        solveConflictBtn.clicked += solveConflictBtnClick;

        // Custom Toggle Button Groups must be declared after Registering Value Changed Callbacks
        mainMenuTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("mainMenuTBG"), true);
        createTBG = new CustomToggleButtonGroup(root.Q<VisualElement>("createTBG"), false);

        GameEvents.Instance.OnAircraftSpawned += AddConflictSolverPopup;
        GameEvents.Instance.OnAircraftDeleted += DeleteConflictSolverPopup;
        conflictSolverRoot.AddToClassList("popupHidden");

        aircraftConflictManagers = new List<AircraftConflictManager>();

    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}, name: {name} )");
    }

    void AddConflictSolverPopup(Aircraft aircraft)
    {
        VisualElement a = aircraftConflictFoldoutTemplate.CloneTree();
        conflictAircraftListRoot.hierarchy.Add(a);
        aircraftConflictManagers.Add(new AircraftConflictManager(a, aircraft));
    }
    void DeleteConflictSolverPopup(Aircraft aircraft)
    {
        foreach (var aircraftConflictManager in aircraftConflictManagers)
        {
            if (aircraftConflictManager.aircraft == aircraft)
            {
                aircraftConflictManager.root.RemoveFromHierarchy();
                aircraftConflictManagers.Remove(aircraftConflictManager);
            }
        }
    }
    void solveConflictBtnClick()
    {
        conflictChecker.CheckConflicts();
        conflictChecker.SolveConflicts();
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
        GameEvents.Instance.ChangeAircraftPrefabWithUI(selectedAircraftModelName);
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
            GameEvents.Instance.ChangeAircraftPrefabWithUI(selectedAircraftModelName);
        }
    }

    void fixedWingTBtnChange()
    {
        if (fixedWingTBtn.value)
        {
            rotorDDM.dropdownField.style.display = DisplayStyle.None;
            fixedWingDDM.dropdownField.style.display = DisplayStyle.Flex;
            UpdateSelectedAircraft();
            GameEvents.Instance.ChangeAircraftPrefabWithUI(selectedAircraftModelName);
        }
    }

    void rotorDDMChange()
    {
        UpdateSelectedAircraft();
        GameEvents.Instance.ChangeAircraftPrefabWithUI(selectedAircraftModelName);
    }
    void fixedWingDDMChange()
    {
        UpdateSelectedAircraft();
        GameEvents.Instance.ChangeAircraftPrefabWithUI(selectedAircraftModelName);
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
        GameEvents.Instance.ChangeAircraftPrefabWithUI(_labels[0]);
    }
}

public class AircraftConflictManager
{
    public VisualElement root;
    public Foldout foldout;
    public Slider timeOrPositionChangeVal; // 0 means time can be change, 1 means position can be change
    public Slider nonEditableOrEditableVal; // 0 Non editable, 1 means editable
    public Aircraft aircraft;
    public AircraftConflictManager(VisualElement _root, Aircraft _aircraft)
    {
        root = _root;
        aircraft = _aircraft;

        foldout = root.Q<Foldout>("aircraftConflictFoldout");
        CheckAssignment(foldout, "foldout");
        foldout.text = _aircraft.aircraftProperties.model.ToString() + " - " + _aircraft.id.ToString();

        nonEditableOrEditableVal = root.Q<Slider>("nonEditableOrEditableVal");
        CheckAssignment(nonEditableOrEditableVal, "nonEditableOrEditableVal");

        timeOrPositionChangeVal = root.Q<Slider>("timeOrPositionChangeVal");
        CheckAssignment(timeOrPositionChangeVal, "timeOrPositionChangeVal");

        timeOrPositionChangeVal.RegisterValueChangedCallback(_ => OnTimeOrPositionChangeValSliderChanged());
        nonEditableOrEditableVal.RegisterValueChangedCallback(_ => OnNonEditableOrEditableValSliderChanged());
    }
    public void OnTimeOrPositionChangeValSliderChanged()
    {
        aircraft.SetTimeOrPositionChange(timeOrPositionChangeVal.value);
    }
    public void OnNonEditableOrEditableValSliderChanged()
    {
        aircraft.SetNonEditableOrEditableVal(nonEditableOrEditableVal.value);
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}, name: {name} )");
    }

}