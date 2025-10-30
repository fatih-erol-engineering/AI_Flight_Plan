using System;
using UnityEngine;


// [DefaultExecutionOrder(-1000)] // 
public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;

    private void OnValidate()
    {
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
    private void Awake()
    {
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

}
