using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RestrictedAreaFactory))]
[ExecuteAlways]
public class RestrictedAreaEditor : Editor
{

    public override void OnInspectorGUI()
    {


        DrawDefaultInspector();
        RestrictedAreaFactory myScript = (RestrictedAreaFactory)target;
        RestrictedAreaType areaType = (RestrictedAreaType)EditorGUILayout.EnumPopup("Area Type", RestrictedAreaType.SoundRestricted);        
        
        if (GUILayout.Button("Create"))
        {
            myScript.CreateRestrictedArea(areaType, Vector3.zero, Quaternion.identity);            
        }
        EditorGUILayout.EndHorizontal();


    }
}
