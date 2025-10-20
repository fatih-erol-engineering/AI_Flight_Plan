using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BSplineDrawer))]
[ExecuteAlways]
public class BSplineDrawerEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BSplineDrawer myScript = (BSplineDrawer)target;

        if (GUILayout.Button("Create"))
        {
            myScript.Create();
        }
        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();
        }
    }
}
