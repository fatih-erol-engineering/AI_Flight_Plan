using UnityEngine;

public class ControlPointShow : MonoBehaviour
{
    [SerializeField] private ControlPoint controlPoint;
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


    void OnValidate()
    {
        AssignData();
    }
    public void AssignData()
    {
        if (!controlPoint) controlPoint = GetComponent<ControlPoint>();
        if (!mainCamera) mainCamera = Camera.main;

        // find renderer (child or same object)

        originalLocalScale = referenceMesh.localScale;
    }

    void LateUpdate()
    {
        ShowWaypoint();
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
        referenceMesh.localScale = newLocalScale;
    }


}
// ...existing code...