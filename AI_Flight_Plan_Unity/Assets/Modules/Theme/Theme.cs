using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Theme", menuName = "Scriptable Objects/Theme")]
public class Theme : ScriptableObject
{

    public Texture2D mouseDefault;
    public Texture2D mouseDrag;
    public GameObject waypointPrefab;
    public GameObject controlPointPrefab;
    public GameObject trajectoryPrefab;
    public GameObject BSplineSegmentPrefab;
    [Header("Menu Icons")]
    public Texture2D addIcon;
    public Texture2D editIcon;
    public Texture2D fixedWingIcon;
    public Texture2D rotorIcon;
    public Texture2D listenIcon;
    public Texture2D playIcon;
    public Texture2D stopIcon;
    public Texture2D pauseIcon;

    [Header("Theme Materials")]
    public Material Hover;
    public Material Select;
    public Material PreCreate;
    [Header("Trajectory")]    
    public Color startColor;
    public Color endColor;    

}
