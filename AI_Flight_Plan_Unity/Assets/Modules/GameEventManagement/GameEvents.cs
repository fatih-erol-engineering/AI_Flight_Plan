using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    private void OnValidate()
    {
        instance = this;
    }
    private void Awake()
    {
        instance = this;
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


    // Control Point Events
    public event Action<ControlPoint, Vector3> OnControlPointPositionChanged;
    public void ControlPointPositionChanged(ControlPoint controlPoint, Vector3 oldPosition)
    {
        OnControlPointPositionChanged?.Invoke(controlPoint, oldPosition);
    }


}
