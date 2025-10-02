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
        base.Init(GetComponent<Renderer>().material);
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
