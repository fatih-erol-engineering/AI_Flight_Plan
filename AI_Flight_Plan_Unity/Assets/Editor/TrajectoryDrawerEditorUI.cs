using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrajectoryDrawer))]
[ExecuteAlways]
public class TrajectoryDrawerEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrajectoryDrawer myScript = (TrajectoryDrawer)target;

        if (GUILayout.Button("Assign Data"))
        {
            myScript.AssignData();
        }
        if (GUILayout.Button("Create"))
        {
            myScript.Create();
        }
        if (GUILayout.Button("Update"))
        {
            myScript.Tick();
        }
        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
    }
}
