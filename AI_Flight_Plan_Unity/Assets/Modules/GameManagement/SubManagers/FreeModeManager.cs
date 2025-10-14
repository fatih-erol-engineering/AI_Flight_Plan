using UnityEngine;
public class FreeModeManager : MonoBehaviour, IGameModeHooks
{
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
        Debug.Log("Tick: Free Mode");
        exitMode = ExitMode.None;
        return false;
    }

    public ExitMode GetExitMode()
    {
        return exitMode;
    }

}

