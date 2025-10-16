// ...existing code...
using UnityEngine;

public class WaypointShow : MonoBehaviour
{
    [SerializeField] private Waypoint waypoint;
    [SerializeField] private Camera mainCamera;

    [Header("Screen size")]
    [Tooltip("Desired vertical size on screen in pixels")]
    [SerializeField] private float desiredPixelHeight = 48f;

    [Tooltip("If true, object will face the camera every frame")]
    [SerializeField] private bool faceCamera = true;

    // cached values for stable scaling
    private Renderer targetRenderer;
    private MeshFilter targetMeshFilter;
    private Vector3 originalLocalScale;
    private float originalMeshHeight = 1f; // mesh bounds height in local units

    void Awake()
    {
        if (!waypoint) waypoint = GetComponent<Waypoint>();
        if (!mainCamera) mainCamera = Camera.main;

        // find renderer (child or same object)
        targetRenderer = (waypoint != null) ? waypoint.GetComponentInChildren<Renderer>() : GetComponentInChildren<Renderer>();
        targetMeshFilter = targetRenderer != null ? targetRenderer.GetComponent<MeshFilter>() : null;

        originalLocalScale = transform.localScale;

        if (targetMeshFilter != null && targetMeshFilter.sharedMesh != null)
        {
            // mesh bounds are in local space
            originalMeshHeight = targetMeshFilter.sharedMesh.bounds.size.y;
            if (originalMeshHeight <= 0f) originalMeshHeight = 1f;
        }
        else if (targetRenderer != null)
        {
            // fallback: use renderer.world bounds and remove lossyScale to approximate local size
            var worldHeight = targetRenderer.bounds.size.y;
            var lossyY = transform.lossyScale.y;
            originalMeshHeight = lossyY != 0f ? worldHeight / lossyY : worldHeight;
            if (originalMeshHeight <= 0f) originalMeshHeight = 1f;
        }
        else
        {
            Debug.LogWarning($"[WaypointShow] No Renderer found on '{gameObject.name}' or children. Scaling will use defaults.");
        }
    }

    void Update()
    {
        ShowWaypoint();
    }


    public void ShowWaypoint()
    {
                if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Face camera
        if (faceCamera)
        {
            // Make the object's forward face the camera (billboard).
            // Use camera up to avoid rolling.
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position, mainCamera.transform.up);
            // If the object's front is opposite, use: Quaternion.LookRotation(mainCamera.transform.position - transform.position)
        }

        // Maintain constant pixel height
        if (targetRenderer == null && targetMeshFilter == null) return;

        // distance from camera to object along view direction (positive in front)
        float distance = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
        if (distance <= 0.001f) distance = 0.001f; // avoid zero or behind-camera issues

        float desiredWorldHeight;

        if (mainCamera.orthographic)
        {
            // orthographic: screen world-height = orthographicSize * 2
            float screenWorldHeight = mainCamera.orthographicSize * 2f;
            desiredWorldHeight = screenWorldHeight * (desiredPixelHeight / (float)Screen.height);
        }
        else
        {
            // perspective: world height at distance = 2 * distance * tan(fov/2)
            float fovRad = mainCamera.fieldOfView * Mathf.Deg2Rad;
            float worldHeightAtDistance = 2f * distance * Mathf.Tan(fovRad * 0.5f);
            desiredWorldHeight = worldHeightAtDistance * (desiredPixelHeight / (float)Screen.height);
        }

        // compute scale factor relative to original mesh height
        float scaleFactor = originalMeshHeight > 0f ? (desiredWorldHeight / originalMeshHeight) : 1f;
        Vector3 newLocalScale = originalLocalScale * scaleFactor;
        transform.localScale = newLocalScale;
    }
}
// ...existing code...