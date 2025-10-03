using System;
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
    public GameController gameController;

    // UI refs
    public VisualElement root, ctxRoot, aircraftRoot, mainActions, waypointInfoRoot, aircraftInfoRoot, selectProjectedPositionRoot, selectAltitudeandTimeRoot;
    public VisualElement[] visualElementRoots;
    public Button btnAddAircraft, btnAddRestricted, btnCreateAircraft;
    public FloatField fieldX_m, fieldY_m, fieldZ_m, fieldTime_s, fieldVel_m_s;
    public Slider timeSlider;
    private Plane plane;
    DropdownField ddAircraft;
    public bool flag_Create_Waypoint;



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
        waypointInfoRoot = root.Q<VisualElement>("WaypointInfoRoot");
        aircraftInfoRoot = root.Q<VisualElement>("AircraftInfoRoot");
        selectProjectedPositionRoot = root.Q<VisualElement>("SelectProjectedPositionRoot");
        selectAltitudeandTimeRoot = root.Q<VisualElement>("SelectAltitudeAndTimeRoot");
        timeSlider = root.Q<Slider>("TimeSlider");
        timeSlider.lowValue = 0f;
        timeSlider.highValue = 10f;
        timeSlider.RegisterValueChangedCallback(evt =>
        {
            gameController.timeManager.currentTime_s = evt.newValue;
        });

        visualElementRoots = new VisualElement[6];
        visualElementRoots[0] = ctxRoot;
        visualElementRoots[1] = aircraftRoot;
        visualElementRoots[2] = waypointInfoRoot;
        visualElementRoots[3] = aircraftInfoRoot;
        visualElementRoots[4] = selectProjectedPositionRoot;
        visualElementRoots[5] = selectAltitudeandTimeRoot;



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
    private void Update()
    {
        timeSlider.value = gameController.timeManager.currentTime_s;
        if((gameController.selectedAircarft != null)&&(gameController.selectedAircarft.trajectory != null)) 
        { 
            if (gameController.selectedAircarft.trajectory.startTime.second < gameController.timeManager.startTime_s)
            {
                gameController.timeManager.startTime_s = gameController.selectedAircarft.trajectory.startTime.second;
                timeSlider.lowValue = gameController.timeManager.startTime_s;
            }
            if (gameController.timeManager.endTime_s < gameController.selectedAircarft.trajectory.endTime.second)
            {
                gameController.timeManager.endTime_s = gameController.selectedAircarft.trajectory.endTime.second;
                timeSlider.highValue = gameController.timeManager.endTime_s;
            }
        }

    }



    /// <summary>
    /// Free Mode
    /// </summary>

    public void Update_in_Free_Mode()
    {        
        if (Input.GetMouseButtonDown(1))
        {
            lastClickScreen = Input.mousePosition;
            ShowContextAt(lastClickScreen);
            
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAll();
            gameController.mode = Mode.Free_Mode;
        }
    }


    /// <summary>
    /// Select Aircraft Projected Position Phase
    /// </summary>
    public void Start_in_Select_Aircraft_Projected_Position()
    {
        
        VisualElement selectedVisualElement = selectProjectedPositionRoot;
        HideAll();
        ShowVisualElementtAt(selectedVisualElement, Input.mousePosition);
        
        fieldX_m = selectedVisualElement.Q<FloatField>("X_m");
        fieldY_m = selectedVisualElement.Q<FloatField>("Y_m");
        gameController.mode = Mode.Select_Aircraft_Projected_Position;        
    }
    public void Update_in_Select_Aircraft_Projected_Position()
    {
                 
            ShowVisualElementtAt(selectProjectedPositionRoot, Input.mousePosition);

            Vector3 hitPos = gameController.MouseHitPos();
            fieldX_m.value = hitPos.x; 
            fieldY_m.value = hitPos.z; // Especially Change with Y        

            if (Input.GetMouseButtonDown(0))
            {            
                Start_in_Select_Aircraft_Altitude_and_Time();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideAll();
                gameController.mode = Mode.Free_Mode;
            }


    }



    /// <summary>
    /// Select Aircraft Altitude and Time Phase
    /// </summary>
    public void Start_in_Select_Aircraft_Altitude_and_Time()
    {
        VisualElement selectedVisualElement = selectAltitudeandTimeRoot;
        HideAll();
        ShowVisualElementtAt(selectedVisualElement, Input.mousePosition);

        fieldZ_m = selectedVisualElement.Q<FloatField>("Z_m");
        fieldTime_s = selectedVisualElement.Q<FloatField>("Time_s");
        gameController.mode = Mode.Select_Aircraft_Altitude_and_Time;
        fieldZ_m.value = gameController.MouseHitPos().y;        
        fieldZ_m.Focus();
    }

    public void Update_in_Select_Aircraft_Altitude_and_Time()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HideAll();
            gameController.mode = Mode.Create_Aircraft;
            Start_in_Create_Aircraft();
        }
    }



    /// <summary>
    /// Create Aircraft Phase
    /// </summary>
    public void Start_in_Create_Aircraft()
    {
        Vector3 hitPos = new Vector3(fieldX_m.value, fieldZ_m.value, fieldY_m.value);
        gameController.selectedAircarft = gameController.aircraftFactory.Spawn(gameController.selectedAircraftModel, hitPos, Quaternion.Euler(0, 0, 0));
        gameController.selectedWaypoint = gameController.selectedAircarft.CreateWaypoint(gameController.selectedAircarft.transform.position, fieldTime_s.value);
        StartWaypointInfo(gameController.selectedAircarft);        
        Start_in_Select_Waypoint_Projected_Position();
    }
    public void Update_in_Create_Aircraft()
    {

    }



    public void Start_in_Select_Waypoint_Projected_Position()
    {
        VisualElement selectedVisualElement = selectProjectedPositionRoot;
        HideAll();
        ShowVisualElementtAt(selectedVisualElement, Input.mousePosition);

        fieldX_m = selectedVisualElement.Q<FloatField>("X_m");
        fieldY_m = selectedVisualElement.Q<FloatField>("Y_m");
        gameController.mode = Mode.Select_Waypoint_Projected_Position;
    }
    public void Update_in_Select_Waypoint_Projected_Position()
    {
        ShowVisualElementtAt(selectProjectedPositionRoot, Input.mousePosition);

        Vector3 hitPos = gameController.MouseHitPos();
        fieldX_m.value = hitPos.x;
        fieldY_m.value = hitPos.z; // Especially Change with Y        

        if (Input.GetMouseButtonDown(0))
        {
            Start_in_Select_Waypoint_Altitude_and_Time();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAll();
            gameController.mode = Mode.Free_Mode;
        }
    }



    public void Start_in_Select_Waypoint_Altitude_and_Time()
    {
        VisualElement selectedVisualElement = selectAltitudeandTimeRoot;
        HideAll();
        ShowVisualElementtAt(selectedVisualElement, Input.mousePosition);

        fieldZ_m = selectedVisualElement.Q<FloatField>("Z_m");
        fieldTime_s = selectedVisualElement.Q<FloatField>("Time_s");
        gameController.mode = Mode.Select_Waypoint_Altitude_and_Time;
        fieldZ_m.value = gameController.MouseHitPos().y;
        fieldZ_m.Focus();

    }
    public void Update_in_Select_Waypoint_Altitude_and_Time()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HideAll();            
            Create_Waypoint();
            Start_in_Select_Waypoint_Projected_Position();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {            
            Create_Waypoint();
            Start_in_Create_Trajectory();
        }
    }

    public void Create_Waypoint()
    {
        Vector3 hitPos = new Vector3(fieldX_m.value, fieldZ_m.value, fieldY_m.value);
        gameController.selectedWaypoint = gameController.selectedAircarft.CreateWaypoint(hitPos,fieldTime_s.value);
    }
  

    public void Start_in_Create_Trajectory()
    {
        gameController.mode = Mode.Create_Trajectory;
    }
    public void Update_in_Create_Trajectory()
    {
        gameController.selectedAircarft.trajectory.CreateTrajectory();
        gameController.mode = Mode.Free_Mode;
        HideAll();
    }

































    public void StartWaypointInfo(Aircraft aircarft)
    {

        VisualElement selectedVisualElement = waypointInfoRoot;
        Action selectedUpdateInfo = UpdateWaypointInfo; 


        selectedVisualElement.style.display = DisplayStyle.Flex;
        selectedVisualElement.Q<FloatField>("X_m").value = 0;
        selectedVisualElement.Q<FloatField>("Y_m").value = 0;
        selectedVisualElement.Q<FloatField>("Z_m").value = 0;
        selectedVisualElement.Q<FloatField>("Time_s").value = 0;
        selectedVisualElement.Q<FloatField>("Vel_m_s").value = aircarft.spec.nominalVelocity_m_s;
        selectedUpdateInfo();
    }

    public void StartAircraftInfo(Aircraft aircarft)
    {
        VisualElement selectedVisualElement = aircraftInfoRoot;
        Action selectedUpdateInfo = UpdateAircraftInfo;

        selectedVisualElement.style.display = DisplayStyle.Flex;
        selectedVisualElement.Q<FloatField>("X_m").value = 0;
        selectedVisualElement.Q<FloatField>("Y_m").value = 0;
        selectedVisualElement.Q<FloatField>("Z_m").value = 0;
        selectedVisualElement.Q<FloatField>("Time_s").value = 0;
        selectedVisualElement.Q<FloatField>("Vel_m_s").value = aircarft.spec.nominalVelocity_m_s;
        selectedUpdateInfo();
    }

    public void UpdateWaypointInfo()
    {
        VisualElement selectedVisualElement = waypointInfoRoot;

        lastClickScreen = Input.mousePosition;

        if (TryScreenToWorld(lastClickScreen, out Vector3 hitPos))
            lastClickWorld = hitPos;
        selectedVisualElement.Q<FloatField>("X_m").value = Round(hitPos.x, 3);
        selectedVisualElement.Q<FloatField>("Y_m").value = Round(hitPos.y, 3);
        selectedVisualElement.Q<FloatField>("Z_m").value = Round(hitPos.z, 3);
    }

    public void UpdateAircraftInfo()
    {
        VisualElement selectedVisualElement = aircraftInfoRoot;

        lastClickScreen = Input.mousePosition;

        if (TryScreenToWorld(lastClickScreen, out Vector3 hitPos))
            lastClickWorld = hitPos;
        selectedVisualElement.Q<FloatField>("X_m").value = Round(hitPos.x, 3);
        selectedVisualElement.Q<FloatField>("Y_m").value = Round(hitPos.y, 3);
        selectedVisualElement.Q<FloatField>("Z_m").value = Round(hitPos.z, 3);
    }
    private float Round(float val,int dec)
    {
        float roundedVal = Mathf.Round(val * Mathf.Pow(10,dec)) / Mathf.Pow(10, dec);
        return roundedVal;
    }
    void OnAddAircraftClicked()
    {
        HideAll();        
        PopulateAircraftDropdown();
        PositionAircraftPanelContext();
        ShowVisualElementtAt(aircraftRoot, Input.mousePosition);
        
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

        AircraftModel model = aircraftTypes[ddAircraft.index];        
        gameController.selectedAircraftModel = model;
        HideAll();
        Start_in_Select_Aircraft_Projected_Position();                
    }

    void ShowContextAt(Vector2 screenPos)
    {
        // UI Toolkit koordinatlarý sol-üst 0,0 — Screen sol-alt
        Vector2 panelPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

        ctxRoot.style.left = panelPos.x;
        ctxRoot.style.top = panelPos.y;
        ctxRoot.style.display = DisplayStyle.Flex;
                
    }
    void ShowVisualElementtAt(VisualElement visualElement,Vector2 screenPos)
    {
        Vector2 panelPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

        visualElement.style.left = panelPos.x;
        visualElement.style.top = panelPos.y;
        visualElement.style.display = DisplayStyle.Flex;
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
        foreach (VisualElement vis in visualElementRoots)
        {
            vis.style.display = DisplayStyle.None;
        }        
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
