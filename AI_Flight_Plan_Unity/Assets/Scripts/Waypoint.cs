using UnityEngine;
using UnityEngine.U2D;


[ExecuteAlways]
public class Waypoint : SelectableMonoBehaviour
{
    public TimeGame time;
    public WaypointType type = WaypointType.Open;
    public ControlPoint[] controlPoints;    
    private Vector3 oldPos;
    void Start()
    {
        oldPos = transform.position;                   
    }
    public void setPosition(Vector3 globalPosition)
    {
        transform.position = globalPosition;  
        Vector3 deltaPos = globalPosition - oldPos;
        foreach (ControlPoint controlPoint in controlPoints)
        {
            controlPoint.transform.position += deltaPos;
        }        

        oldPos = globalPosition;
    }
}


public enum WaypointType
{
    Open,
    Close
}
