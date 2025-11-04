using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConflictChecker))]
public class ConflictCheckerEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConflictChecker myScript = (ConflictChecker)target;

        if (GUILayout.Button("Check Conflicts"))
        {
            myScript.CheckConflicts();
            myScript.CheckRestrictedAreaConflicts();
        }
        if (GUILayout.Button("Solve Conflicts"))
        {
            myScript.SolveConflicts();
        }
        if (GUILayout.Button("Clear Conflicts"))
        {
            myScript.ClearConflicts();
        }
    }
}
