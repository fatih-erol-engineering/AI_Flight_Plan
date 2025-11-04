using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]

[ExecuteAlways]
public class Aircraft : MonoBehaviour, ISelectable, IEditable
{
    [Header("Aircraft Settings")]
    public int id;
    public TimeGame time;

    public AircraftProperties aircraftProperties;
    public TrajectoryDrawer trajectory;

    [Header("Selection Appearance")]
    [SerializeField] private MeshRenderer highlightMeshRenderer;
    [SerializeField] private bool isSelected = false;
    private MaterialPropertyBlock mpb;
    private Color highlightEdgeColor;
    private float highlightEdgeWidth;
    static readonly int EdgeColorID = Shader.PropertyToID("_selectionEdgeColor"); // URP Lit
    static readonly int EdgeWidthID = Shader.PropertyToID("_selectionEdgeWidth"); // URP Lit
    [Header("Conflict Solver Settings")]

    [Range(0f, 1f)]
    public float timeOrPositionChangeVal = 1f; // 0 means time can be changed 1 means position can be changed
    [Range(0f, 1f)]
    public float nonEditableOrEditableVal = 1f; // 0 means non editable, 1 means editable
    [SerializeField] private TimeGame deltaTime; // time step for conflict solver in seconds    

    // [SerializeField] private bool _neonWaveActive = true;

    // public bool neonWaveActive
    // {
    //     get => _neonWaveActive;
    //     set => SetNeonWaveActive(value);
    // }


    [SerializeField] private NeonWave neonWave;

    // void Update()
    // {
    //     SetDeltaTime(deltaTime, true);
    // }
    // public void SetNeonWaveActive(bool _isActive, bool isImmediate = false)
    // {
    //     if (_neonWaveActive != _isActive)
    //     {
    //         _neonWaveActive = _isActive;
    //         neonWave.gameObject.SetActive(_neonWaveActive);
    //     }
    // }

    private bool _eventsBound;

    void OnEnable()
    {
        AssignData();
        if (!_eventsBound && GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEditableEnter += OnEditableEnter;
            GameEvents.Instance.OnEditableExit += OnEditableExit;
            GameEvents.Instance.OnTimeChanged += MoveAircraftWithTime;
            _eventsBound = true;
        }
    }
    void OnDisable()
    {
        if (_eventsBound && GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEditableEnter -= OnEditableEnter;
            GameEvents.Instance.OnEditableExit -= OnEditableExit;
            GameEvents.Instance.OnTimeChanged -= MoveAircraftWithTime;
            _eventsBound = false;
        }
    }

    void AssignData()
    {
        mpb = new MaterialPropertyBlock();
        highlightMeshRenderer.GetPropertyBlock(mpb);
        SetHighlightEdgeWidth(1f, true);
        highlightMeshRenderer.gameObject.SetActive(false);
        deltaTime = new TimeGame(0f);

    }

    public void OnHoverEnter()
    {
        if (isSelected) return;
        SetHighlightMeshRendererEnabled(true);

        if (ThemeManager.Instance == null) return;
        SetHighlightEdgeColor(ThemeManager.Instance.theme.Hover);
        // SetHighlightEdgeWidth(1f);
    }
    public void OnHoverExit()
    {
        if (isSelected) return;
        // SetHighlightEdgeWidth(0f);
        SetHighlightMeshRendererEnabled(false);
        SetHighlightEdgeColor(ThemeManager.Instance.theme.Preview);
    }
    public void OnSelect()
    {
        SetHighlightMeshRendererEnabled(true);
        // SetHighlightEdgeWidth(1f);
        SetIsSelected(true);
        if (ThemeManager.Instance == null) return;
        SetHighlightEdgeColor(ThemeManager.Instance.theme.Select);
    }
    public void OnDeselect()
    {
        // SetHighlightEdgeWidth(0f);
        SetHighlightMeshRendererEnabled(false);
        SetIsSelected(false);
        if (ThemeManager.Instance == null) return;
        SetHighlightEdgeColor(ThemeManager.Instance.theme.Preview);
    }
    public void SetHighlightMeshRendererEnabled(bool _isEnabled)
    {
        if (highlightMeshRenderer != null)
        {
            highlightMeshRenderer.gameObject.SetActive(_isEnabled);
        }
    }

