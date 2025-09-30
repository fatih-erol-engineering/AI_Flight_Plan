// =============================
// SceneViewLikeCamera.cs
// Drop this on your runtime Camera to get Unity-Editor-like Scene view navigation.
// Controls (mimic Scene view):
//   Right Mouse (RMB) + Move mouse   -> Look around
//   WASD                              -> Strafe/forward/back
//   Q / E                             -> Down / Up
//   Shift (hold)                      -> Speed boost
//   Mouse Scroll                      -> Dolly (zoom) forward/back
//   Middle Mouse (MMB) drag           -> Pan
//   Alt + Left Mouse drag             -> Orbit around pivot
//   F (hover an object)               -> Focus/Frame hovered object or point (sets new pivot)
//   R                                  -> Reset pivot to current forward point
// Notes:
//   - Works with the old Input Manager (Edit > Project Settings > Input Manager). For the new Input System,
//     swap Input.* calls with your InputActions.
//   - Optional LayerMask to ignore UI/FX layers when focusing.

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SceneViewLikeCamera : MonoBehaviour
{
    [Header("Look / Orbit")]
    public float lookSensitivity = 2.0f;          // degrees per pixel
    public float orbitSensitivity = 0.6f;         // degrees per pixel for Alt+LMB orbit
    public float pitchMin = -89f;
    public float pitchMax = 89f;

    [Header("Movement")]
    public float moveSpeed = 6f;                  // m/s
    public float boostMultiplier = 4f;            // holds with Shift
    public float scrollDollySpeed = 6f;           // m per scroll unit

    [Header("Pan / Focus")]
    public float panSpeed = 0.01f;                // world units per pixel
    public float focusPadding = 1.15f;            // extra distance when framing bounds
    public LayerMask focusMask = ~0;              // which layers are focusable (default: everything)

    [Header("Pivot")]
    public float defaultPivotDistance = 8f;       // used when resetting pivot

    Camera _cam;
    Vector2 _look;                                // yaw (x), pitch (y)
    Vector3 _pivot;                               // orbit pivot
    float _pivotDistance;                         // current orbit distance
    bool _hasPivot = false;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        var euler = transform.rotation.eulerAngles;
        _look = new Vector2(euler.y, euler.x);
        if (!_hasPivot)
        {
            _pivot = transform.position + transform.forward * defaultPivotDistance;
            _pivotDistance = defaultPivotDistance;
            _hasPivot = true;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- Look (RMB) ---
        bool rmb = Input.GetMouseButton(1);
        if (rmb)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _look.x += Input.GetAxisRaw("Mouse X") * lookSensitivity;
            _look.y -= Input.GetAxisRaw("Mouse Y") * lookSensitivity;
            _look.y = Mathf.Clamp(_look.y, pitchMin, pitchMax);
            transform.rotation = Quaternion.Euler(_look.y, _look.x, 0f);
        }
        else if (!Input.GetMouseButton(2) && !(Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0)))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // --- Move (WASD + QE, Shift boost) ---
        Vector3 move = Vector3.zero;
        move += transform.forward * (Input.GetKey(KeyCode.W) ? 1f : 0f);
        move += transform.backward() * (Input.GetKey(KeyCode.S) ? 1f : 0f);
        move += transform.right * (Input.GetKey(KeyCode.D) ? 1f : 0f);
        move += transform.left() * (Input.GetKey(KeyCode.A) ? 1f : 0f);
        move += Vector3.up * (Input.GetKey(KeyCode.E) ? 1f : 0f);
        move += Vector3.down * (Input.GetKey(KeyCode.Q) ? 1f : 0f);
        if (move.sqrMagnitude > 0f)
        {
            float spd = moveSpeed * (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? boostMultiplier : 1f);
            transform.position += move.normalized * spd * dt;
            if (_hasPivot)
            {
                // keep pivot distance consistent when free-flying
                _pivot = transform.position + transform.forward * _pivotDistance;
            }
        }

        // --- Dolly (scroll wheel) ---
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            float step = scroll * scrollDollySpeed * (_pivotDistance > 0.1f ? Mathf.Log10(_pivotDistance + 1f) : 0.5f);
            if (Input.GetKey(KeyCode.LeftAlt) && _hasPivot)
            {
                // dolly towards pivot
                _pivotDistance = Mathf.Max(0.1f, _pivotDistance - step);
                transform.position = _pivot - transform.forward * _pivotDistance;
            }
            else
            {
                transform.position += transform.forward * step;
                if (_hasPivot) _pivot = transform.position + transform.forward * _pivotDistance;
            }
        }

        // --- Pan (MMB) ---
        if (Input.GetMouseButton(2))
        {
            Vector2 mp = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector3 delta = (-transform.right * mp.x + -transform.up * mp.y) * panSpeed * Mathf.Max(_pivotDistance, 1f) * 100f * dt;
            transform.position += delta;
            if (_hasPivot) _pivot += delta;
        }

        // --- Orbit (Alt + LMB) ---
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0) && _hasPivot)
        {
            Vector2 mp = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            _look.x += mp.x * orbitSensitivity;
            _look.y -= mp.y * orbitSensitivity;
            _look.y = Mathf.Clamp(_look.y, pitchMin, pitchMax);
            var rot = Quaternion.Euler(_look.y, _look.x, 0f);
            transform.rotation = rot;
            transform.position = _pivot - transform.forward * _pivotDistance;
        }

        // --- Focus / Frame (F) ---
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (TryRaycast(out var hit))
            {
                Focus(hit.collider.transform, hit.point);
            }
        }

        // --- Reset pivot in front (R) ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            _pivotDistance = defaultPivotDistance;
            _pivot = transform.position + transform.forward * _pivotDistance;
            _hasPivot = true;
        }
    }

    bool TryRaycast(out RaycastHit hit)
    {
        Ray ray = _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
        return Physics.Raycast(ray, out hit, 10000f, focusMask, QueryTriggerInteraction.Ignore);
    }

    void Focus(Transform t, Vector3 fallbackPoint)
    {
        // Try to use Renderer bounds if available, else use collider bounds, else fallbackPoint
        Bounds b;
        var rends = t.GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        }
        else if (t.TryGetComponent<Collider>(out var col))
        {
            b = col.bounds;
        }
        else
        {
            b = new Bounds(fallbackPoint, Vector3.one * 0.5f);
        }

        _pivot = b.center;
        _hasPivot = true;

        // Compute distance to frame bounds based on camera FOV/aspect
        float radius = b.extents.magnitude;
        radius = Mathf.Max(radius, 0.01f);
        float fov = _cam.fieldOfView * Mathf.Deg2Rad;
        float distV = radius / Mathf.Tan(fov * 0.5f);
        float distH = radius / Mathf.Tan(Mathf.Atan(Mathf.Tan(fov * 0.5f) * _cam.aspect));
        _pivotDistance = Mathf.Max(distV, distH) * focusPadding;

        // Move camera keeping orientation
        transform.position = _pivot - transform.forward * _pivotDistance;
    }
}

