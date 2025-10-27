using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class ControlPoint : MonoBehaviour
{
    [SerializeField] private Vector3 closestPointToSpline;
    private Vector3 prev_position = Vector3.zero;

#if UNITY_EDITOR
    void Update()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            var go = UnityEditor.Selection.activeGameObject;
            if (go != null)
            {
                if (go.GetComponent<ControlPoint>() != null && transform.position != prev_position)
                {
                    GameEvents.instance.ControlPointPositionChanged(this, prev_position);
                    prev_position = transform.position;
                }
            }
        }
    }
#endif
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
