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
    [field: SerializeField] public AircraftFactory aircraftFactory { get; private set; }
    [SerializeField] private VisualTreeAsset popupUxml;
    [SerializeField] private LayerMask hitMask = ~0;
    private float maxDistance;
    [SerializeField] private KeyCode spawnKey = KeyCode.Mouse0;

    [Header("Preview")]
    private Transform previewContainer;
    private Material previewMaterialOverride;
    [SerializeField] private Theme theme;
    private GameObject previewInstance;
    // vertical line under preview (same material as preview)
    private GameObject previewLine;
    [SerializeField] private float previewLineWidth = 0.3f;
    [SerializeField] private float previewLineMaxLength = 10000f;
    private Dictionary<CreateMode, ModeHooks> modes;
    public ModeHooks currentHooks { get; private set; }
    public CreateMode currentMode { get; private set; } = CreateMode.CreateAircraft;
    private VisualElement popupInstance;
    private bool popupOpen;
    private Vector3 lastHitPoint;
    private bool exitFlag;
    private ExitMode exitMode;
    // altitude-drag controls for popup
    private TextField popupAltField;
    private bool isDraggingAltitude = false;
    private float altDragStartMouseY;
    private float altDragStartValue;
    [SerializeField] private float altitudeDragSensitivity = 0.02f; // world units per pixel
    public ExitMode GetExitMode()
    {
        return exitMode;
    }

    [SerializeField] private float altitudeClearanceForMouseSpawn = 0.1f;

    void AssignData()
    {
        CheckAssignment(uiDocument);

        if (!aircraftFactory) aircraftFactory = GetComponent<AircraftFactory>();
        CheckAssignment(aircraftFactory);

        if (aircraftFactory == null) aircraftFactory = GetComponent<AircraftFactory>();
        CheckAssignment(aircraftFactory);

        if (!mainCamera) mainCamera = Camera.main;
        CheckAssignment(mainCamera);

        if (!previewContainer) previewContainer = new GameObject("Preview").transform;
        CheckAssignment(previewContainer);

        maxDistance = mainCamera ? mainCamera.farClipPlane : 1000f;

        previewMaterialOverride = theme.Preview;
    }
    void CheckAssignment<T>(T obj)
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name})");
    }
    public void Apply()
    {
        // Create waypoint instance at aircraft position
        aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager.Init();
        aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager.waypointFactory.Spawn(aircraftFactory.selectedAircraft.transform.position, aircraftFactory.selectedAircraft.transform.rotation, aircraftFactory.selectedAircraft.time);
        aircraftFactory.selectedAircraft.trajectory.createModeWaypointManager.Apply();
        // Debug.Log("Apply: Create Mode");
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
        TimeGame _time = new TimeGame(t);
        aircraftFactory.Spawn(spawnPos, Quaternion.identity, _time);
        ClosePopup();
        exitFlag = true;
        exitMode = ExitMode.Apply;
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
        // destroy associated line first
        if (previewLine != null)
        {
            Destroy(previewLine);
            previewLine = null;
        }
        Destroy(previewInstance);
        previewInstance = null;
    }
    public void Init()
    {
        AssignData();
        // Debug.Log("Init: Create Aircraft Mode");
    }

    public bool Tick(out ExitMode exitModeOut)
    {
        exitFlag = false;


        // Bu sureci iptal edecek olan sey ESC tusu olsun            
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
            exitFlag = true;
            exitModeOut = ExitMode.Cancel;
            exitMode = exitModeOut;
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
                    lastHitPoint = hoverHit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f);
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
                                    lastHitPoint = hit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f);
                                    OpenPopupAtMouse(lastHitPoint);
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
            // Altitude editing with middle mouse drag while popup is open
            // Start dragging
            if (Input.GetKey(KeyCode.LeftAlt))
            {
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
            }

            // While dragging, update altitude
            if (Input.GetKey(KeyCode.LeftAlt))
            {
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
                        previewLine.GetComponent<LineRenderer>().SetPosition(0, p);

                        previewInstance.transform.position = p;
                    }
                    lastHitPoint.y = newAlt;
                }
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (isDraggingAltitude && Input.GetMouseButtonUp(2))
                {
                    isDraggingAltitude = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SpawnFromPopup();
            }
        }
        exitModeOut = exitMode;
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
        // ...existing code...
        Vector2 mouse = Input.mousePosition;
        // initial placement: cursor'ın sağ-altına küçük offset
        const float offset = 12f;
        float left = mouse.x + offset;
        float top = Screen.height - mouse.y + offset;
        popupInstance.style.left = left;
        popupInstance.style.top = top;

        // after layout, clamp using actual popup size so it doesn't overflow screen
        popupInstance.schedule.Execute(() =>
        {
            float w = popupInstance.layout.width > 0 ? popupInstance.layout.width : 300f;
            float h = popupInstance.layout.height > 0 ? popupInstance.layout.height : 220f;
            float clampedLeft = Mathf.Clamp(mouse.x + offset, 8, Screen.width - w - 8);
            float clampedTop = Mathf.Clamp(Screen.height - mouse.y + offset, 8, Screen.height - h - 8);
            popupInstance.style.left = clampedLeft;
            popupInstance.style.top = clampedTop;
        }).StartingIn(1);
        // ...existing code...  

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
        // keep a persistent reference so other methods can update the alt field live
        popupAltField = altField;
        timeField.value = Time.time.ToString("F2", CultureInfo.InvariantCulture);

        // callbacks
        // createBtn.clicked += SpawnFromPopup;
        // cancelBtn.clicked += ClosePopup;

        // Enter key handling on popup — use TrickleDown so TextField'ler de tuşu geçirirse yakalar
        // popupInstance.RegisterCallback<KeyDownEvent>(OnPopupKeyDown, TrickleDown.TrickleDown);

        root.Add(popupInstance);
        // focus altitude
        altField.Focus();
        popupOpen = true;
    }

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

        // create vertical line under preview using LineRenderer, reuse material if available
        if (previewLine != null)
        {
            Destroy(previewLine);
            previewLine = null;
        }
        previewLine = new GameObject("PreviewLine");
        previewLine.transform.SetParent(previewInstance.transform, false);
        var lr = previewLine.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = previewLineWidth;
        lr.endWidth = previewLineWidth;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.allowOcclusionWhenDynamic = false;
        if (previewMat != null)
        {
            lr.material = previewMat;
        }
        else
        {
            // fallback simple material
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.white;
            lr.material = mat;
        }
        // initial positions (will be updated by UpdatePreviewPositionWithMouse)
        lr.SetPosition(0, previewInstance.transform.position);
        lr.SetPosition(1, previewInstance.transform.position + Vector3.down * Mathf.Min(previewLineMaxLength, 1f));
        lr.startWidth = previewLineWidth;
        lr.endWidth = previewLineWidth;
    }

    private void UpdatePreviewPositionWithMouse()
    {
        if (previewInstance == null || mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
        {
            previewInstance.transform.position = hit.point + new Vector3(0f, altitudeClearanceForMouseSpawn, 0f);
            // update vertical line: cast down from preview to find ground, else use max length
            if (previewLine != null)
            {
                Vector3 top = previewInstance.transform.position;
                Vector3 downOrigin = top + Vector3.up * 0.01f;
                Ray down = new Ray(downOrigin, Vector3.down);
                if (Physics.Raycast(down, out RaycastHit downHit, previewLineMaxLength, hitMask))
                {
                    previewLine.GetComponent<LineRenderer>().SetPosition(0, top);
                    previewLine.GetComponent<LineRenderer>().SetPosition(1, downHit.point);
                }
                else
                {
                    previewLine.GetComponent<LineRenderer>().SetPosition(0, top);
                    previewLine.GetComponent<LineRenderer>().SetPosition(1, top + Vector3.down * previewLineMaxLength);
                }
            }
        }
    }


}

