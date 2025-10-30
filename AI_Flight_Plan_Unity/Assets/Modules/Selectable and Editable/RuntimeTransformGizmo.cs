using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Runtime transform gizmo: seçili nesne için X/Y/Z okları yaratır.
/// Fare ile ok üzerine tıklayıp sürüklerseniz seçili obje o eksen boyunca taşınır.
/// Entegrasyon: HoverSelectionSystem.selectedObject kullanır.
/// </summary>
public class RuntimeTransformGizmo : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float handleLength = 1.0f;
    [SerializeField] private float handleThickness = 0.06f;
    [SerializeField] private float tipRadius = 0.12f;
    [SerializeField] private LayerMask handleMask = ~0;
    [SerializeField] private KeyCode selectKey = KeyCode.G;

    private GameObject gizmoRoot;
    private GameObject handleX, handleY, handleZ;
    private Material matX, matY, matZ;
    private Transform target;
    private Camera cam => mainCamera ? mainCamera : Camera.main;

    // drag state
    private bool dragging;
    private Vector3 dragAxis;
    private Vector3 dragStartMousePoint;
    private Vector3 dragStartTargetPos;

    void Awake()
    {
        CreateMaterials();
        CreateGizmoRoot();
    }

    void Update()
    {
        if (SelectionSystem.Instance == null) return;

        var selObj = SelectionSystem.Instance.selectedObject;
        if (selObj != null)
        {
            if (target != selObj.transform)
            {
                AttachTo(selObj.transform);
            }
            UpdateGizmoTransform();
            HandleInput();
        }
        else
        {
            Detach();
        }
    }

    private void CreateMaterials()
    {
        var sh = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
        matX = new Material(sh); matX.color = new Color(1f, 0.2f, 0.2f, 1f);
        matY = new Material(sh); matY.color = new Color(0.2f, 1f, 0.2f, 1f);
        matZ = new Material(sh); matZ.color = new Color(0.2f, 0.6f, 1f, 1f);
    }

    private void CreateGizmoRoot()
    {
        gizmoRoot = new GameObject("RuntimeTransformGizmo");
        gizmoRoot.transform.SetParent(transform, false);
        gizmoRoot.SetActive(false);

        handleX = CreateHandle("Handle_X", Vector3.right, matX);
        handleY = CreateHandle("Handle_Y", Vector3.up, matY);
        handleZ = CreateHandle("Handle_Z", Vector3.forward, matZ);

        handleX.transform.SetParent(gizmoRoot.transform, false);
        handleY.transform.SetParent(gizmoRoot.transform, false);
        handleZ.transform.SetParent(gizmoRoot.transform, false);
    }

    private GameObject CreateHandle(string name, Vector3 axis, Material mat)
    {
        var go = new GameObject(name);

        // shaft
        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "shaft";
        shaft.transform.SetParent(go.transform, false);
        shaft.transform.localScale = new Vector3(handleThickness, handleLength * 0.5f, handleThickness);
        shaft.transform.localPosition = axis * (handleLength * 0.5f);
        shaft.transform.localRotation = Quaternion.FromToRotation(Vector3.up, axis);
        var sR = shaft.GetComponent<Renderer>();
        sR.sharedMaterial = mat;
        // use collider on parent for picking
        DestroyImmediate(shaft.GetComponent<Collider>());

        // tip
        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "tip";
        tip.transform.SetParent(go.transform, false);
        tip.transform.localScale = Vector3.one * tipRadius;
        tip.transform.localPosition = axis * (handleLength + (tipRadius * 0.5f));
        var tR = tip.GetComponent<Renderer>();
        tR.sharedMaterial = mat;
        // keep collider on tip for raycast picking
        var col = tip.GetComponent<Collider>();
        col.isTrigger = false;
        // add an additional BoxCollider on parent to enlarge pick area
        var box = go.AddComponent<BoxCollider>();
        // approximate box size along axis
        var size = new Vector3(handleLength, handleLength, handleLength) * (handleThickness * 2f + tipRadius);
        // orient box along axis
        box.size = size;
        box.center = axis * (handleLength * 0.6f);
        return go;
    }

    private void AttachTo(Transform t)
    {
        target = t;
        gizmoRoot.SetActive(true);
        UpdateGizmoTransform();
    }

    private void Detach()
    {
        target = null;
        gizmoRoot.SetActive(false);
        dragging = false;
    }

    private void UpdateGizmoTransform()
    {
        if (target == null) return;
        gizmoRoot.transform.position = target.position;
        // optional: align gizmo to world axes (editor-like)
        gizmoRoot.transform.rotation = Quaternion.identity;
        // scale handles so they remain visible relative to camera distance
        float scale = 1f;
        if (cam != null)
        {
            float d = Vector3.Distance(cam.transform.position, target.position);
            scale = Mathf.Max(0.4f, d * 0.08f);
        }
        gizmoRoot.transform.localScale = Vector3.one * scale;
    }

    private void HandleInput()
    {
        if (cam == null || target == null) return;

        // start drag
        if (!dragging && Input.GetKeyDown(selectKey))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, handleMask))
            {
                // determine which handle was hit by name
                var hroot = hit.collider.transform;
                var go = hit.collider.gameObject;
                // parent search to our handle
                Transform handleParent = hit.collider.transform;
                while (handleParent != null && handleParent.parent != gizmoRoot.transform)
                    handleParent = handleParent.parent;
                if (handleParent == null) handleParent = hit.collider.transform;

                // decide axis
                if (handleParent.name.Contains("Handle_X")) dragAxis = gizmoRoot.transform.right;
                else if (handleParent.name.Contains("Handle_Y")) dragAxis = gizmoRoot.transform.up;
                else dragAxis = gizmoRoot.transform.forward;

                // create plane perpendicular to axis at target position
                var plane = new Plane(dragAxis, target.position);
                if (plane.Raycast(ray, out float enter))
                {
                    dragStartMousePoint = ray.GetPoint(enter);
                    dragStartTargetPos = target.position;
                    dragging = true;
                }
            }
        }

        // dragging
        if (dragging && Input.GetKey(selectKey))
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            float offset = -0.01f;
            var plane = new Plane(dragAxis, dragStartTargetPos + dragAxis * offset);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter) - dragAxis * offset;
                float delta = Vector3.Dot(hitPoint - dragStartMousePoint, dragAxis);
                // target.position = dragStartTargetPos + dragAxis * delta;
                target.GetComponent<Waypoint>().SetPosition(dragStartTargetPos + dragAxis * delta);
            }
        }

        // end drag
        if (dragging && Input.GetKeyUp(selectKey))
        {
            dragging = false;
        }
    }

    void OnDestroy()
    {
        if (gizmoRoot != null) Destroy(gizmoRoot);
        if (matX != null) Destroy(matX);
        if (matY != null) Destroy(matY);
        if (matZ != null) Destroy(matZ);
    }
}