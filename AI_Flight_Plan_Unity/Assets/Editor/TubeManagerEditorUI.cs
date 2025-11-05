using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TubeManager))]
//[ExecuteAlways]
public class TubeManagerEditorUI : Editor
{
    // local editor field (keeps value while editor session active)
    private Vector3 leftVector = Vector3.zero;

    public override void OnInspectorGUI()
    {


        DrawDefaultInspector();
        TubeManager myScript = (TubeManager)target;

        // Horizontal row: Vector3 field on left, action button on right
        EditorGUILayout.BeginHorizontal();
        leftVector = EditorGUILayout.Vector3Field(GUIContent.none, leftVector, GUILayout.Width(220));
        if (GUILayout.Button("Check Pos Inside ", GUILayout.Height(20)))
        {
            myScript.CheckPositionInsideOrNot(leftVector);
        }
        EditorGUILayout.EndHorizontal();


    }
}
