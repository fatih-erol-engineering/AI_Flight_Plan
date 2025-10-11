using System;
using UnityEngine;

[RequireComponent(typeof(AircraftFactory))]
[RequireComponent(typeof(AircraftFactoryPreCreate))]
public class CreateGameController : MonoBehaviour, IGameModeHooks
{
    [SerializeField]
    private AircraftFactory aircraftFactory;
    [SerializeField]
    private AircraftFactoryPreCreate aircraftFactoryPreCreate;
    public void Apply()
    {
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        aircraftFactoryPreCreate?.Delete();
    }

    public void Init()
    {
        if (!aircraftFactoryPreCreate) aircraftFactoryPreCreate = GetComponent<AircraftFactoryPreCreate>();
        aircraftFactoryPreCreate.Spawn();
        Debug.Log("Init: Create Mode");
    }

    public void Tick()
    {
        Debug.Log("Tick: Create Mode");
    }
}

