using Unity.MLAgents;
using UnityEngine;

public class SelectableMonoBehaviour : MonoBehaviour
{
    private Vector3 originalScale;
    private float selectScaleGain = 1f;
    private float selectEmisionStrength = 1.05f;
    private float hoverScaleGain = 1.1f;
    private float hoverEmisionStrength = 1.05f;
    public bool isSelected;

    //private void Awake()
    //{
    //    originalScale = transform.localScale;
    //}
    //private void OnMouseEnter()
    //{
    //    if (!isSelected) 
    //    { 
    //        Hover();
    //    }
    //}
    //private void OnMouseExit()
    //{
    //    if (!isSelected)
    //    {
    //        De_Hover();
    //    }        
    //}
    //public void Select()
    //{        
    //    transform.localScale = transform.localScale * selectScaleGain;
    //    foreach (Transform child in transform)
    //    {
    //        // Apply Reverse Scale to Childs
    //        child.localScale = new Vector3(
    //            child.localScale.x / selectScaleGain,
    //            child.localScale.y / selectScaleGain,
    //            child.localScale.z / selectScaleGain
    //        );
    //    }

    //    // Highlight Material
    //    Material mat = transform.GetComponent<Renderer>().material;
    //    if (mat != null)
    //    {
    //        mat.EnableKeyword("_EMISSION");
    //        mat.SetColor("_EmissionColor", mat.color * selectEmisionStrength);
    //    }
    //    isSelected = true;
    //}
    //public void De_Select()
    //{
    //    Vector3 oldScale = transform.localScale;
    //    transform.localScale = originalScale;
    //    foreach (Transform child in transform)
    //    {
    //        // Apply Reverse Scale to Childs
    //        child.localScale = new Vector3(
    //            child.localScale.x * oldScale.x / originalScale.x,
    //            child.localScale.y * oldScale.y / originalScale.y,
    //            child.localScale.z * oldScale.z / originalScale.z
    //        );
    //    }
    //    Material mat = transform.GetComponent<Renderer>().material;
    //    if (mat != null)
    //    {
    //        mat.DisableKeyword("_EMISSION");
    //    }
    //    isSelected = false;
    //}

    //private void Hover()
    //{        
    //    transform.localScale = transform.localScale * hoverScaleGain;
    //    foreach (Transform child in transform)
    //    {
    //        // Apply Reverse Scale to Childs
    //        child.localScale = new Vector3(
    //            child.localScale.x / hoverScaleGain,
    //            child.localScale.y / hoverScaleGain,
    //            child.localScale.z / hoverScaleGain
    //        );
    //    }

    //    // Highlight Material
    //    Material mat = transform.GetComponent<Renderer>().material;
    //    if (mat != null)
    //    {
    //        mat.EnableKeyword("_EMISSION");            
    //        mat.SetColor("_EmissionColor", mat.color * hoverEmisionStrength);
    //    }
    //}
    //private void De_Hover()
    //{
    //    Vector3 oldScale = transform.localScale;
    //    transform.localScale = originalScale;
    //    foreach (Transform child in transform)
    //    {
    //        // Apply Reverse Scale to Childs
    //        child.localScale = new Vector3(
    //            child.localScale.x * oldScale.x / originalScale.x,
    //            child.localScale.y * oldScale.y / originalScale.y,
    //            child.localScale.z * oldScale.z / originalScale.z
    //        );
    //    }
    //    Material mat = transform.GetComponent<Renderer>().material;
    //    if (mat != null)
    //    {
    //        mat.DisableKeyword("_EMISSION");            
    //    }
    //}

}
