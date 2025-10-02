using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapPopupSpawner : MonoBehaviour
{
    [Header("UI")]
    public UIDocument uiDocument;

    [Header("Spawn")]
    public Camera cam;
    public LayerMask hitMask = ~0;           // Haritada týklanacak katmanlar
    public float groundY = 0f;               // Kolider yoksa düzlem
    public bool useGroundPlaneIfNoHit = true;

    [Header("Factories & Data")]
    public AircraftSpecRegistry registry;    // Registry (ScriptableObject)
    public AircraftFactory factory;          // Daha önce verdiðimiz factory

    // UI refs
    VisualElement root, ctxRoot, aircraftRoot, mainActions;
    Button btnAddAircraft, btnAddRestricted, btnCreateAircraft;
    DropdownField ddAircraft;

    // Dahili durum
    Vector2 lastClickScreen;
    Vector3 lastClickWorld;
    List<string> aircraftLabels = new();
    List<AircraftModel> aircraftTypes = new();

    void OnEnable()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        if (!cam) cam = Camera.main;

        root = uiDocument.rootVisualElement;
        ctxRoot = root.Q<VisualElement>("ContextRoot");
        aircraftRoot = root.Q<VisualElement>("AircraftRoot");
        mainActions = root.Q<VisualElement>("MainActions");
        btnAddAircraft = root.Q<Button>("BtnAddAircraft");
        btnAddRestricted = root.Q<Button>("BtnAddRestricted");
        btnCreateAircraft = root.Q<Button>("BtnCreateAircraft");
        ddAircraft = root.Q<DropdownField>("DdAircraft");

        // Baðlantýlar
        btnAddAircraft.clicked += OnAddAircraftClicked;
        btnAddRestricted.clicked += OnAddRestrictedClicked;
        btnCreateAircraft.clicked += OnCreateAircraftClicked;

        // Dýþarý týklayýnca menüleri kapat
        root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    void Update()
    {
       
        if (Input.GetMouseButtonDown(1))
        {
            lastClickScreen = Input.mousePosition;

            if (TryScreenToWorld(lastClickScreen, out var hitPos))
                lastClickWorld = hitPos;

            ShowContextAt(lastClickScreen);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAll();
        }
    }

    void OnAddAircraftClicked()
    {
        ShowContextPanel(false);
        PopulateAircraftDropdown();
        PositionAircraftPanelContext();
        ShowAircraftPanel(true);
    }

    void OnAddRestrictedClicked()
    {
        Debug.Log("Add Restricted Area seçildi. Burada kýsýtlý bölge oluþturma sürecini baþlat.");
        HideAll();
    }

    void OnCreateAircraftClicked()
    {
        if (ddAircraft.index < 0 || ddAircraft.index >= aircraftTypes.Count)
        {
            Debug.LogWarning("Geçerli bir aircraft seçilmedi.");
            return;
        }

        var type = aircraftTypes[ddAircraft.index];

        // Spawn noktasý: týklanan dünya noktasý (lastClickWorld)
        var ctrl = factory.Spawn(type, lastClickWorld + Vector3.up * 2f, Quaternion.identity);
        if (ctrl) Debug.Log($"Spawned aircraft: {ctrl.spec?.name ?? type.ToString()}");

        HideAll();
    }

    // --- UI Yardýmcýlarý ---

    void ShowContextAt(Vector2 screenPos)
    {
        // UI Toolkit koordinatlarý sol-üst 0,0 — Screen sol-alt
        Vector2 panelPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

        ctxRoot.style.left = panelPos.x;
        ctxRoot.style.top = panelPos.y;
        ctxRoot.style.display = DisplayStyle.Flex;

        // Yan paneli resetle & gizle
        ShowAircraftPanel(false);
    }

    void PositionAircraftPanelBesideContext()
    {
        // Aircraft panelini context panelinin saðýna, ayný top’la koy
        var ctxBounds = ctxRoot.worldBound;
        aircraftRoot.style.left = ctxBounds.xMax + 8f;  // 8px boþluk
        aircraftRoot.style.top = ctxBounds.y;
    }

    void PositionAircraftPanelContext()
    {
        // Aircraft panelini context panelinin saðýna, ayný top’la koy
        var ctxBounds = ctxRoot.worldBound;
        aircraftRoot.style.left = ctxBounds.x;  // 8px boþluk
        aircraftRoot.style.top = ctxBounds.y;
    }

    void ShowAircraftPanel(bool on)
    {
        aircraftRoot.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
    }
    void ShowContextPanel(bool on)
    {
        ctxRoot.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void HideAll()
    {
        ctxRoot.style.display = DisplayStyle.None;
        aircraftRoot.style.display = DisplayStyle.None;
    }

    void OnRootPointerDown(PointerDownEvent evt)
    {
        // Menülerin dýþýna týklanýnca kapat
        bool ctxVisible = ctxRoot.resolvedStyle.display != DisplayStyle.None;
        bool acVisible = aircraftRoot.resolvedStyle.display != DisplayStyle.None;

        if (!ctxVisible && !acVisible) return;

        bool insideAny = false;
        if (ctxVisible && ctxRoot.worldBound.Contains(evt.position)) insideAny = true;
        if (acVisible && aircraftRoot.worldBound.Contains(evt.position)) insideAny = true;

        if (!insideAny) HideAll();
    }

    // --- Veri ---

    void PopulateAircraftDropdown()
    {
        aircraftLabels.Clear();
        aircraftTypes.Clear();

        if (registry == null || registry.specs == null || registry.specs.Count == 0)
        {
            aircraftLabels.Add("No aircraft in registry");
            ddAircraft.choices = aircraftLabels;
            ddAircraft.index = 0;
            return;
        }

        foreach (var s in registry.specs)
        {
            if (!s) continue;
            aircraftLabels.Add(string.IsNullOrWhiteSpace(s.name) ? s.model.ToString() : s.name);
            aircraftTypes.Add(s.model);
        }

        ddAircraft.choices = aircraftLabels;
        ddAircraft.index = Mathf.Clamp(ddAircraft.index, 0, aircraftLabels.Count - 1);
    }

    // --- Ray yardýmcýlarý ---

    bool TryScreenToWorld(Vector2 screen, out Vector3 world)
    {
        var ray = cam.ScreenPointToRay(screen);
        if (Physics.Raycast(ray, out var hit, 100000f, hitMask, QueryTriggerInteraction.Collide))
        {
            world = hit.point;
            return true;
        }
        else if (useGroundPlaneIfNoHit)
        {
            var plane = new Plane(Vector3.up, new Vector3(0, groundY, 0));
            if (plane.Raycast(ray, out float enter))
            {
                world = ray.GetPoint(enter);
                return true;
            }
        }
        world = default;
        return false;
    }
}