// --- small Transform direction helpers ---
static class TransformExt
{
    public static Vector3 backward(this Transform t) => -t.forward;
    public static Vector3 left(this Transform t) => -t.right;
}


// =============================
// RuntimeSelectAndTransform.cs (lightweight object controller)
// Minimal runtime selection + transform controls inspired by Unity Editor, without gizmo meshes.
// Controls:
//   Left Click (no Alt/RMB/MMB)   -> Select object under cursor (by collider)
//   1 / 2 / 3                     -> Set mode: Move / Rotate / Scale
//   X / Y / Z                     -> Constrain to axis (press again to clear). No key = free on camera plane (Move) or uniform (Scale)
//   While dragging LMB            -> Apply along chosen axis or camera plane
//   Esc                           -> Clear selection
// Notes:
//   - This is intentionally simple and screen-space based; for full-blown gizmos, consider a dedicated package.

public class RuntimeSelectAndTransform : MonoBehaviour
{
    public LayerMask selectableMask = ~0;
    public float dragSensitivity = 0.01f;   // tune depending on scene scale

    enum Mode { Move, Rotate, Scale }
    Mode _mode = Mode.Move;
    Transform _selected;
    Axis _constraint = Axis.None;
    Camera _cam;
    Vector3 _startPos, _startScale;
    Quaternion _startRot;

    enum Axis { None, X, Y, Z }

    void Awake() { _cam = Camera.main; }

    void Update()
    {
        HandleModeKeys();
        HandleConstraintKeys();

        if (Input.GetKeyDown(KeyCode.Escape)) { _selected = null; }

        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            if (Raycast(out var hit))
            {
                _selected = hit.collider.transform;
                CacheStart();
            }
        }

