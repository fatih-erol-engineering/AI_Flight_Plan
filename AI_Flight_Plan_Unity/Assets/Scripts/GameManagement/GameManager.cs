using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(FreeModeController))]
[RequireComponent(typeof(CreateModeController))]
[RequireComponent(typeof(AircraftFactory))]
public class GameManager: MonoBehaviour
{
    
    public GameControllerMode mode { get;private set;} = GameControllerMode.Free;
    private GameControllerMode prevMode { get; set; } = GameControllerMode.Free;
    [SerializeField]
    private UIManager uIManager;
    private FreeModeController freeGameController;
    private CreateModeController createModeController;

    private bool isExited;

    public void Start()
    {
        mode = GameControllerMode.Free;

        freeGameController = gameObject.GetComponent<FreeModeController>();
        createModeController = gameObject.GetComponent<CreateModeController>();

        switch (mode)
        {
            case GameControllerMode.Free: 
                freeGameController.Starter();
                break;
            case GameControllerMode.Create:
                freeGameController.Starter();
                break;
            case GameControllerMode.Edit: break;
        }        
    }
    public void Update()
    {
        ChangeModeWithUI();
        switch (mode)
        {
            case GameControllerMode.Free:
                if (prevMode != mode)
                {
                    freeGameController.Starter();
                }

                freeGameController.Updater();

                break;



            case GameControllerMode.Create:
                if (prevMode != mode)
                {
                    createModeController.Starter();
                }

                createModeController.Updater();

                break;



            case GameControllerMode.Edit: 
                break;
        }

        if(createModeController.currentMode == CreateMode.None) 
        {
            mode = GameControllerMode.Free;
        }

        prevMode = mode;
    }

    private void ChangeModeWithUI()
    {        
        //if (uIManager.isCreateModeActive)
        //{
        //    mode = GameControllerMode.Create;
        //}
        //else
        //{
        //    mode = GameControllerMode.Free;
        //}
    }

}
public enum GameControllerMode
{
    Free,
    Create,
    Edit,
}
