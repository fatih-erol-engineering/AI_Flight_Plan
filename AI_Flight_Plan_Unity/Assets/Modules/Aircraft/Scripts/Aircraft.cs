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






    
    // public void MoveAircraftWithTime(float sec)
    // {
    //     int ct = 0;
    //     foreach (BSplineSegment segment in trajectory.bSplineSegments)
    //     {
    //         float startTime_s = segment.startPoint.time.second;
    //         float endTime_s = segment.endPoint.time.second;

    //         if ((sec <= endTime_s) && (sec >= startTime_s))
    //         {
    //             int n = segment.lr.positionCount;
    //             float lerpVal = (sec - startTime_s) / (endTime_s - startTime_s);
    //             lerpVal = Mathf.Clamp(lerpVal, 0, 1);
    //             float currentIdxFloat = Mathf.Lerp(0, n - 1, lerpVal);
    //             int currentIdx = Mathf.RoundToInt(currentIdxFloat);
    //             aircraftVisualObject.transform.position = segment.lr.GetPosition(currentIdx);
    //             break;
    //         }
    //         ct++;
    //     }
    // }

    // public Waypoint CreateWaypoint(Vector3 globalPosition)
    // {
    //     if (trajectory == null)
    //     {
    //         GameObject trajParentGO = Instantiate(theme.trajectoryPrefab, transform.position, transform.rotation, this.transform);

    //         trajectory = trajParentGO.GetComponent<Trajectory>();
    //         if (trajectory == null)
    //         {
    //             trajectory = trajParentGO.AddComponent<Trajectory>();
    //             trajectory.theme = theme;
    //         }
    //     }
    //     Waypoint wp = trajectory.CreateWaypoint(globalPosition);
    //     return wp;
    // }
    // public Waypoint CreateWaypoint(Vector3 globalPosition, float time_s)
    // {
    //     if (trajectory == null)
    //     {
    //         GameObject trajParentGO = Instantiate(theme.trajectoryPrefab, transform.position, transform.rotation, this.transform);
    //         trajectory = trajParentGO.GetComponent<Trajectory>();
    //         if (trajectory == null)
    //         {
    //             trajectory = trajParentGO.AddComponent<Trajectory>();
    //             trajectory.theme = theme;
    //         }
    //     }
    //     Waypoint wp = trajectory.CreateWaypoint(globalPosition, time_s);
    //     return wp;
    // }
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
    public void SetTime(TimeGame time)
    {
        this.time = time;
    }


}

