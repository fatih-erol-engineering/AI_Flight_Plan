using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{     
    private Material prevSelectedMat;
    private Material prev_HoveredMat;
    private GameObject selectedObj;
    private GameObject prevSelectedObj;
    private GameObject hoveredObj;
    private GameObject prev_HoveredObj;
    
    public Theme theme;
    private Texture2D currentCursor;


    public void UpdateCycle()
    {
        float maxDistance = Camera.main.farClipPlane;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //hoveredObj = new GameObject();
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            hoveredObj = hit.collider.gameObject;
        }

        if (prev_HoveredObj == hoveredObj)
        {
            // DO NOTHING
        }
        else
        {
            
            SelectableMonoBehaviour selectable = hoveredObj.GetComponent<SelectableMonoBehaviour>();
            if (selectable != null)
            {
                hoveredObj.GetComponent<Renderer>().material = theme.hoverMaterial;
                if (prev_HoveredObj != null) 
                {
                    selectable = prev_HoveredObj.GetComponent<SelectableMonoBehaviour>();
                    if (selectable != null)
                    {
                        prev_HoveredObj.GetComponent<Renderer>().material = selectable.originalMaterial;
                    }                
                }
                else
                {
                    prev_HoveredObj = hoveredObj;
                }
            }
        }
    }



        









        ////GameObject hoverObj = null;
        ////Renderer rend = new Renderer();

        ////// Hover 
        ////if (Physics.Raycast(ray, out hit, maxDistance))
        ////{
        ////    hoverObj = hit.collider.gameObject;

        ////    bool hoverIsEmpty = (hoverObj.tag == "CesiumMap") || (hoverObj.tag == "Untagged") || (hoverObj == null);
        ////    bool objectHasHovered = !hoverIsEmpty;
        ////    hoveredObj = hoverObj;
        ////    rend = hoveredObj.GetComponent<Renderer>();

        ////    if (objectHasHovered) 
        ////    {
        ////        if (rend != null)
        ////        {
        ////            prevHoveredMat = rend.material;
        ////            prevHoveredObj = hoveredObj;
        ////            rend.material = theme.hoverMaterial;                    
        ////        }
        ////    }
        ////    else
        ////    {
        ////        if ((prevHoveredObj != null) && (prevHoveredMat != null)) 
        ////        {               
        ////            prevHoveredObj.GetComponent<Renderer>().material = prevHoveredMat; 
        ////        }
        ////    }



        ////}







        //if (Physics.Raycast(ray, out hit, maxDistance))
        //{
        //    hitObj = hit.collider.gameObject;
        //    if (hitObj != selectedObj)
        //    {
        //        if ((hitObj.tag != "CesiumMap") && (hitObj.tag != "Untagged"))
        //        {
        //            selectedObj = hitObj;
        //            Renderer rend = selectedObj.GetComponent<Renderer>();
        //            if (rend != null)
        //            {
        //                originalMat = rend.material;
        //                rend.material = theme.hoverMaterial;
        //            }
        //        }
        //        else
        //        {
        //            selectedObj.GetComponent<Renderer>().material = originalMat;
        //        }
        //    }                        
        //}
        //else
        //{
        //    selectedObj.GetComponent<Renderer>().material = originalMat;
        //}

        //// Sol mouse tusuna tiklanmasi
        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (hitObj != null) { 
        //        if ((hitObj.tag != "CesiumMap") && (hitObj.tag != "Untagged"))
        //        {                                
        //            if (selectedObj != null)
        //            {
        //                selectedObj.GetComponent<Renderer>().material = originalMat;
        //            }


        //            selectedObj = hitObj;
        //            Renderer rend = selectedObj.GetComponent<Renderer>();

        //            if (rend != null)
        //            {
        //                originalMat = rend.material;
        //                rend.material = theme.highlightedMaterial; 
        //            }
        //        }
        //    }
        //}
    private void HighlightObject(GameObject gameObject)
    {

    }
}
