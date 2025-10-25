using UnityEngine;

[ExecuteAlways]
public class TubeManager : MonoBehaviour
{
    [SerializeField] public Transform start;
    [SerializeField] public Transform end;
    [SerializeField] private float radius = 5f;

    [Header("Appearance")]
    [SerializeField] public bool isCollided { get; set { if (isCollided == value) return; isCollided = value; } }

    [SerializeField, ColorUsage(true, true)] public Color edgeColor = Color.white;
    [SerializeField] public Color surfaceColor = Color.blue;
    [SerializeField] public float edgeSize = 0.1f;


    [SerializeField, HideInInspector, ColorUsage(true, true)] private Color prev_edgeColor = Color.white;
    [SerializeField, HideInInspector] private Color prev_surfaceColor = Color.blue;
    [SerializeField, HideInInspector] private float prev_edgeSize = 0.1f;

    [SerializeField, HideInInspector] private float length = 10f;
    [SerializeField, HideInInspector] private float prev_radius = 5f;
    [SerializeField, HideInInspector] private float prev_length = 10f;
    [SerializeField, HideInInspector] private Material material;
    static readonly int BaseColorID = Shader.PropertyToID("_surfaceColor"); // URP Lit
    static readonly int EdgeColorID = Shader.PropertyToID("_edgeColor"); // URP Lit
    static readonly int RadiusID = Shader.PropertyToID("_radius"); // URP Lit
    static readonly int LengthID = Shader.PropertyToID("_length"); // URP Lit
    static readonly int EdgeSizeID = Shader.PropertyToID("_edgeSize"); // URP Lit
    [SerializeField, HideInInspector] MaterialPropertyBlock mpb;
    [SerializeField, HideInInspector] Renderer rend;

    void Awake()
    {
        AssignData();
    }


    
    public void Clear()
    {
        Debug.Log("Clearing TubeManager not implemented yet.");
    }
    public void AssignData()
    {
        if (!material)
        {
            // material = transform.GetComponent<Renderer>().material;
            rend = GetComponent<Renderer>();
            mpb = new MaterialPropertyBlock();

        }        
    }
    // void Update()
    // {
    //     UpdateTube();
    // }

    // Update is called once per frame
    public void UpdateTubeImmidiately()
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
        UpdateLength();
        UpdateRadius();
        UpdateAppearance();
        prev_edgeColor = edgeColor;
        prev_surfaceColor = surfaceColor;
        prev_edgeSize = edgeSize;
        prev_radius = radius;
        prev_length = length;
    }
    public void Tick()
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
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(RadiusID, radius);
        rend.SetPropertyBlock(mpb);
    }
    void UpdateLength()
    {
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(LengthID, length);
        rend.SetPropertyBlock(mpb);
    }
    void UpdateAppearance()
    {
        rend.GetPropertyBlock(mpb);
        mpb.SetColor(EdgeColorID, edgeColor);
        mpb.SetColor(BaseColorID, surfaceColor);
        mpb.SetFloat(EdgeSizeID, edgeSize);
        rend.SetPropertyBlock(mpb);
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
    public void SetEdgeSize(float _size)
    {
        if (edgeSize != _size)
        {
            edgeSize = _size;
            UpdateAppearance();
        }
    }
    public void SetStartAndEndPositions(Vector3 startPos, Vector3 endPos)
    {
        start.position = startPos;
        end.position = endPos;
        Tick();
    }
    public bool CheckPositionInsideOrNot(Vector3 vector3)
    {
        // Check if the position is inside the tube's volume
        Vector3 closestPoint = Vector3.ClampMagnitude(vector3 - start.position, length) + start.position;
        float distance = Vector3.Distance(closestPoint, end.position);
        return distance <= radius;
    }
}
