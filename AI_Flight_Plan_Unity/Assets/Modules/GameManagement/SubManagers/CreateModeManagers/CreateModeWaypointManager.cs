using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MainGameManager))]
public class CreateModeWaypointManager : MonoBehaviour, IGameModeHooks
{    
    private MainGameManager mainGameManager;

    [SerializeField]
    private LayerMask hitMask;

    [SerializeField]
    private Camera mainCamera;
    private Trajectory selectedTrajectory;
    private Aircraft selectedAircraft;

    private AircraftFactory aircraftFactory;
    private WaypointFactory waypointFactory;
    private WaypointFactoryPreCreate waypointFactoryPreCreate;
    private Waypoint waypointCreated;
    private Waypoint waypointPreCreate;
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }

    void AssignData()
    {
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        CheckAssignment(mainGameManager);

        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
        CheckAssignment(aircraftFactory);

        if (!waypointFactory) waypointFactory = GetComponent<WaypointFactory>();
        CheckAssignment(waypointFactory);

        if (!waypointFactoryPreCreate) waypointFactoryPreCreate = GetComponent<WaypointFactoryPreCreate>();
        CheckAssignment(waypointFactoryPreCreate);

        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing: (type: {typeof(T).Name})");
    }
    public void Apply()
    {
        waypointCreated = waypointFactory.Spawn(aircraftFactory.selectedAircraft,waypointPreCreate.transform.position, waypointPreCreate.transform.rotation);
        waypointFactoryPreCreate?.Delete();
        Debug.Log("Apply: Create Waypoint Mode");
    }

    public void Cancel()
    {
        waypointFactoryPreCreate?.Delete();
    }

    public void Init()
    {
        AssignData();
        selectedAircraft = aircraftFactory.selectedAircraft;
        CheckAssignment(selectedAircraft);

        selectedTrajectory = aircraftFactory.selectedAircraft?.trajectory;
        CheckAssignment(selectedTrajectory);
        selectedTrajectory?.Clear();

        waypointFactory.waypointContainer = selectedTrajectory?.waypointContainer;
        waypointFactory.Init();
        CheckAssignment(waypointFactory.waypointContainer);

        if (!waypointFactoryPreCreate) waypointFactoryPreCreate = GetComponent<WaypointFactoryPreCreate>();
        waypointFactoryPreCreate.Init();
        waypointPreCreate = waypointFactoryPreCreate.Spawn(aircraftFactory.selectedAircraft);
        
        Debug.Log("Init: Create Waypoint Mode");
    }

    public bool Tick()
    {
        bool exitFlag = false;
        if (MouseHitPos(out Vector3 globalPosition))
        {
            Vector3 offset = new Vector3(0f, 1f, 0f);
            waypointPreCreate.transform.position = globalPosition + offset;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitFlag = true;
            exitMode = ExitMode.Cancel;
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            exitFlag = true;
            exitMode = ExitMode.Apply;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            {

            }
            else
            {
                exitFlag = true;
                exitMode = ExitMode.Apply;
            }
        }
        return exitFlag;
    }

    bool MouseHitPos(out Vector3 globalPosition)
    {
        Vector2 screen = Input.mousePosition;
        var ray = mainCamera.ScreenPointToRay(screen);
        float maxDist = mainCamera ? mainCamera.farClipPlane : 1000f;
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            globalPosition = default;
            return false;
        }

        if (Physics.Raycast(ray, out var hit, maxDist, hitMask, QueryTriggerInteraction.Collide))
        {
            globalPosition = hit.point;
            return true;
        }
        else
        {
            var plane = new Plane(Vector3.up, new Vector3(0, 0, 0));
            if (plane.Raycast(ray, out float enter))
            {
                globalPosition = ray.GetPoint(enter);
                return true;
            }
        }
        globalPosition = default;
        return false;
    }
}

