using System;
using UnityEngine;
public class FreeGameController : MonoBehaviour, IGameModeHooks
{
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

    public bool Tick()
    {
        Debug.Log("Tick: Free Mode");
        return false;
    }
    public ExitMode exitMode { get; set; }

}

