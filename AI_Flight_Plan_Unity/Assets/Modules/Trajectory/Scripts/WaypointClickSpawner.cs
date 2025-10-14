using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WaypointClickSpawnerUIToolkit : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private WaypointFactory waypointFactory;
    [SerializeField] private VisualTreeAsset popupUxml; // assign WaypointPopup.uxml in inspector
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private float maxDistance = 500f;
    [SerializeField] private KeyCode spawnKey = KeyCode.Mouse0;

    private UIDocument uiDocument;
    private VisualElement popupInstance;
    private bool popupOpen;
    private Vector3 lastHitPoint;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (cam == null) cam = Camera.main;
        if (waypointFactory == null) waypointFactory = GetComponent<WaypointFactory>();
    }

    void Update()
    {
        // if popup open -> second click in scene spawns (ignore clicks over UI)
        if (popupOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePopup();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                SpawnFromPopup();
            }

            return;
        }

        // Normal first click -> open popup
        if (!Input.GetKeyDown(spawnKey)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (cam == null)
        {
            Debug.LogError($"[{GetType().Name}] Camera is not assigned.");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask))
        {
            lastHitPoint = hit.point;
            OpenPopupAtMouse(hit.point);
        }
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
        waypointFactory.Spawn(spawnPos, Quaternion.identity, t);

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
        popupOpen = false;
    }
}