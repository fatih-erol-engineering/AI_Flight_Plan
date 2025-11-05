using System;
using UnityEngine;

[ExecuteAlways]
public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;
    private bool isInitialized = false;

    void OnEnable()
    {
        if (isInitialized) return;
        if (!Application.isPlaying)
        {
            AssignData();
        }
    }
    void OnDisable()
    {
        isInitialized = false;
    }

    private void Awake()
    {
        AssignData();
    }
    private void AssignData()
    {
        isInitialized = true;
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("A ThemeManager already exists in the scene. Removing duplicate.", this);
#if UNITY_EDITOR
            // Safe to remove component immediately in editor
            DestroyImmediate(this);
#else
            Destroy(this);
#endif
            return;
        }
    }

    // Time Manager Events
    public event Action<float> OnTimeChanged;
    public void TimeChanged(float _val)
    {
        OnTimeChanged?.Invoke(_val);
    }

    public event Action<TimeState> OnTimeStateChanged;
    public void TimeStateChanged(TimeState _val)
    {
        OnTimeStateChanged?.Invoke(_val);
    }

    public event Action<float, float> OnStartEndTimeChanged;
    public void StartEndTimeChanged(float _startTime, float _endTime)
    {
        OnStartEndTimeChanged?.Invoke(_startTime, _endTime);
    }


    public event Action<float> OnTimeScaleChanged;
    public void TimeScaleChanged(float _val)
    {
        OnTimeScaleChanged?.Invoke(_val);
    }


    // Waypoint Events
    public event Action<Waypoint, Vector3> OnWaypointPositionChanged;
    public void WaypointPositionChanged(Waypoint waypoint, Vector3 oldPosition)
    {
        OnWaypointPositionChanged?.Invoke(waypoint, oldPosition);
    }


    public event Action<Waypoint, TimeGame> OnWaypointTimeChanged;
    public void WaypointTimeChanged(Waypoint waypoint, TimeGame oldTime)
    {
        OnWaypointTimeChanged?.Invoke(waypoint, oldTime);
    }

    public event Action<Waypoint> OnWaypointSpawned;
    public void WaypointSpawned(Waypoint waypoint)
    {
        OnWaypointSpawned?.Invoke(waypoint);
    }


    // Control Point Events
    public event Action<ControlPoint, Vector3> OnControlPointPositionChanged;
    public void ControlPointPositionChanged(ControlPoint controlPoint, Vector3 oldPosition)
    {
        OnControlPointPositionChanged?.Invoke(controlPoint, oldPosition);
    }


    // Trajectory Events
    public event Action<TrajectoryDrawer> OnTrajectoryCreated;
    public void TrajectoryCreated(TrajectoryDrawer trajectoryDrawer)
    {
        OnTrajectoryCreated?.Invoke(trajectoryDrawer);
    }
    public event Action<TrajectoryDrawer> OnTrajectoryTimeChanged;
    public void TrajectoryTimeChanged(TrajectoryDrawer trajectoryDrawer)
    {
        OnTrajectoryTimeChanged?.Invoke(trajectoryDrawer);
    }

    // Aircraft Events
    public event Action<Aircraft> OnAircraftSpawned;
    public void AircraftSpawned(Aircraft aircraft)
    {
        OnAircraftSpawned?.Invoke(aircraft);
    }

    public event Action<Aircraft> OnAircraftDeleted;
    public void AircraftDeleted(Aircraft aircraft)
    {
        OnAircraftDeleted?.Invoke(aircraft);
    }


    // Spline Events
    public event Action<BSplineDrawer> OnSplineChanged;
    public void SplineChanged(BSplineDrawer splineDrawer)
    {
        OnSplineChanged?.Invoke(splineDrawer);
    }



    // Editable Events
    public event Action<IEditable> OnEditableEnter;
    public void EditableEnter(IEditable _editable)
    {
        OnEditableEnter?.Invoke(_editable);
    }

    public event Action OnEditableExit;
    public void EditableExit()
    {
        OnEditableExit?.Invoke();
    }

    // Camera Events
    public event Action<Camera> OnCameraStateChanged;
    public void CameraStateChanged(Camera camera)
    {
        OnCameraStateChanged?.Invoke(camera);
    }


    // Menu UI Events
    public event Action<string> OnSelectedAircraftToSpawnChangedUI;
    public void ChangeAircraftPrefabWithUI(string _val)
    {
        OnSelectedAircraftToSpawnChangedUI?.Invoke(_val);
    }

    // Restricted Area Events
    public event Action<AbsoluteRestrictedAreaFactory> OnAbsoluteRestrictedAreaCreated;
    public void AbsoluteRestrictedAreaCreated(AbsoluteRestrictedAreaFactory _val)
    {
        OnAbsoluteRestrictedAreaCreated?.Invoke(_val);
    }

}
