
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CreateMode { CreateAircraft, CreateWaypoint, CreateTrajectory}

[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(CreateModeAircraftManager))]
[RequireComponent(typeof(CreateModeWaypointManager))]
[RequireComponent(typeof(AircraftFactory))]
[RequireComponent(typeof(MainGameManager))]
public class CreateModeManager : MonoBehaviour, IGameModeHooks
{
    
    private UIManager uIManager;
    private CreateModeAircraftManager createModeAircraftManager;    
    private CreateModeWaypointManager createModeWaypointManager;   
    private MainGameManager mainGameManager;    
    private AircraftFactory aircraftFactory;        
    private Camera mainCamera;        
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks;
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }

    void Awake()
    {
        AssignData();
    }
    void AssignData()
    {
        if (!uIManager) uIManager = GetComponent<UIManager>();
        CheckAssignment(uIManager);

        if (!createModeAircraftManager) createModeAircraftManager = GetComponent<CreateModeAircraftManager>();
        CheckAssignment(createModeAircraftManager);

        if (!createModeWaypointManager) createModeWaypointManager = GetComponent<CreateModeWaypointManager>();
        CheckAssignment(createModeWaypointManager);
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        CheckAssignment(mainGameManager);

        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
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
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        currentHooks?.Cancel?.Invoke();     
        
        Debug.Log("Cancel: Create Mode");
    }

    public void Init()
    {
        InitMode(CreateMode.CreateAircraft);
        Debug.Log("Init: Create Mode");
    }

    public bool Tick(out ExitMode exitMode)
    {
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
                    ChangeMode(CreateMode.CreateWaypoint, subExitMode);
                }
                break;

            case CreateMode.CreateWaypoint:
                subExitFlag = currentHooks.Tick(out subExitMode);
                if (subExitFlag)
                {                    
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

        switch (currentHooks?.GetExitMode?.Invoke() ?? ExitMode.Cancel)
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
                Init = createModeAircraftManager.Init,
                Tick = createModeAircraftManager.Tick,
                Apply = createModeAircraftManager.Apply,
                Cancel = createModeAircraftManager.Cancel,
                GetExitMode = createModeAircraftManager.GetExitMode,
            },
            [CreateMode.CreateWaypoint] = new ModeHooks
            {
                Init = createModeWaypointManager.Init,
                Tick = createModeWaypointManager.Tick,
                Apply = createModeWaypointManager.Apply,
                Cancel = createModeWaypointManager.Cancel,
                GetExitMode = createModeWaypointManager.GetExitMode,
            },
        };
    }
}

