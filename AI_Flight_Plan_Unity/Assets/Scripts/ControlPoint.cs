using UnityEngine;

public class ControlPoint : SelectableMonoBehaviour
{
    public ControlPoint pairCP;
    public Waypoint waypoint;
    
    public void PairWith(ControlPoint pairCP_) 
    {
        pairCP = pairCP_;
    }
    public void setPosition(Vector3 globalPosition)
    {        
        transform.position = globalPosition;
        if (pairCP !=null)
        {
            Vector3 relPosThis = transform.position - waypoint.transform.position;
            Vector3 relPosPair = pairCP.transform.position - waypoint.transform.position;
            pairCP.transform.position = waypoint.transform.position + relPosThis.normalized * (-1) * relPosPair.magnitude;
        }
        
    }
}
