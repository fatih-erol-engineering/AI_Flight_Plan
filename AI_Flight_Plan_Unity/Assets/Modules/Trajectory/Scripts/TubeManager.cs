using UnityEngine;

public class TubeManager : MonoBehaviour
{
    [SerializeField] public Vector3 start;
    [SerializeField] public Vector3 end;
    [SerializeField] private float radius = 5f;

    [Header("Appearance")]

    [SerializeField, ColorUsage(true, true)] public Color edgeColor = Color.white;
    [SerializeField, ColorUsage(true, true)] public Color surfaceColor = Color.blue;
    [SerializeField] public float edgeSize = 0.1f;


    [SerializeField, HideInInspector, ColorUsage(true, true)] private Color prev_edgeColor = Color.white;
    [SerializeField, HideInInspector, ColorUsage(true, true)] private Color prev_surfaceColor = Color.blue;
    [SerializeField, HideInInspector] private float prev_edgeSize = 0.1f;

    [SerializeField, HideInInspector] private float length = 10f;
    [SerializeField, HideInInspector] private float prev_radius = 5f;    
    [SerializeField, HideInInspector] private float prev_length = 10f;
    [SerializeField, HideInInspector] private Material material;
    public void Create()
    {
        material = transform.GetComponent<Renderer>().sharedMaterial;        

        // if (start == null || end == null)
        // {
        //     start = new GameObject("Start").transform;
        //     end = new GameObject("End").transform;

        //     start.SetParent(transform);
        //     end.SetParent(transform);
        // };
        prev_radius = radius;

        length = Vector3.Distance(start, end);
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
        Vector3 a = start;
        Vector3 b = end;
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

    public void SetRadius(float _radius)
    {
        if (radius != _radius)
        {
            radius = _radius;
            UpdateRadius();
        }
    }
    public void SetEdgeColor(Color _color)
    {
        if (edgeColor != _color)
        {
            edgeColor = _color;
            UpdateAppearance();
        }
    }
    public void SetSurfaceColor(Color _color)
    {
        if (surfaceColor != _color)
        {
            surfaceColor = _color;
            UpdateAppearance();
        }
    }

}
