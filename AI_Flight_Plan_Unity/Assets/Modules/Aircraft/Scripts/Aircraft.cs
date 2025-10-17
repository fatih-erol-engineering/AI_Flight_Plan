using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Aircraft : SelectableBehaviour
{
    [Header("Aircraft Settings")]
    [field: SerializeField]
    public TimeGame time { get; private set; }
    [SerializeField]
    private AircraftProperties aircraftProperties;

    [field: SerializeField]
    public Trajectory trajectory { get; private set; }
    
    private MeshRenderer[] aircraftMeshRenderers;
    [SerializeField]
    private MeshRenderer[] _baseAircraftMeshRenderers;
    // Saklanan orijinal materyaller (her renderer için dizi)
    private Material[][] _originalMaterials;
    
    protected void OnEnable()
    {
        if(_baseAircraftMeshRenderers == null) _baseAircraftMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        // Eğer inspector'dan meshler verilmemişse fallback olarak çocukları kullan
        if (aircraftMeshRenderers == null || aircraftMeshRenderers.Length == 0)
            aircraftMeshRenderers = _baseAircraftMeshRenderers;

        // Orijinal materyalleri kaydet
        if (aircraftMeshRenderers != null && aircraftMeshRenderers.Length > 0)
        {
            _originalMaterials = new Material[aircraftMeshRenderers.Length][];
            for (int i = 0; i < aircraftMeshRenderers.Length; i++)
            {
                var r = aircraftMeshRenderers[i];
                if (r != null)
                    _originalMaterials[i] = r.materials; // renderer.materials returns an array (copy)
                else
                    _originalMaterials[i] = null;
            }
        }
    }
    override public void OnHoverEnter()
    {
        base.OnHoverEnter();
        if (base._selected) return;
        UpdateMaterial(theme.Hover);
    }
    override public void OnHoverExit()
    {
        base.OnHoverExit();
        if (base._selected) return;
        // Hover bittiğinde orijinal materyallere dön
        RestoreOriginalMaterials();
    }
    override public void OnSelect()
    {
        base.OnSelect();
        base._selected = true;
        UpdateMaterial(theme.Select);
    }
    override public void OnDeselect()
    {
        base._selected = false;
        base.OnDeselect();
        // Hover bittiğinde orijinal materyallere dön
        RestoreOriginalMaterials();
    }
    
    private void RestoreOriginalMaterials()
    {
        if (_originalMaterials == null || aircraftMeshRenderers == null) 
            return;

        for (int i = 0; i < aircraftMeshRenderers.Length && i < _originalMaterials.Length; i++)
        {
            var r = aircraftMeshRenderers[i];
            var mats = _originalMaterials[i];
            if (r == null || mats == null) 
                continue;
            r.materials = mats;
        }
    }

    public void MoveAircraftWithTime(float sec)
    {
        int ct = 0;
        foreach (BSplineSegment segment in trajectory.bSplineSegments)
        {
            float startTime_s = segment.startPoint.time.second;
            float endTime_s = segment.endPoint.time.second;

            if ((sec < endTime_s) && (sec >= startTime_s))
            {
                // int n = segment.lr.positionCount;
                // float lerpVal = (sec - startTime_s) / (endTime_s - startTime_s);
                // lerpVal = Mathf.Clamp(lerpVal, 0, 1);
                // float currentIdxFloat = Mathf.Lerp(0, n - 1, lerpVal);
                // int currentIdx = Mathf.RoundToInt(currentIdxFloat);

                // Vector3 pos1 = segment.lr.GetPosition(currentIdx);
                // Vector3 pos2 = segment.lr.GetPosition(currentIdx + 1);


                // Vector3.Lerp(pos1,pos2,)

                int n = segment.lr.positionCount;
                float deltaTime = (endTime_s - startTime_s);
                float gain = (sec - startTime_s) / deltaTime * n;
                int lowerIdx = Mathf.FloorToInt(gain);
                int upperIdx = lowerIdx + 1;

                if (upperIdx > n - 1)
                {
                    break;
                }
                else
                {
                    float lowerIdxTime = ((float)lowerIdx / n) * deltaTime + startTime_s;
                    float upperIdxTime = ((float)upperIdx / n) * deltaTime + startTime_s;

                    float lerpVal = Mathf.Clamp((sec - lowerIdxTime) / (upperIdxTime - lowerIdxTime), 0, 1);

                    Vector3 lowerPosition = segment.lr.GetPosition(lowerIdx);
                    Vector3 upperPosition = segment.lr.GetPosition(upperIdx);
                    Vector3 unitDir = (upperPosition - lowerPosition) / (upperPosition - lowerPosition).magnitude;
                    AlignLocalX_Absolute(transform, unitDir, Vector3.up*(-1f));
                    Vector3 aircraftPosition = Vector3.Lerp(lowerPosition, upperPosition, lerpVal);
                    transform.position = aircraftPosition;
                }
                break;
            }
            ct++;
        }
    }
    // Align object's local +X to unitDir, and control roll with upHint.
    // If upHint is null -> uses Vector3.up
    // Align object's local +X to unitDir, and control roll with upHint.
// If upHint is null -> uses Vector3.up


    public void AlignLocalX_Absolute(Transform t, Vector3 unitDir, Vector3? upHint = null)
    {
        // Guards
        if (unitDir.sqrMagnitude < 1e-10f || float.IsNaN(unitDir.x) || float.IsInfinity(unitDir.x))
            return;

        // Build an orthonormal basis where:
        // X = unitDir, Y = computed using upHint, Z = completes the right-handed frame
        Vector3 x = unitDir.normalized;
        Vector3 up = (upHint ?? Vector3.up).normalized;

        // If up is nearly parallel to x, choose a safe alternative up
        if (Mathf.Abs(Vector3.Dot(x, up)) > 0.999f)
            up = Mathf.Abs(x.y) < 0.9f ? Vector3.up : Vector3.right;

        // Create orthonormal axes
        Vector3 z = Vector3.Cross(up, x).normalized*(1);   // perpendicular to up & x
        Vector3 y = Vector3.Cross(z, x).normalized;    // completes right-handed basis

        // Compose rotation so that:
        //   forward (Z) = z,   up (Y) = y,   right (X) = x
        t.rotation = Quaternion.LookRotation(z, y);
    }

// Align the object's +Z axis to the given unit direction (world-space)
    public static void AlignZAxisTo(Transform t, Vector3 unitDir, Vector3? upHint = null)
    {
        // Guard: zero-length or NaN/Inf inputs are unsafe for rotations
        if (unitDir.sqrMagnitude < 1e-8f || float.IsNaN(unitDir.x) || float.IsInfinity(unitDir.x))
            return;

        // Ensure normalized
        unitDir = unitDir.normalized;

        // Choose an up vector (optional). World up by default.
        Vector3 up = upHint ?? Vector3.up;

        // If up is (nearly) parallel to direction, pick a safe alternative up
        if (Mathf.Abs(Vector3.Dot(unitDir, up)) > 0.999f)
            up = Mathf.Abs(unitDir.y) < 0.9f ? Vector3.up : Vector3.right;

        // Make forward (Z+) point to unitDir
        t.rotation = Quaternion.LookRotation(unitDir, up);
    }

    public void UpdateMaterial(Material material)
    {
        foreach (MeshRenderer renderer in aircraftMeshRenderers)
        {
            renderer.material = material;
            if (renderer.materials.Length > 1)
            {
                var mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = material;
                renderer.materials = mats;
            }
        }
    }
    public void SetTime(TimeGame _time)
    {
        time = _time;
    }


}

