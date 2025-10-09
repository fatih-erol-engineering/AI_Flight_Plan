using System.Collections;   // IEnumerator burada
using System.Collections.Generic; // (List<T> vs. kullanacaksan)
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeModeController : MonoBehaviour, IGameController
{
    [SerializeField]
    public UIManager uIManager { get; private set; }
    public Camera cam;


    [Header("Drag Settings")]
    public LayerMask draggableMask = ~0;  
    public KeyCode cancelKey = KeyCode.Escape;

    Transform _dragT;      
    float _dragY;          
    Vector3 _offsetXZ;     
    bool _dragging;

    [Header("Focus Settings")]
    public Transform selected;              // Dışarıdan set edeceğin seçili obje
    public float padding = 1.2f;            // Kadraj payı
    public float moveDuration = 0.35f;      // Yumuşak geçiş süresi
    public float minDistance = 0.3f;        // Hedefe çok yaklaşmayı önle
    public LayerMask obstacleMask = ~0;     // İstersen çevre katmanlarını burada sınırla
    public bool rotateToLookAt = true;      // Kadrajda hedefe dönsün mü

    [Header("Select Settings")]    
    public LayerMask pickMask = ~0;   // seçilebilir katmanlar
    public float maxDistance = 1000f;
    public bool Updater()
    {
        bool isInterrupted = false;
        if (Input.GetKey(KeyCode.Escape))
        {
            isInterrupted = true;
        }
        DragControl(); // Mouse panning but It moves world aronud Camera
        FocusControl(); // F Button will focus an object
        SelectControl();
        return isInterrupted;
    }
    public void Starter()
    {
        if (!cam) cam = Camera.main;
    }

    void SelectControl()
    {
        if (Input.GetMouseButton(0))
        {
            
            // UI üzerindeyse tıklamayı alma (opsiyonel)
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickMask, QueryTriggerInteraction.Ignore))
            {
                // Rigidbody varsa onu, yoksa collider'ın objesini al
                GameObject go = hit.rigidbody ? hit.rigidbody.gameObject : hit.collider.gameObject;

                selected = go.transform;
            }
        }
    }


    void FocusControl()
    {
        
        if (Input.GetKeyDown(KeyCode.F) && selected != null)
            StartCoroutine(FrameSelected(selected));
    }

    public void SetSelected(Transform t) => selected = t;

    public IEnumerator FrameSelected(Transform target)
    {
        if (!target) yield break;

        // 1) Hedef bounds
        Bounds b = CalculateBounds(target, out bool hasRenderers);
        Vector3 center = hasRenderers ? b.center : target.position;

        // Renderers yoksa küçük bir varsayılan hacim kabul edelim
        Vector3 extents = hasRenderers ? b.extents : Vector3.one * 0.5f;

        // 2) Gerekli mesafeyi hesapla
        float desiredDistance = ComputeDistanceForFraming(extents, cam) * padding;
        desiredDistance = Mathf.Max(desiredDistance, minDistance);

        // 3) İstenilen pozisyon (mevcut ileri eksen boyunca geri çekil)
        Vector3 desiredPos = center - cam.transform.forward * desiredDistance;

        // 4) Çarpışma/engelleme kontrolü (hedeften kameraya doğru)
        if (Physics.Linecast(center, desiredPos, out RaycastHit hit, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // Engelin önünde dur
            desiredPos = hit.point + hit.normal * 0.2f;
        }

        // 5) Ortho ise boyutu ayarla (mesafe yerine size)
        float originalOrthoSize = cam.orthographicSize;
        float targetOrthoSize = cam.orthographicSize;
        if (cam.orthographic)
        {
            float sizeV = extents.y;
            float sizeH = extents.x / cam.aspect;
            targetOrthoSize = Mathf.Max(sizeV, sizeH) * padding;
        }

        // 6) Yumuşak geçiş
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        Quaternion endRot = rotateToLookAt ? Quaternion.LookRotation(center - desiredPos, Vector3.up) : startRot;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float s = Smooth01(t);
            cam.transform.position = Vector3.Lerp(startPos, desiredPos, s);
            cam.transform.rotation = Quaternion.Slerp(startRot, endRot, s);
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Lerp(originalOrthoSize, targetOrthoSize, s);
            yield return null;
        }

        cam.transform.position = desiredPos;
        cam.transform.rotation = endRot;
        if (cam.orthographic) cam.orthographicSize = targetOrthoSize;
    }

    static float Smooth01(float x) => x * x * (3f - 2f * x); // smoothstep

    static Bounds CalculateBounds(Transform root, out bool hasRenderers)
    {
        var rends = root.GetComponentsInChildren<Renderer>();
        hasRenderers = (rends.Length > 0);
        if (!hasRenderers)
        {
            // Renderer yoksa tek noktalı bounds
            return new Bounds(root.position, Vector3.zero);
        }

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);
        return b;
    }

    static float ComputeDistanceForFraming(Vector3 extents, Camera cam)
    {
        if (cam.orthographic) return 1f; // Ortho’da mesafe önemsiz; size ayarlanacak

        // Dikey FOV ve en-boy oranıyla hem yatay hem dikey için mesafeleri hesapla
        float fovRad = cam.fieldOfView * Mathf.Deg2Rad;
        float halfFovTan = Mathf.Tan(fovRad * 0.5f);

        float distV = extents.y / halfFovTan;                    // dikey
        float distH = extents.x / (halfFovTan * cam.aspect);     // yatay

        // Derinlik (Z extents) de ek güvenlik payı için düşünülür:
        float dist = Mathf.Max(distV, distH) + extents.z;

        return dist;
    }

   
    /// <summary>
    /// DRAG CONTROL
    /// </summary>
    public void DragControl()
    {
        if (!cam) return;
        
        if (!_dragging && Input.GetMouseButtonDown(0))
            TryBeginDrag();
        
        if (_dragging && _dragT)
        {
            if (TryRayPlaneHit(_dragY, out var p))
            {
                var target = p + _offsetXZ; 
                _dragT.position = new Vector3(target.x, _dragY, target.z);
            }
        }
        
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
