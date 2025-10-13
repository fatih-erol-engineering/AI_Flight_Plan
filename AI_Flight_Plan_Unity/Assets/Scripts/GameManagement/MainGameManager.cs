using System;
using System.Collections.Generic;
using UnityEngine;


public enum MainGameMode { Free, Create, }
public enum ExitMode { Cancel, Apply }

[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(FreeModeController))]
[RequireComponent(typeof(CreateModeController))]
[RequireComponent(typeof(AircraftFactory))]

public class MainGameManager : MonoBehaviour
{
    private UIManager uIManager;
    private FreeModeController freeModeController;
    private CreateModeController createModeController;
    private AircraftFactory aircraftFactory;
    private Dictionary<MainGameMode, ModeHooks> modes;
    public ModeHooks currentHooks;
    public MainGameMode currentMode { get; private set; } = MainGameMode.Free;
    private bool justStarted;

    void Awake()
    {
        AssignData();
    }

    void AssignData()
    {        
        if (!uIManager) uIManager = gameObject.GetComponent<UIManager>();
        if (!freeModeController) freeModeController = gameObject.GetComponent<FreeModeController>();
        if (!createModeController) createModeController = gameObject.GetComponent<CreateModeController>();
        if (!aircraftFactory) aircraftFactory = gameObject.GetComponent<AircraftFactory>();

        ConfigureModes();
        InitMode(MainGameMode.Free);
    }

    void Update()
    {
        bool exitFlag = currentHooks?.Tick?.Invoke() ?? true;
        if (exitFlag)
        {
            ChangeMode(MainGameMode.Free, currentHooks?.GetExitMode?.Invoke() ?? ExitMode.Cancel);
        }

        else
        {
            SetModeFromUI();
        }
    }

    void SetModeFromUI()
    {        
        ChangeMode(uIManager.gameModeUI,ExitMode.Cancel);
    }

    public void InitMode(MainGameMode mode)
    {        
        currentMode = mode;
        currentHooks = modes.TryGetValue(mode, out var h) ? h : null;
        currentHooks?.Init?.Invoke();        
    }

    public void ChangeMode(MainGameMode next, ExitMode exitMode)
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
            [MainGameMode.Free] = new ModeHooks
            {
                Init = freeModeController.Init,
                Tick = freeModeController.Tick,
                Apply = freeModeController.Apply,
                Cancel = freeModeController.Cancel,
                GetExitMode = freeModeController.GetExitMode,
            },

            [MainGameMode.Create] = new ModeHooks
            {
                Init = createModeController.Init,
                Tick = createModeController.Tick,
                Apply = createModeController.Apply,
                Cancel = createModeController.Cancel,
                GetExitMode = createModeController.GetExitMode,
            },
        };
    }
}


public class ModeHooks
{
    public Action Init;
    public Func<bool> Tick;
    public Action Apply;
    public Action Cancel;
    public Func<ExitMode> GetExitMode;
}