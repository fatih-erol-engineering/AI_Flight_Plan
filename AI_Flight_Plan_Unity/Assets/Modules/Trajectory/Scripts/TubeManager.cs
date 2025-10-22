using UnityEngine;

public class TubeManager : MonoBehaviour
{
    [SerializeField] public Transform start;
    [SerializeField] public Transform end;
    [SerializeField] public float radius = 5f;

    [Header("Appearance")]
    [SerializeField] public Color edgeColor = Color.white;
    [SerializeField] public Color surfaceColor = Color.blue;
    [SerializeField] public float edgeSize = 0.1f;


    [SerializeField] private Color prev_edgeColor = Color.white;
    [SerializeField] private Color prev_surfaceColor = Color.blue;
    [SerializeField] private float prev_edgeSize = 0.1f;

    [SerializeField, HideInInspector] private float length = 10f;
    [SerializeField, HideInInspector] private float prev_radius = 5f;    
    [SerializeField, HideInInspector] private float prev_length = 10f;
    [SerializeField, HideInInspector] private Material material;
    void Start()
    {
        material = transform.GetComponent<Renderer>().material;
        Debug.Log("Material assigned: " + material.name);

        if (start == null || end == null)
        {
            start = new GameObject("Start").transform;
            end = new GameObject("End").transform;

            start.SetParent(transform);
            end.SetParent(transform);
        };
        prev_radius = radius;

        length = Vector3.Distance(start.position, end.position);
        prev_length = length;
        UpdateTube();
    }
    void Update()
    {
        UpdateTube();
    }

    // Update is called once per frame
    public void UpdateTube()
    {
        Vector3 a = start.position;
        Vector3 b = end.position;
        Vector3 dir = b - a;
        float newLength = dir.magnitude;
        if (newLength <= Mathf.Epsilon) return;

        // position: midpoint between start and end
        transform.position = (a + b) * 0.5f;

        // rotation: align local Y (up) to direction
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        length = newLength;
        
        if (prev_length != length)
        {
            UpdateLength();
        }
        if (prev_radius != radius)
        {
            UpdateRadius();
        }
        if (prev_edgeColor != edgeColor || prev_surfaceColor != surfaceColor || prev_edgeSize != edgeSize)
        {
            UpdateAppearance();
        }

        prev_edgeColor = edgeColor;
        prev_surfaceColor = surfaceColor;
        prev_edgeSize = edgeSize;
        prev_radius = radius;
        prev_length = length;

    }
    void UpdateRadius()
    {
        material.SetFloat("radius", radius);
        material.SetFloat("_radius", radius);
    }
    void UpdateLength()
    {
        material.SetFloat("length", length);
        material.SetFloat("_length", length);
    }
    void UpdateAppearance()
    {
        material.SetColor("edgeColor", edgeColor);
        material.SetColor("_edgeColor", edgeColor);
        material.SetColor("surfaceColor", surfaceColor);
        material.SetColor("_surfaceColor", surfaceColor);
        material.SetFloat("edgeSize", edgeSize);
        material.SetFloat("_edgeSize", edgeSize);
    }
}
