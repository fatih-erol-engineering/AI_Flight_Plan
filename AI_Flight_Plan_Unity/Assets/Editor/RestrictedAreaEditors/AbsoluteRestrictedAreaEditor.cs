using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AbsoluteRestrictedAreaFactory))]
//[ExecuteAlways]
public class AbsoluteRestrictedAreaEditor : Editor
{
    Vector3 position = Vector3.zero;
    float radius = 1.0f;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AbsoluteRestrictedAreaFactory myScript = (AbsoluteRestrictedAreaFactory)target;

        position = EditorGUILayout.Vector3Field("Position", position);
        radius = EditorGUILayout.FloatField("Radius", radius);

        if (GUILayout.Button("Spawn"))
        {
            myScript.Spawn(position, radius);
        }
        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();

        }
    }
}
