using UnityEngine;

public class SelectableMonoBehaviour : MonoBehaviour
{
    public Material originalMaterial;
    public void Init(Material mat)
    {
        originalMaterial = mat;
    }
}
