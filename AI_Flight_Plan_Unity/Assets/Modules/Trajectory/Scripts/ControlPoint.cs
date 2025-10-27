using UnityEngine;

// [ExecuteAlways]
public class ControlPoint : MonoBehaviour
{        
    [SerializeField] private Vector3 closestPointToSpline;
    
    public void SetPosition(Vector3 _position)
    {
        if (_position == transform.position) return;
        Vector3 oldposition = transform.position;
        GameEvents.instance.ControlPointPositionChanged(this, oldposition);
        transform.position = _position;
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
