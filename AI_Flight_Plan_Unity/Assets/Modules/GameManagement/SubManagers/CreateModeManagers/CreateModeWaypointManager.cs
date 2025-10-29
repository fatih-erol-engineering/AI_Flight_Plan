using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(WaypointFactory))]
public class CreateModeWaypointManager : MonoBehaviour, IGameModeHooks
{
    [SerializeField] private Camera mainCamera;
    [field: SerializeField]
    public WaypointFactory waypointFactory { get; private set; }
    public AircraftFactory aircraftFactory;
    [SerializeField] private VisualTreeAsset popupUxml; // assign WaypointPopup.uxml in inspector
    [SerializeField] private LayerMask hitMask = ~0;
    private float maxDistance;
    [SerializeField] private KeyCode spawnKey = KeyCode.Mouse0;


    [Header("Preview")]
    private Transform previewContainer;
    private Material previewMaterialOverride; // inspector fallback
    [SerializeField] private Theme theme; // optional, varsa theme.precreate kullanılır
    private GameObject previewInstance;
    private Waypoint previewInstanceWaypoint;

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    private VisualElement popupInstance;
    private bool popupOpen;
    private Vector3 lastHitPoint;


    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateWaypoint;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }
    [SerializeField]
    private float altitudeClearanceForMouseSpawn = 0.1f;



    // altitude-drag controls for popup
    private TextField popupAltField;
    private bool isDraggingAltitude = false;
    private float altDragStartMouseY;
    private float altDragStartValue;
    [SerializeField] private float altitudeDragSensitivity = 0.02f; // world units per pixel


    public void SetUIManager(UIDocument _uIDocument)
    {
        uiDocument = _uIDocument;
    }

    public void AssignData()
    {
        if (!waypointFactory) waypointFactory = GetComponent<WaypointFactory>();
        CheckAssignment(waypointFactory);

        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);
        maxDistance = mainCamera ? mainCamera.farClipPlane : 1000f;

        if (!previewContainer) previewContainer = new GameObject("Preview").transform;
        CheckAssignment(previewContainer);

        // previewMaterialOverride = theme.Preview;
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing: (type: {typeof(T).Name})");
    }
    public void Apply()
    {
        // If popup açık ise SpawnFromPopup kullan (zaten SpawnFromPopup temizliyor)
        if (popupOpen)
        {
            SpawnFromPopup();
        }
        // waypointFactory.transform.parent.GetComponent<TrajectoryDrawer>().AssignData();
        // waypointFactory.transform.parent.GetComponent<TrajectoryDrawer>().Create();

    }


    public void Cancel()
    {
        ClosePopup();
        DestroyPreview();
    }

    public void Init()
    {
        AssignData();
        // Debug.Log("Init: Create Waypoint Mode");
    }

    public bool Tick(out ExitMode _exitMode)
    {
        // Bu sureci basariyla durduracak olan sey space tusu olsun        
        bool exitFlag = false;
        exitMode = ExitMode.None;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClosePopup();
            exitFlag = true;
            exitMode = ExitMode.Apply;
            _exitMode = exitMode;
            return exitFlag;
        }


        // Bu sureci iptal edecek olan sey ESC tusu olsun            
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
            exitFlag = true;
            exitMode = ExitMode.Cancel;
            _exitMode = exitMode;
            return exitFlag; // exitFlag = true;
        }

        // Eger Popup Menu Acik Degilse ilk tiklama ile popup acilir.
        if (!popupOpen)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                DestroyPreview();
            }
            else if (mainCamera != null)
            {
                Ray hoverRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(hoverRay, out RaycastHit hoverHit, maxDistance, hitMask))
                {
                    lastHitPoint = hoverHit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f); ;
                    if (previewInstance == null)
                    {
                        CreatePreview(lastHitPoint);
                    }
                    UpdatePreviewPositionWithMouse();
                }
                else
                {
                    DestroyPreview();
                }
            }
        }

        // Popup Menu Acik ise tiklama, enter veya tusa basma ile waypoint spawn edilir.
        if (popupOpen)
        {
            // Altitude editing with middle mouse drag while popup is open
            // Start dragging
            if (Input.GetMouseButtonDown(2))
            {
                // ensure we have reference to the alt field
                if (popupAltField == null && popupInstance != null)
                    popupAltField = popupInstance.Q<TextField>("altField");

                if (popupAltField != null)
                {
                    // parse current altitude shown in popup
                    float currentAlt = 0f;
                    float.TryParse(popupAltField.value, NumberStyles.Float, CultureInfo.InvariantCulture, out currentAlt);
                    isDraggingAltitude = true;
                    altDragStartMouseY = Input.mousePosition.y;
                    altDragStartValue = currentAlt;
                }
            }

            // While dragging, update altitude
            if (isDraggingAltitude && Input.GetMouseButton(2))
            {
                float deltaPixels = altDragStartMouseY - Input.mousePosition.y; // drag up -> increase
                float newAlt = altDragStartValue - deltaPixels * altitudeDragSensitivity;
                // clamp or snap as needed (optional)
                // update popup UI and preview position live
                if (popupAltField != null)
                    popupAltField.SetValueWithoutNotify(newAlt.ToString("F3", CultureInfo.InvariantCulture));

                // update preview's y if exists
                if (previewInstance != null)
                {
                    var p = previewInstance.transform.position;
                    p.y = newAlt;

                    previewInstance.transform.position = p;
                    GameEvents.Instance.WaypointPositionChanged(previewInstanceWaypoint, p);
                }
                lastHitPoint.y = newAlt;
            }

            // End dragging
            if (isDraggingAltitude && Input.GetMouseButtonUp(2))
            {
                isDraggingAltitude = false;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {

                }
                else
                {
                    SpawnFromPopup();
                }
            }
        }


        // Normal first click -> open popup
        if (!Input.GetKeyDown(spawnKey))
        {
            exitFlag = false;
        }
        else
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                exitFlag = false;
            }
            else
            {

                exitFlag = false;
                if (mainCamera == null)
                {
                    Debug.LogError($"[{GetType().Name}] Camera is not assigned.");
                    exitFlag = true;
                    exitMode = ExitMode.Cancel;  // DEGISTIR
                }

                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
                {
                    lastHitPoint = hit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f);
                    OpenPopupAtMouse(hit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f));
                    if (previewInstance == null)
                    {
                        CreatePreview(hit.point);
                    }

                }
            }
        }
        _exitMode = exitMode;
        return exitFlag;
    }

    private void OpenPopupAtMouse(Vector3 hitPoint)
    {
        if (popupUxml == null || uiDocument == null)
        {
            Debug.LogError($"[{GetType().Name}] popupUxml or UIDocument not assigned.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        popupInstance = popupUxml.CloneTree();
        // style positioning (absolute)
        popupInstance.style.position = Position.Absolute;
        Vector2 mouse = Input.mousePosition;
        // Convert to UI coordinates (UI root origin top-left)
        float left = Mathf.Clamp(mouse.x, 8, Screen.width - 308);
        float top = Mathf.Clamp(Screen.height - mouse.y, 8, Screen.height - 8 - 220);
        popupInstance.style.left = left;
        popupInstance.style.top = top;

        // prefill fields
        var latField = popupInstance.Q<TextField>("latField");
        var lonField = popupInstance.Q<TextField>("lonField");
        var altField = popupInstance.Q<TextField>("altField");
        var timeField = popupInstance.Q<TextField>("timeField");
        var createBtn = popupInstance.Q<Button>("createBtn");
        var cancelBtn = popupInstance.Q<Button>("cancelBtn");

        lonField.value = hitPoint.x.ToString("F3", CultureInfo.InvariantCulture);
        latField.value = hitPoint.z.ToString("F3", CultureInfo.InvariantCulture);
        altField.value = (hitPoint.y).ToString("F3", CultureInfo.InvariantCulture);
        // keep a persistent reference so other methods can update the alt field live
        popupAltField = altField;

        Waypoint prev_waypoint = aircraftFactory.selectedAircraft.trajectory.waypointFactory.waypointList.Last();
        float dist_m = (hitPoint - prev_waypoint.transform.position).magnitude;
        float aircraft_nom_Vel = aircraftFactory.selectedAircraft.aircraftProperties.nominalVelocity_m_s;
        float dt = (dist_m / aircraft_nom_Vel);
        float prevWpTime = prev_waypoint.time.second;
        float finalTime = prevWpTime + dt;
        timeField.value = finalTime.ToString("F2", CultureInfo.InvariantCulture);

        // callbacks
        createBtn.clicked += SpawnFromPopup;
        cancelBtn.clicked += ClosePopup;

        // Enter key handling on popup — use TrickleDown so TextField'ler de tuşu geçirirse yakalar
        popupInstance.RegisterCallback<KeyDownEvent>(OnPopupKeyDown, TrickleDown.TrickleDown);

        root.Add(popupInstance);
        // focus altitude
        altField.Focus();
        popupOpen = true;
    }

    private void CreatePreview(Vector3 position)
    {
        GameObject prefabToUse = null;
        if (waypointFactory != null)
            prefabToUse = waypointFactory.WaypointPrefab;

        if (prefabToUse != null && previewContainer != null)
        {
            previewInstance = Instantiate(prefabToUse, position, Quaternion.identity, previewContainer);
            previewInstance.GetComponent<WaypointShow>()?.AssignData();
            // // disable runtime behaviours on preview
            // var behaviours = previewInstance.GetComponentsInChildren<Behaviour>();
            // for (int i = 0; i < behaviours.Length; i++)
            //     behaviours[i].enabled = false;
        }
        else if (prefabToUse != null)
        {
            // parent yoksa world instantiate
            previewInstance = Instantiate(prefabToUse, position, Quaternion.identity);
            previewInstance.GetComponent<WaypointShow>()?.AssignData();
            // var behaviours = previewInstance.GetComponentsInChildren<Behaviour>();
            // for (int i = 0; i < behaviours.Length; i++)
            //     behaviours[i].enabled = false;
        }
        else
        {
            previewInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            previewInstance.transform.position = position;
            previewInstance.transform.localScale = Vector3.one * 0.4f;
            var col = previewInstance.GetComponent<Collider>();
            if (col) Destroy(col);
            previewInstance.name = "WaypointPreview";
        }

        // apply preview material if available (try theme fields/properties with fallbacks)
        Material previewMat = previewMaterialOverride;
        if (theme != null)
        {
            // try common property/field names
            var t = theme.GetType();
            var prop = t.GetProperty("Preview") ?? t.GetProperty("precreate") ?? t.GetProperty("PreCreate") ?? t.GetProperty("preCreate");
            if (prop != null) previewMat = prop.GetValue(theme) as Material ?? previewMat;
            else
            {
                var field = t.GetField("Preview") ?? t.GetField("precreate") ?? t.GetField("PreCreate") ?? t.GetField("preCreate");
                if (field != null) previewMat = field.GetValue(theme) as Material ?? previewMat;
            }
        }

        if (previewMat != null)
        {
            // handle MeshRenderer and SkinnedMeshRenderer
            var meshRenderers = previewInstance.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in meshRenderers)
            {
                if (r == null) continue;
                var mats = r.materials;
                var newMats = new Material[mats.Length];
                for (int i = 0; i < newMats.Length; i++) newMats[i] = previewMat;
                r.materials = newMats;
            }
            var lr = previewInstance.GetComponentsInChildren<LineRenderer>();
            foreach (var r in lr)
            {
                if (r == null) continue;
                var mats = r.materials;
                var newMats = new Material[mats.Length];
                for (int i = 0; i < newMats.Length; i++) newMats[i] = previewMat;
                r.materials = newMats;
            }
        }


        previewInstanceWaypoint = previewInstance.GetComponent<Waypoint>();

    }

    private void UpdatePreviewPositionWithMouse()
    {
        if (previewInstance == null || mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
        {
            previewInstance.transform.position = hit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f);
            GameEvents.Instance.WaypointPositionChanged(previewInstanceWaypoint, hit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f));
            // update vertical line: cast down from preview to find ground, else use max length
        }
    }

    private void DestroyPreview()
    {
        if (previewInstance == null) return;

        Destroy(previewInstance);
        previewInstance = null;
    }

    private void OnPopupKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            SpawnFromPopup();
            evt.StopImmediatePropagation();
            return;
        }

        if (evt.keyCode == KeyCode.Escape)
        {
            ClosePopup();
            evt.StopImmediatePropagation();
            return;
        }
    }

    private void SpawnFromPopup()
    {
        if (!popupOpen || popupInstance == null)
            return;

        if (waypointFactory == null)
        {
            Debug.LogError($"[{GetType().Name}] WaypointFactory is not assigned.");
            ClosePopup();
            return;
        }

        var latField = popupInstance.Q<TextField>("latField");
        var lonField = popupInstance.Q<TextField>("lonField");
        var altField = popupInstance.Q<TextField>("altField");
        var timeField = popupInstance.Q<TextField>("timeField");

        float lat = 0f, lon = 0f, alt = 0f, t = 0f;
        float.TryParse(latField.value, NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
        float.TryParse(lonField.value, NumberStyles.Float, CultureInfo.InvariantCulture, out lon);
        float.TryParse(altField.value, NumberStyles.Float, CultureInfo.InvariantCulture, out alt);
        float.TryParse(timeField.value, NumberStyles.Float, CultureInfo.InvariantCulture, out t);

        Vector3 spawnPos = new Vector3(lon, alt, lat);
        // preview'i yok et ve gerçek waypoint oluştur
        DestroyPreview();
        TimeGame _time = new TimeGame(t);
        waypointFactory.Spawn(spawnPos, Quaternion.identity, _time);

        ClosePopup();
    }

    private void ClosePopup()
    {
        if (popupInstance != null)
        {
            popupInstance.UnregisterCallback<KeyDownEvent>(OnPopupKeyDown, TrickleDown.TrickleDown);
            popupInstance.RemoveFromHierarchy();
            popupInstance = null;
        }
        // iptal ise preview'i temizle
        DestroyPreview();
        popupOpen = false;
    }

}

