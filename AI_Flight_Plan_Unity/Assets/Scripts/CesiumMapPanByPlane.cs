using UnityEngine;

public class CesiumMapPanByPlane : MonoBehaviour
{
    public Transform mapRoot;    // Cesium tileset parent
    public Camera cam;
    public int mouseButton = 0;  // 0: sol, 2: orta tuş
    public float sensitivity = 1.0f;

    private Plane dragPlane;
    private bool isDragging;
    private Vector3 prevHit;
    private float lockY;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(mouseButton))
        {
            if (TryMakeHorizontalPlaneHit(Input.mousePosition, out Vector3 startPoint))
            {
                isDragging = true;
                prevHit = startPoint;
                lockY = mapRoot.position.y; // Y sabitlenecek
            }
        }

        if (Input.GetMouseButton(mouseButton) && isDragging)
        {
            if (RaycastToDragPlane(Input.mousePosition, out Vector3 currHit))
            {
                Vector3 delta = (-1f)*(currHit - prevHit) * sensitivity;

                // Sadece yatay kayma
                delta.y = 0f;

                // Yeni pozisyon
                Vector3 targetPos = mapRoot.position - delta;
                targetPos.y = lockY; // Y sabit tut

                mapRoot.position = targetPos;
                prevHit = currHit;
            }
        }

        if (Input.GetMouseButtonUp(mouseButton))
        {
            isDragging = false;
        }
    }

    // MouseDown anında yatay düzlem kur
    bool TryMakeHorizontalPlaneHit(Vector3 mousePos, out Vector3 hitPoint)
    {
        hitPoint = default;
        dragPlane = new Plane(Vector3.up, new Vector3(0f, mapRoot.position.y, 0f));

        Ray ray = cam.ScreenPointToRay(mousePos);
        if (dragPlane.Raycast(ray, out float enter))
        {
            hitPoint = ray.GetPoint(enter);
            return true;
        }
        return false;
    }

    bool RaycastToDragPlane(Vector3 mousePos, out Vector3 hitPoint)
    {
        hitPoint = default;
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (dragPlane.Raycast(ray, out float enter))
        {
            hitPoint = ray.GetPoint(enter);
            return true;
        }
        return false;
    }
}
