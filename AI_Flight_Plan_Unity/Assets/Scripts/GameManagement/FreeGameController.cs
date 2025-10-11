using System;
using UnityEngine;
public class FreeGameController : MonoBehaviour, IGameModeHooks
{
    public void Apply()
    {
        Debug.Log("Apply: Free Mode");
    }

    public void Cancel()
    {
        Debug.Log("Cancel: Free Mode");
    }

    public void Init()
    {
        Debug.Log("Init: Free Mode");
    }

    public void Tick()
    {
        Debug.Log("Tick: Free Mode");
    }
}