        if (_selected && Input.GetMouseButton(0))
        {
            Vector2 delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            ApplyDrag(delta);
        }
    }

    void HandleModeKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) _mode = Mode.Move;
        if (Input.GetKeyDown(KeyCode.Alpha2)) _mode = Mode.Rotate;
        if (Input.GetKeyDown(KeyCode.Alpha3)) _mode = Mode.Scale;
    }

    void HandleConstraintKeys()
    {
        if (Input.GetKeyDown(KeyCode.X)) _constraint = _constraint == Axis.X ? Axis.None : Axis.X;
        if (Input.GetKeyDown(KeyCode.Y)) _constraint = _constraint == Axis.Y ? Axis.None : Axis.Y;
        if (Input.GetKeyDown(KeyCode.Z)) _constraint = _constraint == Axis.Z ? Axis.None : Axis.Z;
    }

    void CacheStart()
    {
        _startPos = _selected.position;
        _startRot = _selected.rotation;
        _startScale = _selected.localScale;
    }

    void ApplyDrag(Vector2 mouseDelta)
    {
        switch (_mode)
        {
            case Mode.Move:
                MoveWithMouse(mouseDelta);
                break;
            case Mode.Rotate:
                RotateWithMouse(mouseDelta);
                break;
            case Mode.Scale:
                ScaleWithMouse(mouseDelta);
                break;
        }
    }

    void MoveWithMouse(Vector2 delta)
    {
        Vector3 dir;
        if (_constraint == Axis.None)
        {
            // move in camera plane
            dir = (-_cam.transform.right * delta.x + -_cam.transform.up * delta.y);
        }
        else
        {
            dir = AxisVector(_constraint); // axis in world
            // project mouse delta onto screen-space axis
            Vector3 axisOnScreen = _cam.WorldToScreenPoint(_selected.position + dir) - _cam.WorldToScreenPoint(_selected.position);
            Vector2 axis2D = new Vector2(axisOnScreen.x, axisOnScreen.y).normalized;
            float amount = Vector2.Dot(axis2D, delta) * dragSensitivity * DistanceScale();
            _selected.position = _startPos + dir.normalized * amount;
            return;
        }

        _selected.position = _startPos + dir * dragSensitivity * DistanceScale();
    }

    void RotateWithMouse(Vector2 delta)
    {
        float amount = (delta.x + delta.y) * 5f; // degrees per pixel
        if (_constraint == Axis.None)
        {
            // rotate around camera forward
            _selected.rotation = Quaternion.AngleAxis(amount, _cam.transform.forward) * _startRot;
        }
        else
        {
            _selected.rotation = Quaternion.AngleAxis(amount, AxisVector(_constraint)) * _startRot;
        }
    }

    void ScaleWithMouse(Vector2 delta)
    {
        float amount = (delta.x + delta.y) * 0.01f;
        if (_constraint == Axis.None)
        {
            _selected.localScale = Vector3.Max(Vector3.one * 0.001f, _startScale * (1f + amount));
        }
        else
        {
            Vector3 s = _startScale;
            Vector3 axis = Vector3.zero;
            if (_constraint == Axis.X) axis = new Vector3(1, 0, 0);
            if (_constraint == Axis.Y) axis = new Vector3(0, 1, 0);
            if (_constraint == Axis.Z) axis = new Vector3(0, 0, 1);
            s += axis * amount * s.magnitude;
            _selected.localScale = new Vector3(Mathf.Max(0.001f, s.x), Mathf.Max(0.001f, s.y), Mathf.Max(0.001f, s.z));
        }
    }

    float DistanceScale()
    {
        // scale movement by distance to camera so it feels consistent
        float d = Vector3.Distance(_cam.transform.position, _selected.position);
        return Mathf.Max(0.2f, d * 0.1f);
    }

    Vector3 AxisVector(Axis a)
    {
        switch (a)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
            default: return Vector3.zero;
        }
    }

    bool Raycast(out RaycastHit hit)
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, 10000f, selectableMask, QueryTriggerInteraction.Ignore);
    }
}

/*
Setup:
1) Add SceneViewLikeCamera to your runtime Camera.
2) Add RuntimeSelectAndTransform to any always-active object (e.g., an empty GameObject named "RuntimeControls").
3) Ensure your selectable objects have Colliders (any type). Optionally adjust LayerMasks.
4) Press Play and use the key/mouse scheme listed above. Enjoy Scene view-like navigation and basic runtime editing.

Tips:
- For New Input System, wire actions: Look (Vector2), Move (Vector3/WASD), Up/Down, Pan (MMB), Orbit (Alt+LMB), Focus (F), and map analogically.
- To snap like the Editor, you can add modifiers (e.g., Ctrl for 1-unit move, 5° rotate). This skeleton keeps it lean.
*/
