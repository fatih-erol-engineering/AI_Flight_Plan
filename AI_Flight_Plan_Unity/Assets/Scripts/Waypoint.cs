using UnityEngine;
using UnityEngine.U2D;

public class Waypoint : MonoBehaviour
{    
    public WaypointType type = WaypointType.Open;
    public ControlPoint[] controlPoints;
    private Vector3 oldPos;
    void Awake()
    {
        oldPos = transform.position;
    }
    public void setPosition(Vector3 position)
    {
        transform.position = position;  
        Vector3 deltaPos = position - oldPos;
        foreach (ControlPoint controlPoint in controlPoints)
        {
            controlPoint.transform.position += deltaPos;
        }        

        oldPos = position;
    }
}


public enum WaypointType
{
    Open,
    Close
}
