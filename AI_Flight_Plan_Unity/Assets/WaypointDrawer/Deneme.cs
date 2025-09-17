using System.Collections.Generic;
using UnityEngine;

public class Deneme : MonoBehaviour
{
    [Header("References")]
    public Camera topDownCamera;         // Tepeden bakan kamera (boşsa Camera.main)
    public GameObject waypointPrefab;    // İsteğe bağlı prefab (küçük sphere + collider)

    [Header("Raycast")]
    public LayerMask groundMask;
    public float rayMaxDistance = 5000f;

    [Header("Selection")]
    public float selectMaxDistance = 2f;

    // Panel (taşınabilir + yeniden boyutlandırılabilir)
    private Rect _panelRect = new Rect(12, 12, 320, 280);
    private const float RESIZE_HANDLE = 16f;
    private bool _resizing = false;

    private readonly List<Transform> _waypoints = new List<Transform>();
    private Transform _selected;
    private string _x = "0", _y = "0", _z = "0";

    // Renk UI durumları
    private Color _pickedColor = Color.cyan;
    private string _hex = "#00FFFF";
    private Vector2 _scroll;
    private bool _dragMode = false;

    // Çizgi için
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Material _lineMat; // yoksa default materyal yaratacağız
    [SerializeField] private float _lineWidth = 0.05f;
    [SerializeField] private bool _closeLoop = false; // sonu başa bağlamak istersen


    // Panel durumları
    
    [SerializeField] private bool _isCollapsed = false;
    [SerializeField] private bool _isMaximized = false;

    private Vector2 _savedPanelSize;   // maximize öncesi boyutu saklamak için
    private const float TITLE_BAR_H = 20f;
    private const float MIN_W = 260f;
    private const float MIN_H = 200f;
    


    void Awake()
    {
        if (topDownCamera == null) topDownCamera = Camera.main;
        if (topDownCamera == null)
            Debug.LogWarning("[WaypointPlacer] Bir kamera atayın (topDownCamera).");

        // … (mevcut kodun kalacak)
        if (_lr == null)
        {
            var go = new GameObject("WaypointLine");
            _lr = go.AddComponent<LineRenderer>();
            _lr.useWorldSpace = true;
            _lr.alignment = LineAlignment.View;
            _lr.textureMode = LineTextureMode.Stretch;   // Düz çizgi
            _lr.numCapVertices = 4;                      // Uçları yuvarla
            _lr.numCornerVertices = 2;                   // Köşeleri yumuşat
        }
        if (_lineMat == null)
        {
            // Basit, görünür bir materyal
            _lineMat = new Material(Shader.Find("Sprites/Default"));
        }
        _lr.material = _lineMat;
        _lr.startWidth = _lineWidth;
        _lr.endWidth = _lineWidth;
        _lr.positionCount = 0;
    }
    private void UpdatePolyline()
    {
        if (_lr == null) return;

        int n = _waypoints.Count;
        if (n < 2)
        {
            _lr.positionCount = 0; // 0 veya 1 WP varken çizgi göstermeyelim
            return;
        }

        // Kapalı/ açık poligon
        _lr.loop = _closeLoop;

        if (_closeLoop)
        {
            _lr.positionCount = n;
            for (int i = 0; i < n; i++)
                _lr.SetPosition(i, _waypoints[i].position);
        }
        else
        {
            _lr.positionCount = n;
            for (int i = 0; i < n; i++)
                _lr.SetPosition(i, _waypoints[i].position);
        }
        _lr.startColor = _pickedColor;
        _lr.endColor = _pickedColor;

    }


    void Update()
    {
        if (topDownCamera == null) return;

        // Sol tık
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);

            if (_dragMode && _selected != null)
            {
                if (Physics.Raycast(ray, out RaycastHit hitDrag, rayMaxDistance, groundMask))
                {
                    UndoLikeRecord("Move Waypoint");
                    _selected.position = hitDrag.point;
                    SyncFieldsFromSelected();                    
                    UpdatePolyline(); // <<< ekle
                }
                return;
            }

