
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreateModeAircraftController : MonoBehaviour, IGameModeHooks
{
    [SerializeField]
    private MainGameManager mainGameManager;

    [SerializeField]
    private LayerMask hitMask;

    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private AircraftFactory aircraftFactory;
    [SerializeField]
    private AircraftFactoryPreCreate aircraftFactoryPreCreate;
    private Aircraft aircraftCreated;
    private Aircraft aircraftPreCreate;
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks;
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
        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
        if (!aircraftFactoryPreCreate) aircraftFactoryPreCreate = GetComponent<AircraftFactoryPreCreate>();
        if (!mainCamera) mainCamera = Camera.main;        
    }
    public void Apply()
    {        
        aircraftCreated = aircraftFactory.Spawn(aircraftPreCreate.spec.model,aircraftPreCreate.aircraftVisualObject.transform.position,aircraftPreCreate.aircraftVisualObject.transform.rotation);        
        aircraftFactoryPreCreate?.Delete();        
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        aircraftFactoryPreCreate?.Delete();
    }

    public void Init()
    {
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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
        {
            exitFlag = true;
            exitMode = ExitMode.Apply;
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

