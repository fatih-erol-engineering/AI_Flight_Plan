using System.Collections.Generic;
using UnityEngine;


public enum MainGameMode { Free, CreateAircraft, }
public enum ExitMode { Cancel, Apply, None}


[RequireComponent(typeof(FreeModeManager))]
[RequireComponent(typeof(CreateModeManager))]
public class MainGameManager : MonoBehaviour
{
    [SerializeField]
    private UIManager uIManager;
    [SerializeField]
    private FreeModeManager freeModeController;
    [SerializeField]
    private CreateModeManager createModeManager;
    public Dictionary<MainGameMode, ModeHooks> modes{ get; private set; }
    public ModeHooks currentHooks  { get; private set; }
    public MainGameMode currentMode { get; private set; } = MainGameMode.Free;    

    void Awake()
    {
        AssignData();
    }

    void AssignData()
    {        
        CheckAssignment(uIManager);    

        if (!freeModeController) freeModeController = gameObject.GetComponent<FreeModeManager>();
        CheckAssignment(freeModeController);
        if (!createModeManager) createModeManager = gameObject.GetComponent<CreateModeManager>();
        CheckAssignment(createModeManager);

        ConfigureModes();
        InitMode(MainGameMode.Free);
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  (type: {typeof(T).Name}) is null.");
    }

    void Update()
    {
        // Öncelikle exit isteği var mı diye bakılıyor.        
        // Eger ESC tusuna basılırsa önce current Hookun cancel aktif olup ardından Free moda geçilir.
        bool exitFlag = false;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitFlag = true;
            currentHooks?.Cancel?.Invoke();
            ChangeMode(MainGameMode.Free, ExitMode.Cancel);
        }

        // Current Hook un kendi exit sinyali olabiliyor. O da tick fonksiyonu ile kontrol ediliyor.
        // Boyle bir durumda current hook exit request ediyor ve mode değişikliği yapılıyor.
        ExitMode currentHooksExitMode = ExitMode.None;
            exitFlag = currentHooks.Tick(out currentHooksExitMode);
        if (exitFlag)
        {
            // Current Hook tarafından gelen exit isteği apply ya da cancel olabilir.
            // Bu yuzden current hookun get exit mode fonksiyonu çağırılıyor. 
            ChangeMode(MainGameMode.Free, currentHooksExitMode);
        }        
        else
        {
            // UI den mode değişikliği isteği var mı diye bakılıyor.
            SetModeFromUI();
        }
        
    }

    void SetModeFromUI()
    {
        if (uIManager.restartRequestUI)
        {            
            InitMode(uIManager.gameModeUI);
        }
        else
        {
            ChangeMode(uIManager.gameModeUI, ExitMode.Cancel);
        }
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

        currentMode = next;
        currentHooks = modes.TryGetValue(next, out var h) ? h : null;
        currentHooks?.Init?.Invoke();
        uIManager.SetGameMode(currentMode);
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
            },

            [MainGameMode.CreateAircraft] = new ModeHooks
            {
                Init = createModeManager.Init,
                Tick = createModeManager.Tick,
                Apply = createModeManager.Apply,
                Cancel = createModeManager.Cancel,                
            },
        };
    }
}


