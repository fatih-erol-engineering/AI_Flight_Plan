using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Trajectory))]
public class TrajectoryEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        // Normal inspector çiz
        DrawDefaultInspector();

        Trajectory myScript = (Trajectory)target;

        // Buton ekle
        if (GUILayout.Button("Create Trajectory"))
        {
            myScript.CreateTrajectory();
        }
        if (GUILayout.Button("Delete Trajectory"))
        {
            myScript.DeleteTrajectory();
        }
    }
}
