using UnityEngine;

// [ExecuteAlways]
public class ControlPoint : MonoBehaviour
{
    public ControlPoint pairCP;
    public Waypoint waypoint;
    [SerializeField] private Vector3 closestPointToSpline;

    public void PairWith(ControlPoint pairCP_)
    {
        pairCP = pairCP_;
    }
    public void setPosition(Vector3 globalPosition)
    {
        transform.position = globalPosition;
        if (pairCP != null)
        {
            Vector3 relPosThis = transform.position - waypoint.transform.position;
            Vector3 relPosPair = pairCP.transform.position - waypoint.transform.position;
            pairCP.transform.position = waypoint.transform.position + relPosThis.normalized * (-1) * relPosPair.magnitude;
        }
    }
    public void SetClosestPointToSpline(Vector3 point)
    {
        closestPointToSpline = point;
    }
    public Vector3 GetClosestPointToSpline()
    {
        return closestPointToSpline;
    }
}
