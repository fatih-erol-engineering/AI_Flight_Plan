using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Scriptable Objects/Theme")]
public class Theme : ScriptableObject
{
    public Texture2D mouseDefault;    
    public Texture2D mouseHover;
    public Material highlightedMaterial;
    public Material hoverMaterial;
}
