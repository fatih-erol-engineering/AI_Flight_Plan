using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;


public enum MainGameMode { Free, Create, }
public enum ExitMode { Cancel, Apply }

[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(FreeGameController))]
[RequireComponent(typeof(CreateGameController))]
[RequireComponent(typeof(AircraftFactory))]

public class MainGameManager : MonoBehaviour
{
    private UIManager uIManager;
    private FreeGameController freeGameController;
    private CreateGameController createGameController;
    private AircraftFactory aircraftFactory;
    private Dictionary<MainGameMode, MainGameModeHooks> modes;
    public MainGameModeHooks currentHooks;
    public MainGameMode currentMode { get; private set; } = MainGameMode.Free;
    private bool justStarted;

    void Awake()
    {
        if (!uIManager) uIManager = gameObject.GetComponent<UIManager>();
        if (!freeGameController) freeGameController = gameObject.GetComponent<FreeGameController>();
        if (!createGameController) createGameController = gameObject.GetComponent<CreateGameController>();
        if (!aircraftFactory) aircraftFactory = gameObject.GetComponent<AircraftFactory>();

        ConfigureModes();
        InitMode(MainGameMode.Free);
        justStarted = true;
    }

    void Update()
    {
        bool exitFlag = currentHooks?.Tick?.Invoke() ?? true;
        if (exitFlag)
        {
            ChangeMode(MainGameMode.Free, currentHooks.exitMode);
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

        switch (currentHooks.exitMode)
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
            [MainGameMode.Free] = new MainGameModeHooks
            {
                Init = () => freeGameController.Init(),
                Tick = () => freeGameController.Tick(),
                Apply = () => freeGameController.Apply(),
                Cancel = () => freeGameController.Cancel(),
                exitMode = freeGameController.exitMode,
            },

            [MainGameMode.Create] = new MainGameModeHooks
            {
                Init = () => createGameController.Init(),
                Tick = () => createGameController.Tick(),
                Apply = () => createGameController.Apply(),
                Cancel = () => createGameController.Cancel(),
                exitMode = createGameController.exitMode,
            },
        };
    }
}


public class MainGameModeHooks
{
    public Action Init;
    public Func<bool> Tick;
    public Action Apply;
    public Action Cancel;
    public ExitMode exitMode;
}