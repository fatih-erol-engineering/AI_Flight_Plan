using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConflictChecker))]
public class ConflictCheckerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConflictChecker myScript = (ConflictChecker)target;

        if (GUILayout.Button("Check Conflicts"))
        {
            myScript.CheckConflicts();
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