    public void SetTime(TimeGame _time, bool isImmediate = false)
    {
        if (time != _time || isImmediate)
        {
            time = _time;
        }
    }
    public void SetDeltaTime(TimeGame _deltaTime, bool isImmediate = false)
    {
        if (deltaTime != _deltaTime || isImmediate)
        {
            deltaTime = _deltaTime;
            trajectory.waypointFactory.waypointList.ForEach(wp => wp.SetTime(new TimeGame(wp.originalTime.second + deltaTime.second)));
        }
    }
    public void AddDeltaTime(TimeGame _deltaTime, bool isImmediate = false)
    {
        deltaTime.SetTime(deltaTime.second + _deltaTime.second);
        trajectory.waypointFactory.waypointList.ForEach(wp => wp.SetTime(new TimeGame(wp.originalTime.second + deltaTime.second)));
    }
    public void SetHighlightEdgeColor(Color _color, bool isImmediate = false)
    {
        if (highlightEdgeColor != _color || isImmediate)
        {
            highlightEdgeColor = _color;
            mpb.SetColor(EdgeColorID, _color);
            highlightMeshRenderer.SetPropertyBlock(mpb);
            neonWave.SetColor(_color, isImmediate);
        }
    }
    public void SetHighlightEdgeWidth(float _width, bool isImmediate = false)
    {
        if (highlightEdgeWidth != _width || isImmediate)
        {
            highlightEdgeWidth = _width;
            mpb.SetFloat(EdgeWidthID, _width);
            highlightMeshRenderer.SetPropertyBlock(mpb);
        }
    }

    public void SetIsSelected(bool _isSelected, bool isImmediate = false)
    {
        if (isSelected != _isSelected || isImmediate)
        {
            isSelected = _isSelected;
        }
    }


    public void SetTimeOrPositionChange(float _val)
    {
        if (_val != timeOrPositionChangeVal)
        {
            timeOrPositionChangeVal = _val;
        }
    }

    public void SetNonEditableOrEditableVal(float _val)
    {
        if (_val != nonEditableOrEditableVal)
        {
            nonEditableOrEditableVal = _val;
        }
    }

    public void MoveAircraftWithTime(float sec)
    {
        foreach (BSplineDrawer segment in trajectory.GetSegmentDrawers())
        {

            float startTime_s = segment.startTime.second;
            float endTime_s = segment.endTime.second;

            if ((sec < endTime_s) && (sec >= startTime_s))
            {
                for (int i = 0; i < segment.trajectoryPoints.Length; i++)
                {
                    if (segment.trajectoryPoints[i].time.second > sec)
                    {
                        int upperIdx = i;
                        int lowerIdx = Mathf.Max(0, i - 1);

                        float lerpVal = (sec - segment.trajectoryPoints[lowerIdx].time.second) / (segment.trajectoryPoints[upperIdx].time.second - segment.trajectoryPoints[lowerIdx].time.second);
                        lerpVal = Mathf.Clamp(lerpVal, 0, 1);

                        Vector3 lowerPosition = segment.trajectoryPoints[lowerIdx].position;
                        Vector3 upperPosition = segment.trajectoryPoints[upperIdx].position;
                        Vector3 unitDir = (upperPosition - lowerPosition).normalized;
                        AlignLocalX_Absolute(transform, unitDir, Vector3.up * (-1f));
                        Vector3 aircraftPosition = Vector3.Lerp(lowerPosition, upperPosition, lerpVal);
                        transform.position = aircraftPosition;
                        break;
                    }
                }
            }
        }
    }

    public int SelectClosestIdx(float[] list, float val)
    {
        float lim = Mathf.Infinity;
        int selectedIdx = 0;
        for (int i = 0; i < list.Length; i++)
        {
            float len = Mathf.Abs(list[i] - val);
            if (len < lim)
            {
                selectedIdx = i;
                lim = len;
            }
        }
        return selectedIdx;
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
        Vector3 z = Vector3.Cross(up, x).normalized * (1);   // perpendicular to up & x
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

    public void OnEditableEnter(IEditable _editable)
    {
        if (_editable == (this as IEditable))
        {
            AircraftPopupUI.Instance.ShowPopup(this);
        }

    }

    public void OnEditableExit()
    {
        AircraftPopupUI.Instance.HidePopup();
    }

    public void SetActiveHighlightMeshRenderer(bool _isActive)
    {
        highlightMeshRenderer.gameObject.SetActive(_isActive);
    }
}

