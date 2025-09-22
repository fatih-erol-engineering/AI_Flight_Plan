using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrajectoryDrawer))]
public class TrajectoryDrawerEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        // Normal inspector çiz
        DrawDefaultInspector();

        TrajectoryDrawer myScript = (TrajectoryDrawer)target;

        // Buton ekle
        if (GUILayout.Button("Create Trajectory"))
        {
            myScript.CreateTrajectory();
        }
    }
}
