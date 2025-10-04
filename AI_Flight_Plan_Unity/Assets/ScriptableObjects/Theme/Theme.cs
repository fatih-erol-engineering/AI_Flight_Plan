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
    [Header("Menu Icons")]
    public Texture2D addIcon;
    public Texture2D editIcon;
    public Texture2D fixedWingIcon;
    public Texture2D rotorIcon;
    public Texture2D listenIcon;
    public Texture2D playIcon;
    public Texture2D stopIcon;
    public Texture2D pauseIcon;

    [Header("Theme Colors")]
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public Color color5;
}
