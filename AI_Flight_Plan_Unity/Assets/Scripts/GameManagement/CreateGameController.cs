using System;
using UnityEngine;
public class CreateGameController : MonoBehaviour, IGameModeHooks
{
    [SerializeField]
    private AircraftFactory aircraftFactory;
    public Action Apply()
    {
        throw new NotImplementedException();
    }

    public Action Cancel()
    {
        throw new NotImplementedException();
    }

    public Action Init()
    {
        if(!aircraftFactory) aircraftFactory=GetComponent<AircraftFactory>();

    }

    public Action Tick()
    {
        throw new NotImplementedException();
    }
}

