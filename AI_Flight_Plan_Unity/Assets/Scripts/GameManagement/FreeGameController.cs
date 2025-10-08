using UnityEngine;

public class FreeGameController : MonoBehaviour, IGameController
{
    [SerializeField]
    public UIManager uIManager { get; private set; }
    public Camera cam;                     // atanmazsa Awake'te Camera.main alınır
    public LayerMask draggableMask = ~0;   // raycast bu katmanlarda obje seçer
    public KeyCode cancelKey = KeyCode.Escape;

    Transform _dragT;        // sürüklenen obje
    float _dragY;            // sabitlenecek Y
    Vector3 _offsetXZ;       // mouse'un değdiği nokta ile obje merkezi arasındaki XZ ofset
    bool _dragging;
    public bool Updater()
    {
        bool isInterrupted = false;
        if (Input.GetKey(KeyCode.Escape))
        {
            isInterrupted = true;
        }

        if (Input.GetKey(KeyCode.Return))
        {

        }


        return isInterrupted;
    }
    public void Starter()
    {

    }


    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!cam) return;

        // Başlat: LMB ile objeyi yakala
        if (!_dragging && Input.GetMouseButtonDown(0))
            TryBeginDrag();

        // Sürükleme sırasında konumu güncelle (mouse hep hizasında kalsın)
        if (_dragging && _dragT)
        {
            if (TryRayPlaneHit(_dragY, out var p))
            {
                var target = p + _offsetXZ; // aynı yüzey noktasını mouse altında tut
                _dragT.position = new Vector3(target.x, _dragY, target.z);
            }
        }

        // Bırak veya iptal
        if (_dragging && (Input.GetMouseButtonUp(0) || Input.GetKeyDown(cancelKey)))
            EndDrag();
    }

    void TryBeginDrag()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, draggableMask, QueryTriggerInteraction.Ignore))
        {
            _dragT = hit.collider.transform;
            _dragY = _dragT.position.y;

            // Mouse'un değdiği yüzey noktasıyla objenin merkezinin XZ farkını koru ki
            // sürüklerken aynı nokta mouse altında kalsın.
            var delta = _dragT.position - hit.point;
            delta.y = 0f;
            _offsetXZ = delta;

            // İlk karede de hizala
            if (TryRayPlaneHit(_dragY, out var p))
                _dragT.position = new Vector3((p + _offsetXZ).x, _dragY, (p + _offsetXZ).z);

            _dragging = true;
        }
    }

    void EndDrag()
    {
        _dragging = false;
        _dragT = null;
        _offsetXZ = Vector3.zero;
    }

    // Ekran ışınını Y = sabit düzlemiyle kes: başarıysa p dünya konumu döner
    bool TryRayPlaneHit(float planeY, out Vector3 p)
    {
        p = default;
        var plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f)); // Y = planeY
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            p = ray.GetPoint(enter);
            return true;
        }
        return false;
    }

}
