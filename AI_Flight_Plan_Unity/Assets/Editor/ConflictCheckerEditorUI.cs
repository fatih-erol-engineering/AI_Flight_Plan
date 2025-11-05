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
        }
        if (GUILayout.Button("Solve Conflicts"))
        {
            myScript.SolveConflictsWithRuleBased();
        }
        if (GUILayout.Button("Solve with AI"))
        {
            myScript.SolveConflictsWithAI();
        }
        if (GUILayout.Button("Clear Conflicts"))
        {
            myScript.ClearConflicts();
        }
    }
}
