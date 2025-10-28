
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class Waypoint : MonoBehaviour
{
    [field: SerializeField] public TimeGame time { get; private set; }
    private Vector3 prev_position = Vector3.zero;

    // // // // // #if UNITY_EDITOR
    // // // // void Update()
    // // // // {
    // // // //     if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<Waypoint>() != null)
    // // // //     {
    // // // //         if (Selection.activeTransform.GetComponent<Waypoint>() == this)
    // // // //         {
    // // // //             GameEvents.Instance.WaypointPositionChanged(this, transform.position);
    // // // //         }
    // // // //     }
    // // // // }



#if UNITY_EDITOR
    void Update()
    {
        var go = UnityEditor.Selection.activeGameObject;
        if (go != null)
        {
            if (go.GetComponent<Waypoint>() != null && transform.position != prev_position)
            {
                GameEvents.Instance.WaypointPositionChanged(this, prev_position);
                prev_position = transform.position;
            }
        }
    }
#endif


    public void SetPosition(Vector3 _position)
    {
        if (transform.position == _position) return;
        Vector3 oldPosition = transform.position;
        GameEvents.Instance.WaypointPositionChanged(this, oldPosition);
        transform.position = _position;
    }
    public void SetTime(TimeGame _time)
    {
        TimeGame oldTime = time;
        GameEvents.Instance.WaypointTimeChanged(this, oldTime);
        time.SetTime(_time.second);
    }


}