            // Ctrl + Sol tık => seç
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                TrySelectWaypointUnderMouse(ray);
            }
            else
            {
                // Yeni waypoint
                if (Physics.Raycast(ray, out RaycastHit hit, rayMaxDistance, groundMask))
                {
                    CreateWaypointAt(hit.point);
                }
            }
        }

        // Delete => sil
        if (_selected != null && Input.GetKeyDown(KeyCode.Delete))
        {
            RemoveSelected();
        }
    }

    private void CreateWaypointAt(Vector3 pos)
    {
        GameObject go;
        if (waypointPrefab != null)
        {
            go = Instantiate(waypointPrefab, pos, Quaternion.identity);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.5f;
        }

        go.name = $"Waypoint_{_waypoints.Count}";

        // Renk için materyal örneği, marker component
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = _pickedColor;
            renderer.material.EnableKeyword("_EMISSION");
        }

        var wp = go.GetComponent<WaypointMarker>();
        if (wp == null) wp = go.AddComponent<WaypointMarker>();
        wp.index = _waypoints.Count;

        _waypoints.Add(go.transform);
        Select(go.transform);
        UpdatePolyline();
    }

    private void TrySelectWaypointUnderMouse(Ray ray)
    {
        // Önce collider’a tıklamayı dene
        if (Physics.Raycast(ray, out RaycastHit hitAll, rayMaxDistance))
        {
            var t = hitAll.transform;
            if (_waypoints.Contains(t))
            {
                Select(t);
                return;
            }
        }
        // Olmazsa zemine projekte edip en yakın waypoint
        if (Physics.Raycast(ray, out RaycastHit hit, rayMaxDistance, groundMask))
        {
            Transform nearest = null;
            float best = float.MaxValue;
            Vector3 p = hit.point;

            foreach (var w in _waypoints)
            {
                float d = Vector3.Distance(w.position, p);
                if (d < best) { best = d; nearest = w; }
            }
            if (nearest != null && best <= selectMaxDistance)
                Select(nearest);
        }
    }

    private void Select(Transform t)
    {
        _selected = t;
        SyncFieldsFromSelected();
        // Renk alanlarını doldur
        var r = _selected.GetComponent<Renderer>();
        _pickedColor = r != null ? r.material.color : Color.white;
        _hex = ColorToHex(_pickedColor);
    }

    private void RemoveSelected()
    {
        if (_selected == null) return;
        var t = _selected;
        _selected = null;
        _waypoints.Remove(t);
        Destroy(t.gameObject);
        UpdatePolyline();
    }

    private void SyncFieldsFromSelected()
    {
        if (_selected == null) return;
        Vector3 p = _selected.position;
        _x = p.x.ToString("F3");
        _y = p.y.ToString("F3");
        _z = p.z.ToString("F3");
    }

    private void ApplyFieldsToSelected()
    {
        if (_selected == null) return;

        if (float.TryParse(_x, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float px) &&
            float.TryParse(_y, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float py) &&
            float.TryParse(_z, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pz))
        {
            UndoLikeRecord("Edit Waypoint XYZ");
            _selected.position = new Vector3(px, py, pz);
        }
        UpdatePolyline();
    }

    private void ApplyColorToSelected()
    {
        if (_selected == null) return;
        var r = _selected.GetComponent<Renderer>();
        if (r == null) return;
        UndoLikeRecord("Edit Waypoint Color");
        r.material.color = _pickedColor;
        r.material.EnableKeyword("_EMISSION");
    }

    // IMGUI --------------------------------------------------------------------


    void OnGUI()
    {
        // Pencereyi çiz
        _panelRect = GUI.Window(3210, _panelRect, DrawPanel, "Waypoint Panel");

        var rect = _panelRect;
        if (_isCollapsed)
            rect.height = TITLE_BAR_H + 6f; // küçük bir pay

        _panelRect = GUI.Window(3210, rect, DrawPanel, "Waypoint Panel");

        // Sürüklemeden sonra konum güncellendi; maximize/resize mantığı aşağıda
        if (!_isCollapsed && !_isMaximized)
            HandleResize(_panelRect); // sadece normal modda resize
    }

    private void DrawPanel(int id)
    {
        // Taşıma (üst bar)
        var titleDragRect = new Rect(0, 0, _panelRect.width - RESIZE_HANDLE, 20);
        GUI.DragWindow(titleDragRect);

        GUILayout.Space(6);
        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Width(_panelRect.width - 10), GUILayout.Height(_panelRect.height - 40));

        if (_selected == null)
        {
            GUILayout.Label("Seçili waypoint yok.");
            GUILayout.Space(8);
            GUILayout.Label("İpuçları:");
            GUILayout.Label("- Sol tık: Yeni waypoint");
            GUILayout.Label("- Ctrl + Sol tık: Seç");
            GUILayout.Label("- Delete: Sil");
            GUILayout.EndScrollView();
            return;
        }

        GUILayout.Label($"Seçili: {_selected.name}");
        GUILayout.Space(6);

        // XYZ alanları
        GUILayout.Label("Pozisyon (XYZ):");
        GUILayout.BeginHorizontal();
        GUILayout.Label("X", GUILayout.Width(16));
        _x = GUILayout.TextField(_x, GUILayout.MinWidth(60));
        GUILayout.Label("Y", GUILayout.Width(16));
        _y = GUILayout.TextField(_y, GUILayout.MinWidth(60));
        GUILayout.Label("Z", GUILayout.Width(16));
        _z = GUILayout.TextField(_z, GUILayout.MinWidth(60));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Uygula (XYZ -> Transform)"))
            ApplyFieldsToSelected();

        GUILayout.Space(10);
        _dragMode = GUILayout.Toggle(_dragMode, "Drag modu (tıkla -> taşı)");

        GUILayout.Space(12);
        DrawColorSection();

        GUILayout.Space(10);
        if (GUILayout.Button("Seçileni Sil (Delete)"))
            RemoveSelected();

        GUILayout.Space(6);
        GUILayout.Label("Kısayollar: Sol tık=Ekle, Ctrl+Sol tık=Seç, Delete=Sil");

        GUILayout.EndScrollView();



        // Üst bar (drag + butonlar)
        var titleArea = new Rect(0, 0, _panelRect.width, TITLE_BAR_H);

        GUILayout.BeginArea(titleArea);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Waypoint Panel", GUILayout.ExpandWidth(true));

        // Collapse / Expand
        if (GUILayout.Button(_isCollapsed ? "▢" : "—", GUILayout.Width(24)))
        {
            _isCollapsed = !_isCollapsed;
            GUI.FocusControl(null);
        }

        // Maximize / Restore
        if (GUILayout.Button(_isMaximized ? "❐" : "□", GUILayout.Width(24)))
        {
            _isMaximized = !_isMaximized;

            if (_isMaximized)
            {
                // maximize olurken mevcut boyutu sakla
                _savedPanelSize = new Vector2(_panelRect.width, _panelRect.height);
                // Ekranın çoğunu kapla (kenarlarda margin bırak)
                float margin = 12f;
                _panelRect.x = margin;
                _panelRect.y = margin;
                _panelRect.width = Mathf.Max(MIN_W, Screen.width - 2 * margin);
                _panelRect.height = Mathf.Max(TITLE_BAR_H + 40f, Screen.height - 2 * margin);
            }
            else
            {
                // restore: eski boyutu geri yükle (konumu koruyarak)
                _panelRect.width = Mathf.Max(MIN_W, _savedPanelSize.x);
                _panelRect.height = Mathf.Max(TITLE_BAR_H + 40f, _savedPanelSize.y);
            }
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Başlığın geri kalanını drag yapılabilir bırak
        // (butonların altındaki kısmı değil; soldaki alanı kullanalım)
        var dragRect = new Rect(0, 0, _panelRect.width - 60f, TITLE_BAR_H);
        GUI.DragWindow(dragRect);

        // Eğer collapse ise, içerik çizmeden çık
        if (_isCollapsed)
            return;

        // ... (mevcut scroll/XYZ/renk vs. içerik burada kalacak)
        // İçerik alanı başlangıcını, başlıktan sonra başlat:
        GUILayout.BeginArea(new Rect(6, TITLE_BAR_H + 6f, _panelRect.width - 12f, _panelRect.height - TITLE_BAR_H - 12f));
        //  -> burada mevcut _scroll = GUILayout.BeginScrollView(...); vs devam etsin
        //  -> panel içeriğinin geri kalanına dokunma
        //  -> sonunda EndScrollView/EndArea ile kapat
        // ...
        GUILayout.EndArea();
    }

    private void DrawColorSection()
    {
        GUILayout.Label("Renk (RGB + HEX):");

        // Önizleme kutusu
        var previewRect = GUILayoutUtility.GetRect(40, 30);
        EditorLikeFillRect(previewRect, _pickedColor);
        GUI.Box(previewRect, GUIContent.none);

        // RGB slider’lar
        float r = _pickedColor.r;
        float g = _pickedColor.g;
        float b = _pickedColor.b;

        GUILayout.BeginHorizontal(); GUILayout.Label("R", GUILayout.Width(14));
        r = GUILayout.HorizontalSlider(r, 0f, 1f); GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(); GUILayout.Label("G", GUILayout.Width(14));
        g = GUILayout.HorizontalSlider(g, 0f, 1f); GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(); GUILayout.Label("B", GUILayout.Width(14));
        b = GUILayout.HorizontalSlider(b, 0f, 1f); GUILayout.EndHorizontal();

        var newColor = new Color(r, g, b, 1f);
        if (newColor != _pickedColor)
        {
            _pickedColor = newColor;
            _hex = ColorToHex(_pickedColor);
            ApplyColorToSelected(); // anında uygula
        }

        // HEX giriş
        GUILayout.BeginHorizontal();
        GUILayout.Label("HEX", GUILayout.Width(32));
        _hex = GUILayout.TextField(_hex, GUILayout.MinWidth(90));
        if (GUILayout.Button("Uygula", GUILayout.Width(64)))
        {
            if (TryParseHex(_hex, out var hexColor))
            {
                _pickedColor = hexColor;
                ApplyColorToSelected();
                _hex = ColorToHex(_pickedColor);
            }
        }
        GUILayout.EndHorizontal();
    }

    // Panel resize mantığı -----------------------------------------------------

    private void HandleResize(Rect windowRect)
    {
        var e = Event.current;
        var handleRect = new Rect(windowRect.xMax - RESIZE_HANDLE, windowRect.yMax - RESIZE_HANDLE, RESIZE_HANDLE, RESIZE_HANDLE);

        // Görsel tutamak
        EditorLikeFillRect(handleRect, new Color(0, 0, 0, 0.2f));

        // Input
        if (e.type == EventType.MouseDown && handleRect.Contains(e.mousePosition))
        {
            _resizing = true;
            e.Use();
        }
        if (_resizing && e.type == EventType.MouseDrag)
        {
            float newW = Mathf.Max(260, e.mousePosition.x - windowRect.x + RESIZE_HANDLE * 0.5f);
            float newH = Mathf.Max(200, e.mousePosition.y - windowRect.y + RESIZE_HANDLE * 0.5f);
            _panelRect.width = newW;
            _panelRect.height = newH;
            e.Use();
        }
        if (e.type == EventType.MouseUp) _resizing = false;
        if (_resizing && e.type == EventType.MouseDrag)
        {
            float newW = Mathf.Max(MIN_W, e.mousePosition.x - windowRect.x + RESIZE_HANDLE * 0.5f);
            float newH = Mathf.Max(MIN_H, e.mousePosition.y - windowRect.y + RESIZE_HANDLE * 0.5f);
            _panelRect.width = newW;
            _panelRect.height = newH;
            e.Use();
        }

    }

    // Basit doldurma (IMGUI içinde renkli dikdörtgen)
    private void EditorLikeFillRect(Rect r, Color c)
    {
        var prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = prev;
    }

    // Yardımcılar --------------------------------------------------------------

    private string ColorToHex(Color c)
    {
        Color32 c32 = c;
        return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}";
    }

    private bool TryParseHex(string s, out Color col)
    {
        col = Color.white;
        if (string.IsNullOrEmpty(s)) return false;
        s = s.Trim();

        if (s[0] == '#') s = s.Substring(1);
        if (s.Length == 3) // #RGB -> #RRGGBB
            s = "" + s[0] + s[0] + s[1] + s[1] + s[2] + s[2];

        if (s.Length != 6) return false;

        bool ok = byte.TryParse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte r) &&
                  byte.TryParse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte g) &&
                  byte.TryParse(s.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b);

        if (!ok) return false;
        col = new Color32(0, 0, 0, 255);
        return true;
    }

    private void UndoLikeRecord(string action)
    {
#if UNITY_EDITOR
        if (_selected != null)
            UnityEditor.Undo.RecordObject(_selected, action);
#endif
    }
}

// Küçük yardımcı component (gizmos + etiket)
public class WaypointMarker : MonoBehaviour
{
    public int index = -1;

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.4f);
#if UNITY_EDITOR
        var style = new GUIStyle(UnityEditor.EditorStyles.boldLabel);
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, $"WP {index}", style);
#endif
    }
}
