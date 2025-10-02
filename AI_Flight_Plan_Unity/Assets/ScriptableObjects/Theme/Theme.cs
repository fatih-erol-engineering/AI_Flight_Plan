using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Scriptable Objects/Theme")]
public class Theme : ScriptableObject
{
    public Texture2D mouseDefault;    
    public Texture2D mouseDrag;
    public GameObject waypointPrefab;
    public GameObject controlPointPrefab;
    public GameObject trajectoryPrefab;
    public GameObject BSplineSegmentPrefab;
}
