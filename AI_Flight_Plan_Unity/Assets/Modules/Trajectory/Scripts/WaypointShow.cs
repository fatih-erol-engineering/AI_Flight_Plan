// ...existing code...
using UnityEngine;
using UnityEngine.UI;

public class WaypointShow : MonoBehaviour
{
    [SerializeField] private Waypoint waypoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] Transform referenceMesh;

    [Header("Screen size")]
    [Tooltip("Desired vertical size on screen in pixels")]
    [SerializeField] private float desiredPixelHeight = 100f;
    [SerializeField] private float min_LocalScale = 1f;
    [SerializeField] private float max_LocalScale = 20f;


    // cached values for stable scaling
    private Vector3 originalLocalScale;
    private float originalMeshHeight = 1f; // mesh bounds height in local units


    [Header("Altitude Line")]
    [SerializeField] private LineRenderer altitudeLineRenderer;
    [SerializeField] private float altitudeLineLimit = -0.1f;
    // [SerializeField] private LayerMask hitMask;

    void OnValidate()
    {        
        AssignData();
    }
    public void AssignData()
    {
        if (!waypoint) waypoint = GetComponent<Waypoint>();
        if (!mainCamera) mainCamera = Camera.main;

        // find renderer (child or same object)

        originalLocalScale = referenceMesh.localScale;

        GameEvents.Instance.OnWaypointPositionChanged -= DrawAltitudeLine;
        GameEvents.Instance.OnWaypointSpawned -= DrawAltitudeLine;
        GameEvents.Instance.OnWaypointPositionChanged += DrawAltitudeLine;
            GameEvents.Instance.OnWaypointSpawned += DrawAltitudeLine;
    }

    void LateUpdate()
    {
        ShowWaypoint();
        DrawAltitudeLine(waypoint);
    }

    void DrawAltitudeLine(Waypoint wp, Vector3 newPosition)
    {
        DrawAltitudeLine(wp);
    }
    void DrawAltitudeLine(Waypoint wp)
    {
        if (waypoint == wp)
        {
            if (altitudeLineRenderer == null || waypoint == null) return;

            if (waypoint.transform.position.y <= altitudeLineLimit)
            {
                altitudeLineRenderer.enabled = false;
                return;
            }
            altitudeLineRenderer.enabled = true;

            Vector3 startPos = waypoint.transform.position;
            Vector3 endPos = new Vector3(waypoint.transform.position.x, altitudeLineLimit, waypoint.transform.position.z);

            altitudeLineRenderer.SetPosition(0, startPos);
            altitudeLineRenderer.SetPosition(1, endPos);
        }
    }
    public void ShowWaypoint()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;


        // distance from camera to object along view direction (positive in front)
        float distance = Vector3.Dot(referenceMesh.position - mainCamera.transform.position, mainCamera.transform.forward);
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
        scaleFactor = Mathf.Clamp(scaleFactor, min_LocalScale, max_LocalScale);
        Vector3 newLocalScale = originalLocalScale * scaleFactor;
        altitudeLineRenderer.startWidth = 0.05f * scaleFactor;
        altitudeLineRenderer.endWidth = 0.05f * scaleFactor;
        referenceMesh.localScale = newLocalScale;
    }


}
// ...existing code...