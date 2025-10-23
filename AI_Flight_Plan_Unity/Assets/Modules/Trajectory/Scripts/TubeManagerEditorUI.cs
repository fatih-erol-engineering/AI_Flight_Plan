using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TubeManager))]
[ExecuteAlways]
public class TubeManagerEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TubeManager myScript = (TubeManager)target;

        if (GUILayout.Button("Assign Data"))
        {
            myScript.AssignData();
        }
        if (GUILayout.Button("Update"))
        {
            myScript.UpdateTubeImmidiately();
        }
        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
    }
}
