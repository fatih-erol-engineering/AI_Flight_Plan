
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CreateMode { CreateAircraft, CreateWaypoint, CreateTrajectory }

[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(CreateModeAircraftController))]
[RequireComponent(typeof(AircraftFactory))]
[RequireComponent(typeof(AircraftFactoryPreCreate))]
[RequireComponent(typeof(MainGameManager))]
public class CreateModeController : MonoBehaviour, IGameModeHooks
{
    [SerializeField]
    private UIManager uIManager;

    [SerializeField]
    private CreateModeAircraftController createModeAircraftController;

    [SerializeField]
    private MainGameManager mainGameManager;

    [SerializeField]
    private LayerMask hitMask = ~0;

    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private AircraftFactory aircraftFactory;
    [SerializeField]
    private AircraftFactoryPreCreate aircraftFactoryPreCreate;
    private Aircraft aircraftPreCreate;
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
        if (!createModeAircraftController) createModeAircraftController = GetComponent<CreateModeAircraftController>();
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
        if (!aircraftFactoryPreCreate) aircraftFactoryPreCreate = GetComponent<AircraftFactoryPreCreate>();
        if (!mainCamera) mainCamera = Camera.main;

        ConfigureModes();        
    }
    public void InitMode(CreateMode mode)
    {        
        currentMode = mode;
        currentHooks = modes.TryGetValue(mode, out var h) ? h : null;
        currentHooks?.Init?.Invoke();        
    }

    public void Apply()
    {
        currentHooks?.Apply?.Invoke();
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        // Preshow, Created Aircraft Created Waypoint exc. all will be deleted!        
        Debug.Log("Cancel: Create Mode");
    }

    public void Init()
    {
        InitMode(CreateMode.CreateAircraft);
        Debug.Log("Init: Create Mode");
    }

    public bool Tick()
    {
        bool exitFlag = currentHooks?.Tick?.Invoke() ?? true;
        if (exitFlag)
        {
            exitMode = currentHooks?.GetExitMode?.Invoke() ?? ExitMode.Cancel;
        }
        else
        {
            // SetModeFromUI();
        }
        return exitFlag;
    }

    
    

     private void ConfigureModes()
    {
        modes = new()
        {
            [CreateMode.CreateAircraft] = new ModeHooks
            {
                Init = createModeAircraftController.Init,
                Tick = createModeAircraftController.Tick,
                Apply = createModeAircraftController.Apply,
                Cancel = createModeAircraftController.Cancel,
                GetExitMode = createModeAircraftController.GetExitMode,
            },
        };
    }
}

