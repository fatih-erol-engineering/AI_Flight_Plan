using System;
using System.Collections.Generic;
using UnityEngine;


public enum MainGameMode { Free, Create}

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

    void Awake()
    {
        if (!uIManager) uIManager = gameObject.GetComponent<UIManager>();
        if (!freeGameController) freeGameController = gameObject.GetComponent<FreeGameController>();
        if (!createGameController) createGameController = gameObject.GetComponent<CreateGameController>();
        if (!aircraftFactory) aircraftFactory = gameObject.GetComponent<AircraftFactory>();

        ConfigureModes();
        SetMode(MainGameMode.Free, true); // örnek başlangıç
    }

    void Update()
    {
        // 1) Her frame aktif modu çalıştır
        currentHooks?.Tick?.Invoke();

        // 2) ESC ile interrupt
        if (Input.GetKeyDown(KeyCode.Escape))
            Interrupt(); // Null/None moda dön
    }

    public void SetMode(MainGameMode next, bool isCompleted)
    {
        if (next == currentMode) return;

        if (isCompleted)
        {
            currentHooks?.Apply?.Invoke();
        }
        else
        {
            currentHooks?.Cancel?.Invoke();
        }
        currentMode = next;
        currentHooks = modes.TryGetValue(next, out var h) ? h : null;
        currentHooks?.Init?.Invoke();
        OnModeChanged?.Invoke(next);
    }
 
    public void Interrupt()
    {
        // Mod özelinde bir “iptal” varsa önce onu çağır
        currentHooks?.Cancel?.Invoke();

        // Ardından Null moda geç
        SetMode(MainGameMode.Free,false);
    }

    public event Action<MainGameMode> OnModeChanged;

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
            },

            [MainGameMode.Create] = new MainGameModeHooks
            {
                Init = () => createGameController.Init(),
                Tick = () => createGameController.Tick(),
                Apply = () => createGameController.Apply(),                
                Cancel = () => createGameController.Cancel(),                
            },
        };
    }
}


public class MainGameModeHooks
{
    public Action Init;
    public Action Tick;
    public Action Apply;
    public Action Cancel;
}