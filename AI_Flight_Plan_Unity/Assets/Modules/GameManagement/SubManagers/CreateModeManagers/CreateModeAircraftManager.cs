using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MainGameManager))]
public class CreateModeAircraftManager : MonoBehaviour, IGameModeHooks
{    
    private MainGameManager mainGameManager;

    [SerializeField]
    private LayerMask hitMask;

    [SerializeField]
    private Camera mainCamera;

    private AircraftFactory aircraftFactory;
    private AircraftFactoryPreCreate aircraftFactoryPreCreate;
    private Aircraft aircraftCreated;
    private Aircraft aircraftPreCreate;
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }
    void Awake()
    {
        AssignData();
    }
    void AssignData()
    {
        if (!mainGameManager) mainGameManager = GetComponent<MainGameManager>();
        CheckAssignment(mainGameManager);
        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
        CheckAssignment(aircraftFactory);
        if (!aircraftFactoryPreCreate) aircraftFactoryPreCreate = GetComponent<AircraftFactoryPreCreate>();
        CheckAssignment(aircraftFactoryPreCreate);
        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }
    public void Apply()
    {
        aircraftCreated = aircraftFactory.Spawn(aircraftPreCreate.spec.model, aircraftPreCreate.aircraftVisualObject.transform.position, aircraftPreCreate.aircraftVisualObject.transform.rotation);
        aircraftFactory.SelectAircraft(aircraftCreated);
        aircraftFactoryPreCreate?.Delete();
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        aircraftFactoryPreCreate?.Delete();
    }

    public void Init()
    {
        aircraftFactoryPreCreate?.Clear();
        if (!aircraftFactoryPreCreate) aircraftFactoryPreCreate = GetComponent<AircraftFactoryPreCreate>();
        aircraftPreCreate = aircraftFactoryPreCreate.Spawn();
        Debug.Log("Init: Create Aircraft Mode");
    }

    public bool Tick()
    {
        bool exitFlag = false;
        if (MouseHitPos(out Vector3 globalPosition))
        {
            Vector3 offset = new Vector3(0f, 1f, 0f);
            aircraftPreCreate.aircraftVisualObject.transform.position = globalPosition + offset;
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

