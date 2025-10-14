using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Trajectory))]
public class TrajectoryEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Trajectory myScript = (Trajectory)target;

        if (GUILayout.Button("Create Trajectory"))
        {
            // myScript.Create();
        }
        if (GUILayout.Button("Clear Trajectory"))
        {
            myScript.Clear();
        }
    }
}
