using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Theme", menuName = "Theme")]
public class Theme : ScriptableObject
{

    public Texture2D mouseDefault;
    public Texture2D mouseDrag;
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
    public Material Preview;
    [Header("Trajectory")]
    public Color startColor;

    public Color endColor;
    public int linePointNumber;

    [Header("Tube Properties")]

    [ColorUsage(true, true)] public Color tubeEdgeColor_nonCollided;
    public Color tubeSurfaceColor_nonCollided;
    [ColorUsage(true, true)] public Color tubeEdgeColor_collided;
    public Color tubeSurfaceColor_collided;
    public float tubeEdgeSize = 0.2f;


}
