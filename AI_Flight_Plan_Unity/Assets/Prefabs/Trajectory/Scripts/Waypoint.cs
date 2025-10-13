using UnityEngine;

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
        Vector3 deltaPos = globalPosition - oldPos;;
        if(controlPoints != null) 
        {            
            foreach (ControlPoint controlPoint in controlPoints)
            {
                if (controlPoint != null)
                {
                    controlPoint.transform.position += deltaPos;
                }
            }
        }
        oldPos = globalPosition;
    }
    public void setPosition(Vector3 globalPosition, float time_s)
    {
        transform.position = globalPosition;
        Vector3 deltaPos = globalPosition - oldPos;
        if (controlPoints != null)
        {
            foreach (ControlPoint controlPoint in controlPoints)
            {
                if (controlPoint != null)
                {
                    controlPoint.transform.position += deltaPos;
                }
            }
        }
        oldPos = globalPosition;
        time.second = time_s;
    }

}


public enum WaypointType
{
    Open,
    Close
}
