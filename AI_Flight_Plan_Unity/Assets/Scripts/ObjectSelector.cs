using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{             
    private GameObject selectedObj;
    private GameObject prev_SelectedObj;
    private float hightLightEmisionStrength = 1.2f;


    public void UpdateCycle()
    {
        SelectableMonoBehaviour sMB = new SelectableMonoBehaviour();
        if (Input.GetMouseButtonDown(0))
        {

            float maxDistance = Camera.main.farClipPlane;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //hoveredObj = new GameObject();
            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                GameObject hitObj = hit.collider.gameObject;
                sMB = hitObj.GetComponent<SelectableMonoBehaviour>();
                if (sMB != null)
                {
                    sMB.Select();
                    selectedObj = hitObj;
                }

                if (prev_SelectedObj != null)
                {                
                    sMB = prev_SelectedObj.GetComponent<SelectableMonoBehaviour>();
                    if (sMB != null)
                    {
                        sMB.De_Select();
                    }                                    
                }

                prev_SelectedObj = selectedObj;
            }
        }        
    }



}
