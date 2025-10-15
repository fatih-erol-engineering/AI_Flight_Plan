using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System.Globalization;


public class CreateModeAircraftManager : MonoBehaviour, IGameModeHooks
{        
    [SerializeField] 
    private UIDocument uiDocument;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AircraftFactory aircraftFactory;    
    [SerializeField] private VisualTreeAsset popupUxml; 
    [SerializeField] private LayerMask hitMask= ~0;
    [SerializeField] private float maxDistance = 500f;
    [SerializeField] private KeyCode spawnKey = KeyCode.Mouse0;

    [Header("Preview")]    
    [SerializeField] private Transform previewContainer;
    private Material previewMaterialOverride; 
    [SerializeField] private Theme theme; 
    private GameObject previewInstance;    
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private VisualElement popupInstance;
    private bool popupOpen;
    private Vector3 lastHitPoint;

    private ExitMode exitMode;
    public ExitMode GetExitMode()
    {
        return exitMode;
    }

    void AssignData()
    {
        CheckAssignment(uiDocument);

        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
        CheckAssignment(aircraftFactory);

        if (aircraftFactory == null) aircraftFactory = GetComponent<AircraftFactory>();
        CheckAssignment(aircraftFactory);

        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);
        
        previewMaterialOverride = theme.Preview;
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }
    public void Apply()
    {
        if (popupOpen)
        {
            SpawnFromPopup();
            CheckAssignment(aircraftFactory.selectedAircraft);
            CheckAssignment(aircraftFactory.selectedAircraft.trajectory);
            aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager.Init();
            aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager.waypointFactory.Spawn(aircraftFactory.selectedAircraft.transform.position,aircraftFactory.selectedAircraft.transform.rotation, aircraftFactory.selectedAircraft.time);
            aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager.Apply();
        }
        Debug.Log("Apply: Create Mode");
    }

    public void Cancel()
    {
        ClosePopup();
        DestroyPreview();
    }
    private void SpawnFromPopup()
    {
        if (!popupOpen || popupInstance == null)
            return;

        if (aircraftFactory == null)
        {
            ClosePopup();
            CheckAssignment(aircraftFactory);
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
        aircraftFactory.Spawn(spawnPos, Quaternion.identity, _time);

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
    private void DestroyPreview()
    {
        if (previewInstance == null) return;
        Destroy(previewInstance);
        previewInstance = null;
    }
    public void Init()
    {
        AssignData();        
        Debug.Log("Init: Create Aircraft Mode");
    }

    public bool Tick(out ExitMode exitMode)
    {
        bool exitFlag = false;
        exitMode = ExitMode.None;

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
            else
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

                    // Normal first click -> open popup
                    if (Input.GetKeyDown(spawnKey))
                    {
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                        {
                            // UI elementlerinin uzerine tiklanmis ise spawn yapma
                        }
                        else
                        {
                            if (mainCamera != null)
                            {
                                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
                                {
                                    lastHitPoint = hit.point;
                                    OpenPopupAtMouse(hit.point);
                                    exitFlag = false; // Dont exit the code but end current tick.
                                    exitMode = ExitMode.None; // Dont exit the code but end current tick.
                                    return exitFlag;
                                }
                            }
                        }
                    }
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
                    // UI elementlerinin uzerine tiklanmis ise spawn yapma
                }
                else
                {
                    SpawnFromPopup();
                    exitFlag = true;
                    exitMode = ExitMode.Apply;
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
    // ...existing code...
    private void CreatePreview(Vector3 position)
    {
        // Prefab'ı al
        GameObject prefabToUse = null;
        if (aircraftFactory != null)
            prefabToUse = aircraftFactory.aircraftPrefab;

        // Instantiate (prefab varsa) veya fallback sphere oluştur
        if (prefabToUse != null)
        {
            if (previewContainer != null)
                previewInstance = Instantiate(prefabToUse, position, Quaternion.identity, previewContainer);
            else
                previewInstance = Instantiate(prefabToUse, position, Quaternion.identity);

            // runtime davranışları kapat
            var behaviours = previewInstance.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
                behaviours[i].enabled = false;

            previewInstance.name = "AircraftPreview";
        }
        else
        {
            previewInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            previewInstance.transform.position = position;
            previewInstance.transform.localScale = Vector3.one * 0.35f;
            var col = previewInstance.GetComponent<Collider>();
            if (col) Destroy(col);
            previewInstance.name = "AircraftPreview";
            if (previewContainer != null) previewInstance.transform.SetParent(previewContainer, true);
        }

        // Materyal uygula: önce previewMaterialOverride, yoksa theme içindeki olası alanlara bak
        Material previewMat = previewMaterialOverride;
        if (previewMat == null && theme != null)
        {
            var t = theme.GetType();
            var prop = t.GetProperty("precreate") ?? t.GetProperty("Preview") ?? t.GetProperty("PreCreate") ?? t.GetProperty("preview");
            if (prop != null) previewMat = prop.GetValue(theme) as Material;
            else
            {
                var field = t.GetField("precreate") ?? t.GetField("Preview") ?? t.GetField("PreCreate") ?? t.GetField("preview");
                if (field != null) previewMat = field.GetValue(theme) as Material;
            }
        }

        if (previewMat != null)
        {
            // Tüm renderer tipleri için uygula (MeshRenderer, SkinnedMeshRenderer vb.)
            var renderers = previewInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var shared = r.sharedMaterials;
                int len = Mathf.Max(1, shared.Length);
                var newMats = new Material[len];
                for (int i = 0; i < len; i++) newMats[i] = previewMat;
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

