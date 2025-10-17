
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public enum CreateMode { CreateAircraft, CreateWaypoint, CreateTrajectory}

public class CreateModeManager : MonoBehaviour, IGameModeHooks
{

    [SerializeField] UIManager uIManager;
    
    [SerializeField] private CreateModeAircraftManager createModeAircraftManager;
    [SerializeField] private CreateModeWaypointManager createModeWaypointManager;   
    [SerializeField] private CreateModeTrajectoryManager createModeTrajectoryManager;   

    [SerializeField] private AircraftFactory aircraftFactory;        
    [SerializeField] private Camera mainCamera;        
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks;
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }

    [field: SerializeField] public bool trajectoryCreatedFlag { get; private set; } = false;    

    void Awake()
    {
        AssignData();
    }
    void AssignData()
    {
        if (!uIManager) uIManager = GetComponent<UIManager>();
        CheckAssignment(uIManager);
        
        CheckAssignment(createModeAircraftManager);        
        CheckAssignment(aircraftFactory);

        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);

        ConfigureModes();
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }

    public void Apply()
    {
        currentHooks?.Apply?.Invoke();
        trajectoryCreatedFlag = true;
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        currentHooks?.Cancel?.Invoke();
        trajectoryCreatedFlag = false;
        Debug.Log("Cancel: Create Mode");
    }

    public void Init()
    {
        trajectoryCreatedFlag = false;
        InitMode(CreateMode.CreateAircraft);
        Debug.Log("Init: Create Mode");
    }

    public bool Tick(out ExitMode _exitMode)
    {
        trajectoryCreatedFlag = false;
        bool subExitFlag = false;
        ExitMode subExitMode = ExitMode.None;

        bool exitFlag = false;
        exitMode = ExitMode.None;    
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitFlag = true;
            currentHooks?.Cancel?.Invoke();
            exitMode = ExitMode.Cancel;
            ChangeMode(CreateMode.CreateAircraft, exitMode);
        }


        switch (currentMode)
        {
            case CreateMode.CreateAircraft:
                subExitFlag = currentHooks.Tick(out subExitMode);
                if (subExitFlag)
                {
                    UpdateCreateWaypointMode();
                    ChangeMode(CreateMode.CreateWaypoint, subExitMode);
                }
                break;

            case CreateMode.CreateWaypoint:
                subExitFlag = currentHooks.Tick(out subExitMode);
                if (subExitFlag)
                {
                    UpdateCreateTrajectoryMode();                    
                    ChangeMode(CreateMode.CreateTrajectory, subExitMode);
                }
                break;

                case CreateMode.CreateTrajectory:
                    exitFlag = currentHooks.Tick(out subExitMode);
                    if (exitFlag)
                    {
                        exitMode = subExitMode;
                    }
                    break;
        }
        _exitMode = exitMode;
        return exitFlag;
    }

    public void InitMode(CreateMode mode)
    {        
        currentMode = mode;
        currentHooks = modes.TryGetValue(mode, out var h) ? h : null;
        currentHooks?.Init?.Invoke();        
    }

    public void ChangeMode(CreateMode next, ExitMode exitMode)
    {
        if ((next == currentMode) && (currentHooks != null)) return;

        switch (exitMode)
        {
            case ExitMode.Cancel:
                currentHooks?.Cancel?.Invoke();
                break;
            case ExitMode.Apply:
                currentHooks?.Apply?.Invoke();
                break;
        }

        currentMode = next;
        currentHooks = modes.TryGetValue(next, out var h) ? h : null;
        currentHooks?.Init?.Invoke();        
    }
    
     private void ConfigureModes()
    {
        modes = new()
        {
            [CreateMode.CreateAircraft] = new ModeHooks
            {
                modeName = "CreateAircraft",
                Init = createModeAircraftManager.Init,
                Tick = createModeAircraftManager.Tick,
                Apply = createModeAircraftManager.Apply,
                Cancel = createModeAircraftManager.Cancel,
                GetExitMode = createModeAircraftManager.GetExitMode,
            },
            [CreateMode.CreateWaypoint] = new ModeHooks
            {
                modeName = "CreateWaypoint",
                Init = () => { /* Place Holder*/},
                Tick = (out ExitMode _out) => { _out = ExitMode.None; return false; },
                Apply = () => { /* Place Holder*/},
                Cancel = () => { /* Place Holder*/},
                GetExitMode = () => ExitMode.None,
            },
            [CreateMode.CreateTrajectory] = new ModeHooks
            {
                modeName = "CreateTrajectory",
                Init = ()=> { /* Place Holder*/},
                Tick = (out ExitMode _out )=> { _out = ExitMode.None; return false; },
                Apply = ()=> { /* Place Holder*/},
                Cancel = ()=> { /* Place Holder*/},
                GetExitMode =  ()=> ExitMode.None,
            },
        };
    }
    private void UpdateCreateWaypointMode()
    {
        createModeWaypointManager = aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager;
        modes[CreateMode.CreateWaypoint] = new ModeHooks
        {
            modeName = "CreateWaypoint",
            Init = createModeWaypointManager.Init,
            Tick = createModeWaypointManager.Tick,
            Apply = createModeWaypointManager.Apply,
            Cancel = createModeWaypointManager.Cancel,
            GetExitMode = createModeWaypointManager.GetExitMode,
        };
        createModeWaypointManager.aircraftFactory = createModeAircraftManager.aircraftFactory;
        createModeWaypointManager.SetUIManager(uIManager.uIDocument);
    }
    private void UpdateCreateTrajectoryMode()
    {
        createModeTrajectoryManager = aircraftFactory.selectedAircraft.trajectory.transform.GetComponent<CreateModeTrajectoryManager>(); 
        modes[CreateMode.CreateTrajectory] = new ModeHooks
        {
            modeName = "CreateTrajectory",
            Init = createModeTrajectoryManager.Init,
            Tick = createModeTrajectoryManager.Tick,
            Apply = createModeTrajectoryManager.Apply,
            Cancel = createModeTrajectoryManager.Cancel,
            GetExitMode = createModeTrajectoryManager.GetExitMode,
        };        
    }
}

