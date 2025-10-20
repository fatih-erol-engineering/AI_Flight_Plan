using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CollisionCheckerTrajectory))]
public class CollisionCheckerTrajectoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CollisionCheckerTrajectory myScript = (CollisionCheckerTrajectory)target;

        if (GUILayout.Button("Check Collisions"))
        {
            myScript.CheckCollisions();
        }
        if (GUILayout.Button("Clear Collisions"))
        {
            myScript.ClearCollisions();
        }
    }
}
