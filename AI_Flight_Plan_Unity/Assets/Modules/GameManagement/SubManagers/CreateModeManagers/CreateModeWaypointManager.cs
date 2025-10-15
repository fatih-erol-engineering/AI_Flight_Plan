using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(WaypointFactory))]
public class CreateModeWaypointManager : MonoBehaviour, IGameModeHooks
{
    [SerializeField] private Camera mainCamera;
    [field:SerializeField]
    public WaypointFactory waypointFactory { get; private set; }
    [SerializeField] private VisualTreeAsset popupUxml; // assign WaypointPopup.uxml in inspector
    [SerializeField] private LayerMask hitMask = ~0;
    private float maxDistance = 500f;
    [SerializeField] private KeyCode spawnKey = KeyCode.Mouse0;


    [Header("Preview")]
    [SerializeField] private Transform previewContainer;
    private Material previewMaterialOverride; // inspector fallback
    [SerializeField] private Theme theme; // optional, varsa theme.precreate kullanılır
    private GameObject previewInstance;

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    private VisualElement popupInstance;
    private bool popupOpen;
    private Vector3 lastHitPoint;


    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }
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

        previewMaterialOverride = theme.Preview;
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
    }


    public void Cancel()
    {
        ClosePopup();
        DestroyPreview();
    }

    public void Init()
    {
        AssignData();
        Debug.Log("Init: Create Waypoint Mode");
    }

    public bool Tick(out ExitMode exitMode)
    {
        // Bu sureci basariyla durduracak olan sey space tusu olsun        
        bool exitFlag = false;
        exitMode = ExitMode.None;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClosePopup();
            exitFlag = true;
            exitMode = ExitMode.Apply;
        }


        // Bu sureci iptal edecek olan sey ESC tusu olsun            
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
            exitFlag = true;
            exitMode = ExitMode.Cancel;
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
                    lastHitPoint = hoverHit.point;
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
                    lastHitPoint = hit.point;
                    OpenPopupAtMouse(hit.point);
                    if (previewInstance == null)
                    {
                        CreatePreview(hit.point);
                    }
                        
                }
            }
        }
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
        altField.value = hitPoint.y.ToString("F3", CultureInfo.InvariantCulture);
        timeField.value = Time.time.ToString("F2", CultureInfo.InvariantCulture);

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
            // disable runtime behaviours on preview
            var behaviours = previewInstance.GetComponentsInChildren<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
                behaviours[i].enabled = false;
        }
        else if (prefabToUse != null)
        {
            // parent yoksa world instantiate
            previewInstance = Instantiate(prefabToUse, position, Quaternion.identity);
            var behaviours = previewInstance.GetComponentsInChildren<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
                behaviours[i].enabled = false;
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
            var skinned = previewInstance.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>();
            foreach (var r in skinned)
            {
                if (r == null) continue;
                var mats = r.materials;
                var newMats = new Material[mats.Length];
                for (int i = 0; i < newMats.Length; i++) newMats[i] = previewMat;
                r.materials = newMats;
            }
        }
    }

    private void UpdatePreviewPositionWithMouse()
    {
        if (previewInstance == null || mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
        {
            previewInstance.transform.position = hit.point;
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
        TimeGame _time = new TimeGame();
        _time.SetTime(t);
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
    bool MouseHitPos(out Vector3 globalPosition)
    {
        Vector2 screen = Input.mousePosition;
        var ray = mainCamera.ScreenPointToRay(screen);
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            globalPosition = default;
            return false;
        }

        if (Physics.Raycast(ray, out var hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
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

