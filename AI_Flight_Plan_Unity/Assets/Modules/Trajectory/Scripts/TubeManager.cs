using UnityEngine;

[ExecuteAlways]

public class TubeManager : MonoBehaviour
{


    [SerializeField] private bool isCollided;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;

    [Header("Appearance Properties")]
    [SerializeField] private bool isDrivenByThemeManager = false;
    [SerializeField] private Theme currentTheme;
    [SerializeField, ColorUsage(true, true)] private Color edgeColor;
    [SerializeField] private Color surfaceColor;
    [SerializeField] private float edgeSize;
    [SerializeField] private float radius;
    [SerializeField] private float length;

    [SerializeField, HideInInspector] private Material material;
    static readonly int BaseColorID = Shader.PropertyToID("_surfaceColor"); // URP Lit
    static readonly int EdgeColorID = Shader.PropertyToID("_edgeColor"); // URP Lit
    static readonly int RadiusID = Shader.PropertyToID("_radius"); // URP Lit
    static readonly int LengthID = Shader.PropertyToID("_length"); // URP Lit
    static readonly int EdgeSizeID = Shader.PropertyToID("_edgeSize"); // URP Lit
    [SerializeField, HideInInspector] MaterialPropertyBlock mpb;
    [SerializeField, HideInInspector] Renderer rend;



    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (material == null)
        {
            material = rend.sharedMaterial;
        }
        mpb = new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);
        OnValidate();
    }
    void OnValidate()
    {
        rend = GetComponent<Renderer>();
        if (material == null)
        {
            material = rend.sharedMaterial;
        }
        mpb = new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);
        SetEdgeColor(edgeColor, true);
        SetSurfaceColor(surfaceColor, true);
        SetEdgeSize(edgeSize, true);
        SetRadius(radius, true);
        SetLength(length, true);
        SetIsCollided(isCollided, true);
        SetStartPosition(startPosition, true);
        SetEndPosition(endPosition, true);
    }

    void UpdateColorWithTheme()
    {
        if (ThemeManager.Instance != null)
        {
            isDrivenByThemeManager = true;
            currentTheme = ThemeManager.Instance.theme;
            if (isCollided)
            {
                SetEdgeColor(currentTheme.tubeEdgeColor_collided);
                SetSurfaceColor(currentTheme.tubeSurfaceColor_collided);
            }
            else
            {
                SetEdgeColor(currentTheme.tubeEdgeColor_nonCollided);
                SetSurfaceColor(currentTheme.tubeSurfaceColor_nonCollided);
            }
            SetEdgeSize(currentTheme.tubeEdgeSize);
        }
        else
        {
            isDrivenByThemeManager = false;
            currentTheme = null;
        }
    }


    public void SetEdgeColor(Color _color, bool isImmediate = false)
    {
        if (edgeColor != _color || isImmediate)
        {
            edgeColor = _color;
            rend.GetPropertyBlock(mpb);
            mpb.SetColor(EdgeColorID, edgeColor);
            rend.SetPropertyBlock(mpb);
        }
    }

    public void SetSurfaceColor(Color _color, bool isImmediate = false)
    {
        if (_color != surfaceColor || isImmediate)
        {
            surfaceColor = _color;
            rend.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorID, surfaceColor);
            rend.SetPropertyBlock(mpb);
        }
    }

    public void SetEdgeSize(float _size, bool isImmediate = false)
    {
        if (edgeSize != _size || isImmediate)
        {
            edgeSize = _size;
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(EdgeSizeID, edgeSize);
            rend.SetPropertyBlock(mpb);
        }
    }
    public void SetLength(float _val, bool isImmediate = false)
    {
        if (length != _val || isImmediate)
        {
            length = _val;
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(LengthID, length);
            rend.SetPropertyBlock(mpb);
        }
    }
    public void SetRadius(float _val, bool isImmediate = false)
    {
        if (radius != _val || isImmediate)
        {
            radius = _val;
            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(RadiusID, radius);
            rend.SetPropertyBlock(mpb);
        }
    }
    public void SetIsCollided(bool _val, bool isImmediate = false)
    {
        if (isCollided != _val || isImmediate)
        {
            isCollided = _val;
            UpdateColorWithTheme();
        }
    }
    public bool GetIsCollided()
    {
        return isCollided;
    }

    public void SetStartPosition(Vector3 _val, bool isImmediate = false)
    {
        if (startPosition != _val || isImmediate)
        {
            startPosition = _val;
            SetLengthWithStartAndEndPositions();
        }
    }
    public void SetEndPosition(Vector3 _val, bool isImmediate = false)
    {
        if (endPosition != _val || isImmediate)
        {
            endPosition = _val;
            SetLengthWithStartAndEndPositions();
        }
    }
    public void SetLengthWithStartAndEndPositions()
    {
        Vector3 a = startPosition;
        Vector3 b = endPosition;
        Vector3 dir = b - a;
        float newLength = dir.magnitude;
        if (newLength <= Mathf.Epsilon) return;

        // position: midpoint between start and end
        transform.position = (a + b) * 0.5f;

        // rotation: align local Y (up) to direction
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
        SetLength(newLength);
    }

    public bool CheckPositionInsideOrNot(Vector3 vector3)
    {
        bool isInside = false;
        Vector3 def1 = endPosition - startPosition;
        Vector3 def2 = vector3 - startPosition;
        float projectionLength = Vector3.Dot(def2, def1.normalized);
        float angle_rad = Mathf.Acos(projectionLength / (def2.magnitude));
        float distance = def2.magnitude * Mathf.Sin(angle_rad);
        if (distance <= radius)
        {
            // Check if the projection of vector3 onto the tube axis is within the tube length            
            if (projectionLength >= 0 && projectionLength <= def1.magnitude)
            {
                isInside = true;
            }
        }
        return isInside;
    }
}
