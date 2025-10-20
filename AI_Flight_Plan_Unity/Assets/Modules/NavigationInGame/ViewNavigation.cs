using UnityEngine;

/// <summary>
/// Editor-like scene navigation at runtime:
/// - RMB = free look (yaw/pitch)
/// - RMB + WASD = fly; Q/E = down/up; Shift = boost; Ctrl = slow
/// - MMB = pan
/// - Alt + LMB = orbit around pivot
/// - Scroll = dolly; while RMB held, scroll tunes fly speed
/// - F = focus: selectedObject -> editor selection (in editor) -> mouse raycast -> ahead
/// Smoothing runs ONLY during focus transitions. Any manual input cancels focus-smoothing.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ViewNavigation : MonoBehaviour
{
    // --- XZ-only pan with fixed plane & anchor ---
    private bool _isPanningXZ = false;
    private Vector3 _panAnchorWorld; // where you first clicked in world
    private float _panPlaneY;        // fixed horizontal plane height for this drag
    // --- XZ-only pan state ---
    [Header("Look / Orbit")]
    public float lookSensitivity = 2.2f;
    public float orbitSensitivity = 4.0f;
    public bool invertY = false;

    [Header("Move / Pan / Dolly")]
    public float moveSpeed = 5f;
    public float boostMultiplier = 5f;
    public float slowMultiplier = 0.2f;
    public float panSensitivity = 0.5f;
    public float dollySensitivity = 8f;
    public float scrollSpeedTuning = 0.25f; // when RMB held, tunes fly speed

    [Header("Focus / Pivot")]
    public float minPivotDistance = 0.2f;
    public float maxFocusDistance = 500f;
    public float focusPadding = 1.15f;
    public LayerMask focusMask = ~0;
    public KeyCode focusKey = KeyCode.F;
    public KeyCode constantFocusKey = KeyCode.K;
    public bool constantFocusFlag = false;

    [Header("Runtime Selection")]        
    public bool focusOnSelection = false;
    public bool instantOnSelection = false;

    [Header("Smoothing (applies ONLY while focusing)")]
    public float focusPositionSmoothTime = 0.08f; // seconds, SmoothDamp
    public float focusRotationSmoothTime = 0.04f; // seconds, exp smoothing
    public float focusEndPosThreshold = 0.01f;    // meters to stop smoothing
    public float focusEndAngThreshold = 0.2f;     // degrees to stop smoothing
    public float focusMaxDuration = 1.0f;         // safety timeout (seconds)

    // Internal state
    private Vector3 _pivot;             // orbit pivot in world
    private float _pivotDistance = 5f;  // camera->pivot distance
    private bool  _hasPivot = false;

    private Vector3 _targetPos;
    private Quaternion _targetRot;

    private Vector3 _moveVel;           // for SmoothDamp
    private float _yaw, _pitch;

    private Camera _cam;
    private GameObject _prevSelected;

    // Focus-smoothing state
    private bool _isFocusing = false;
    private float _focusTimer = 0f;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _targetPos = transform.position;
        _targetRot = transform.rotation;

        Vector3 fwd = transform.forward;
        _pivot = transform.position + fwd * _pivotDistance;
        _hasPivot = true;

        Vector3 e = transform.rotation.eulerAngles;
        _yaw = e.y;
        _pitch = e.x;
    }

    void LateUpdate()
    {
        bool manualInput = HandleInput(); // returns true if any manual nav input detected
        ApplyTransform(manualInput);

        // Optional: auto-focus when your selection changes
        if (HoverSelectionSystem.Instance.selectedObject != null)
        {            
            if (focusOnSelection && HoverSelectionSystem.Instance.selectedObject != _prevSelected)
            {
                _prevSelected = HoverSelectionSystem.Instance.selectedObject;
                if (HoverSelectionSystem.Instance.selectedObject != null)
                    Focus(HoverSelectionSystem.Instance.selectedObject.transform, includeChildren: true, keepOrientation: true, instant: instantOnSelection);
            }
        }
    }

    /// <summary>
    /// Returns true if the user provided any manual navigation input this frame.
    /// Manual input cancels focus-smoothing.
    /// </summary>
    bool HandleInput()
    {
        float dt = Time.deltaTime;

        // Mouse deltas
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        bool lmb = Input.GetMouseButton(0);
        bool rmb = Input.GetMouseButton(1);
        bool mmb = Input.GetMouseButton(2);
        bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        bool manualInput = false;

        // ------- LOOK (RMB) -------
        if (rmb)
        {
            manualInput = true;

            float s = lookSensitivity;
            _yaw   += mx * s;
            _pitch += (invertY ? my : -my) * s; // correct: use my for pitch
            _pitch = Mathf.Clamp(_pitch, -89.9f, 89.9f);
            _targetRot = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        // ------- ORBIT (Alt+LMB) -------
        if (alt && lmb)
        {
            manualInput = true;

            if (!_hasPivot)
            {
                _pivot = transform.position + transform.forward * _pivotDistance;
                _hasPivot = true;
            }

            float s = orbitSensitivity;
            _yaw   += mx * s;
            _pitch += (invertY ? my : -my) * s;
            _pitch = Mathf.Clamp(_pitch, -89.9f, 89.9f);
            _targetRot = Quaternion.Euler(_pitch, _yaw, 0f);

            // Keep camera on a sphere around pivot
            _targetPos = _pivot - (_targetRot * Vector3.forward * _pivotDistance);
        }

// ------- PAN XZ with fixed anchor (MMB) -------
if (Input.GetMouseButtonDown(0) && !alt)
{
    // 1) Try to anchor on actual geometry under cursor
    var ray = _cam.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hit, maxFocusDistance, focusMask, QueryTriggerInteraction.Ignore))
    {
        _panAnchorWorld = hit.point;
        _panPlaneY      = _panAnchorWorld.y; // lock plane to the clicked surface height
    }
    else
    {
        // 2) Fallback: intersect with a horizontal plane (use pivot height)
        var plane = new Plane(Vector3.up, new Vector3(0f, _pivot.y, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            _panAnchorWorld = ray.GetPoint(enter);
            _panPlaneY      = _pivot.y; // lock plane for the whole drag
        }
        else
        {
            // no valid anchor; abort this pan
            _isPanningXZ = false;
            goto PanEndCheck;
        }
    }

    _isPanningXZ = true;
    // Any manual pan cancels focus smoothing
    _isFocusing = false;
}

if (_isPanningXZ && Input.GetMouseButton(0) && !alt)
{
    // Always raycast onto the SAME horizontal plane (no height drift)
    var plane = new Plane(Vector3.up, new Vector3(0f, _panPlaneY, 0f));
    var ray   = _cam.ScreenPointToRay(Input.mousePosition);

    if (plane.Raycast(ray, out float enter))
    {
        Vector3 currentOnPlane = ray.GetPoint(enter);   // y == _panPlaneY
        Vector3 delta = _panAnchorWorld - currentOnPlane;
        delta.y = 0f; // enforce pure XZ translation

        _targetPos += delta;
        _pivot     += delta; // keep orbit sphere & pan plane coherent
    }
}

// release or mode change ends pan
PanEndCheck:
if (Input.GetMouseButtonUp(2) || alt) _isPanningXZ = false;
        if(rmb)
        {
            // RMB held: scroll adjusts fly speed (like Scene view)
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                manualInput = true;

                float factor = 1f + scroll * scrollSpeedTuning;
                moveSpeed = Mathf.Clamp(moveSpeed * factor, 0.01f, 1000f);
            }
        }

        // ------- FLY (RMB + WASD/QE) -------
        if (rmb)
        {
            float speed = moveSpeed * (shift ? boostMultiplier : 1f) * (ctrl ? slowMultiplier : 1f);

            float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            float v = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            float u = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);

            if (h != 0f || v != 0f || u != 0f) manualInput = true;

            Vector3 moveLocal = new Vector3(h, u, v);
            if (moveLocal.sqrMagnitude > 1f) moveLocal.Normalize();

            Vector3 moveWorld = _targetRot * moveLocal * speed * dt;
            _targetPos += moveWorld;
            _pivot += moveWorld; // keep orbit distance consistent
        }

        // ------- FOCUS (F) -------

        if (Input.GetKeyDown(constantFocusKey))
        {
            constantFocusFlag = !constantFocusFlag;
        }
        
        if (constantFocusFlag)
        {
            manualInput = false; // pressing F is not "manual nav" that should cancel smoothing

            if (HoverSelectionSystem.Instance.selectedObject != null)
            {
                Focus(HoverSelectionSystem.Instance.selectedObject.transform, includeChildren: true, keepOrientation: true, instant: true);
            }
        }
        if (Input.GetKeyDown(focusKey))
        {
            manualInput = false; // pressing F is not "manual nav" that should cancel smoothing

            if (HoverSelectionSystem.Instance.selectedObject != null)
            {
                Focus(HoverSelectionSystem.Instance.selectedObject.transform, includeChildren: true, keepOrientation: true, instant: false);
            }
            // else if (!FocusByRaycast())
            // {
            //     // fallback: focus some point ahead
            //     Vector3 ahead = transform.position + transform.forward * Mathf.Min(20f, maxFocusDistance);
            //     FocusPoint(ahead, 5f, instant:false);
            // }
        }

        // Any manual navigation cancels focus smoothing immediately
        if (manualInput) _isFocusing = false;

        return manualInput;
    }

    void ApplyTransform(bool manualInputThisFrame)
    {
        if (_isFocusing)
        {
            _focusTimer += Time.deltaTime;

            // Smooth position
            transform.position = Vector3.SmoothDamp(
                transform.position, _targetPos, ref _moveVel, Mathf.Max(0.0001f, focusPositionSmoothTime));

            // Smooth rotation (exponential smoothing)
            float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, focusRotationSmoothTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, t);

            // Stop criteria (close enough or timeout)
            if ((transform.position - _targetPos).sqrMagnitude <= focusEndPosThreshold * focusEndPosThreshold &&
                Quaternion.Angle(transform.rotation, _targetRot) <= focusEndAngThreshold
                || _focusTimer >= focusMaxDuration)
            {
                transform.SetPositionAndRotation(_targetPos, _targetRot);
                _isFocusing = false;
            }
        }
        else
        {
            // No smoothing: snap to target each frame for ultra-responsive manual control
            transform.SetPositionAndRotation(_targetPos, _targetRot);
        }

        // Keep internal pivot distance in sync
        _pivotDistance = Mathf.Max(minPivotDistance, Vector3.Distance(transform.position, _pivot));
    }

    // ---- Public focus API ----
    public void Focus(Transform target, bool includeChildren = true, bool keepOrientation = true, bool instant = false)
    {
        if (!target) return;
        Bounds b = GetWorldBounds(target.gameObject, includeChildren);
        FocusBounds(b, keepOrientation, instant);
    }

    public void FocusPoint(Vector3 point, float desiredDistance, bool instant = false, bool keepOrientation = true)
    {
        _pivot = point;
        _pivotDistance = Mathf.Max(minPivotDistance, desiredDistance);
        if (!keepOrientation)
            _targetRot = Quaternion.LookRotation((_pivot - transform.position).normalized, Vector3.up);

        _targetPos = _pivot - (_targetRot * Vector3.forward * _pivotDistance);
        _hasPivot = true;

        if (instant)
        {
            transform.SetPositionAndRotation(_targetPos, _targetRot);
            _isFocusing = false;
        }
        else
        {
            _isFocusing = true;
            _focusTimer = 0f;
            _moveVel = Vector3.zero; // reset velocity for SmoothDamp
        }
    }

    bool FocusByRaycast()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, maxFocusDistance, focusMask, QueryTriggerInteraction.Ignore))
        {
            Bounds b = GetWorldBounds(hit.collider.gameObject, includeChildren: true);
            FocusBounds(b, keepOrientation: true, instant: false);
            return true;
        }
        return false;
    }

    void FocusBounds(Bounds b, bool keepOrientation = true, bool instant = false)
    {
        // Fit bounds into view using camera FOV (robust for big/small objects)
        float radius = b.extents.magnitude;
        if (radius < 1e-4f) radius = 0.5f;

        float fovRad = Mathf.Deg2Rad * Mathf.Clamp(_cam.fieldOfView, 1f, 179f);
        float tanHalfFov = Mathf.Tan(fovRad * 0.5f);

        float distV = radius / tanHalfFov;
        float distH = radius / (tanHalfFov * Mathf.Max(0.0001f, _cam.aspect));
        float dist = Mathf.Max(distV, distH) * focusPadding;
        dist = Mathf.Clamp(dist, minPivotDistance, maxFocusDistance);

        _pivot = b.center;
        _pivotDistance = dist;

        if (!keepOrientation)
            _targetRot = Quaternion.LookRotation((_pivot - transform.position).normalized, Vector3.up);

        _targetPos = _pivot - (_targetRot * Vector3.forward * _pivotDistance);
        _hasPivot = true;

        if (instant)
        {
            transform.SetPositionAndRotation(_targetPos, _targetRot);
            _isFocusing = false;
        }
        else
        {
            _isFocusing = true;
            _focusTimer = 0f;
            _moveVel = Vector3.zero;
        }
    }

    // Use Renderers if available, else fall back to Colliders, else transform position
    Bounds GetWorldBounds(GameObject root, bool includeChildren)
    {
        var rends = includeChildren ? root.GetComponentsInChildren<Renderer>() : root.GetComponents<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }

        var cols = includeChildren ? root.GetComponentsInChildren<Collider>() : root.GetComponents<Collider>();
        if (cols != null && cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
            return b;
        }

        // As a last resort use the transform position
        return new Bounds(root.transform.position, Vector3.one * 0.5f);
    }

    void OnDisable()
    {
        // Ensure no leftover smoothing state if the component is toggled
        _isFocusing = false;
        _moveVel = Vector3.zero;
    }
}
