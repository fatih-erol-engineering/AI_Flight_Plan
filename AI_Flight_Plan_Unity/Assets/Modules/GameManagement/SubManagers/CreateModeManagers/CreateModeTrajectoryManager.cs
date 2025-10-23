using System.Collections.Generic;
using UnityEngine;


public class CreateModeTrajectoryManager : MonoBehaviour, IGameModeHooks
{
    [SerializeField] private Camera mainCamera;
    [field:SerializeField]
    public TrajectoryDrawer trajectory { get; private set; }    
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateTrajectory;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }     
    public void AssignData()
    {
        CheckAssignment(trajectory);
        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);
        trajectory.AssignData();
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing: (type: {typeof(T).Name})");
    }
    public void Apply()
    {
        trajectory.Create();
    }


    public void Cancel()
    {        
    }

    public void Init()
    {
        AssignData();
        // Debug.Log("Init: Create Waypoint Mode");
    }

    public bool Tick(out ExitMode _exitMode)
    {        
        _exitMode = ExitMode.Apply;
        return true;
    }

}

