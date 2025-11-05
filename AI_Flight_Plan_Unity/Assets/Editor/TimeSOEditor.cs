using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TimeSO))]
[ExecuteAlways]
public class TimeSOEditor : Editor
{
    // local editor field (keeps value while editor session active)
    private Vector3 leftVector = Vector3.zero;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TimeSO myScript = (TimeSO)target;

        if (GUILayout.Button("Send Events"))
        {
            myScript.SendEvents();
        }        
    }
}
