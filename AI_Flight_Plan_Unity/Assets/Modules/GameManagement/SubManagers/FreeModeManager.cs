using UnityEngine;
public class FreeModeManager : MonoBehaviour, IGameModeHooks
{
    [SerializeField] private AircraftFactory aircraftFactory;
    private ExitMode exitMode = ExitMode.Cancel;
    public void Apply()
    {
        Debug.Log("Apply: Free Mode");
        // return false;
    }

    public void Cancel()
    {
        Debug.Log("Cancel: Free Mode");
        // return false;
    }

    public void Init()
    {
        Debug.Log("Init: Free Mode");
    }

    public bool Tick(out ExitMode exitMode)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                foreach (Aircraft aircraft in aircraftFactory.aircraftList)
                {
                    aircraft.trajectory.Clear();
                    aircraft.trajectory.Create();
                    aircraft.SetDeltaTime(new TimeGame(0f), true);                    
                }

            }
            Debug.Log("Tick: Free Mode");
        }
        exitMode = ExitMode.None;
        return false;
    }

    public ExitMode GetExitMode()
    {
        return exitMode;
    }

}

