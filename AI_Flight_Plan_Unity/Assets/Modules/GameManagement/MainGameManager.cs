// using System.Collections.Generic;
// using UnityEngine;


// public enum MainGameMode { Free, Create, }
// public enum ExitMode { Cancel, Apply }

// [RequireComponent(typeof(UIManager))]
// [RequireComponent(typeof(FreeModeManager))]
// [RequireComponent(typeof(CreateModeManager))]
// [RequireComponent(typeof(AircraftFactory))]

// public class MainGameManager : MonoBehaviour
// {
//     [field: SerializeField]
//     public AircraftSpecRegistry aircraftSpecRegistry { get; private set; }    
//     [field: SerializeField]
//     public Theme theme{ get; private set; }
//     public UIManager uIManager{ get; private set; }
//     public FreeModeManager freeModeController{ get; private set; }
//     public CreateModeManager createModeController{ get; private set; }
//     public AircraftFactory aircraftFactory{ get; private set; }
//     public Dictionary<MainGameMode, ModeHooks> modes{ get; private set; }
//     public ModeHooks currentHooks  { get; private set; }
//     public MainGameMode currentMode { get; private set; } = MainGameMode.Free;    

//     void Awake()
//     {
//         AssignData();
//     }

//     void AssignData()
//     {
//         if (!uIManager) uIManager = gameObject.GetComponent<UIManager>();
//         CheckAssignment(uIManager);    

//         if (!freeModeController) freeModeController = gameObject.GetComponent<FreeModeManager>();
//         CheckAssignment(freeModeController);

//         if (!createModeController) createModeController = gameObject.GetComponent<CreateModeManager>();
//         CheckAssignment(createModeController);

//         if (!aircraftFactory) aircraftFactory = gameObject.GetComponent<AircraftFactory>();
//         CheckAssignment(aircraftFactory);

//         ConfigureModes();
//         InitMode(MainGameMode.Free);
//     }
//     void CheckAssignment<T>(T obj)
//     {
//         if (obj == null)
//             Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
//     }

//     void Update()
//     {
//         // Öncelikle exit isteği var mı diye bakılıyor.        
//         // Eger ESC tusuna basılırsa önce current Hookun cancel aktif olup ardından Free moda geçilir.
//         bool exitFlag = false;
//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             exitFlag = true;
//             currentHooks?.Cancel?.Invoke();
//             ChangeMode(MainGameMode.Free, ExitMode.Cancel);
//         }
        
//         // Current Hook un kendi exit sinyali olabiliyor. O da tick fonksiyonu ile kontrol ediliyor.
//         // Boyle bir durumda current hook exit request ediyor ve mode değişikliği yapılıyor.
//         exitFlag = currentHooks?.Tick?.Invoke() ?? true;
//         if (exitFlag)
//         {
//             // Current Hook tarafından gelen exit isteği apply ya da cancel olabilir.
//             // Bu yuzden current hookun get exit mode fonksiyonu çağırılıyor. 
//             ChangeMode(MainGameMode.Free, currentHooks?.GetExitMode?.Invoke() ?? ExitMode.Cancel);
//         }        
//         else
//         {
//             // UI den mode değişikliği isteği var mı diye bakılıyor.
//             SetModeFromUI();
//         }
        
//     }

//     void SetModeFromUI()
//     {
//         if (uIManager.restartRequestUI)
//         {            
//             InitMode(uIManager.gameModeUI);
//         }
//         else
//         {
//             ChangeMode(uIManager.gameModeUI, ExitMode.Cancel);
//         }
//     }

//     public void InitMode(MainGameMode mode)
//     {        
//         currentMode = mode;
//         currentHooks = modes.TryGetValue(mode, out var h) ? h : null;
//         currentHooks?.Init?.Invoke();        
//     }

//     public void ChangeMode(MainGameMode next, ExitMode exitMode)
//     {
//         if ((next == currentMode) && (currentHooks != null)) return;

//         switch (currentHooks?.GetExitMode?.Invoke() ?? ExitMode.Cancel)
//         {
//             case ExitMode.Cancel:
//                 currentHooks?.Cancel?.Invoke();
//                 break;
//             case ExitMode.Apply:
//                 currentHooks?.Apply?.Invoke();
//                 break;
//         }

//         currentMode = next;
//         currentHooks = modes.TryGetValue(next, out var h) ? h : null;
//         currentHooks?.Init?.Invoke();
//         uIManager.SetGameMode(currentMode);
//     }
    
//     private void ConfigureModes()
//     {
//         modes = new()
//         {
//             [MainGameMode.Free] = new ModeHooks
//             {
//                 Init = freeModeController.Init,
//                 Tick = freeModeController.Tick,
//                 Apply = freeModeController.Apply,
//                 Cancel = freeModeController.Cancel,
//                 GetExitMode = freeModeController.GetExitMode,
//             },

//             [MainGameMode.Create] = new ModeHooks
//             {
//                 Init = createModeController.Init,
//                 Tick = createModeController.Tick,
//                 Apply = createModeController.Apply,
//                 Cancel = createModeController.Cancel,
//                 GetExitMode = createModeController.GetExitMode,
//             },
//         };
//     }
// }


