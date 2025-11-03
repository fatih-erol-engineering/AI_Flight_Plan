using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConflictCheckerForEditor))]
[ExecuteAlways]
public class ConflictCheckerForEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConflictCheckerForEditor myScript = (ConflictCheckerForEditor)target;

        if (GUILayout.Button("Check Conflicts"))
        {
            myScript.CheckConflicts();
        }
        if (GUILayout.Button("Solve Conflicts"))
        {
            myScript.SolveConflicts();
        }
    }
}
