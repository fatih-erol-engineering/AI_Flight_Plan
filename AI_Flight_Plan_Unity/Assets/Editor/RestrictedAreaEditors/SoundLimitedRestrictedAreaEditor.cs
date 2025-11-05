using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundLimitedRestrictedAreaFactory))]
//[ExecuteAlways]
public class SoundLimitedRestrictedAreaEditor : Editor
{
    Vector3 position = Vector3.zero;
    float radius = 1.0f;
    float soundLimit_dBa = 50f;
    Color minSoundColor;
    Color maxSoundColor;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SoundLimitedRestrictedAreaFactory myScript = (SoundLimitedRestrictedAreaFactory)target;

        position = EditorGUILayout.Vector3Field("Position", position);
        radius = EditorGUILayout.FloatField("Radius", radius);
        soundLimit_dBa = EditorGUILayout.FloatField("Sound Limit (dBA)", soundLimit_dBa);

        if (GUILayout.Button("Spawn"))
        {
            myScript.Spawn(position, radius, soundLimit_dBa);
        }
        if (GUILayout.Button("Clear"))
        {
            myScript.Clear();

        }
    }
}
