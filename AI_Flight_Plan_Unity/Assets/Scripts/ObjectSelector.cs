using UnityEngine;

public class ObjectSelector : MonoBehaviour
{     
    private Material originalMat;   
    private GameObject selectedObj;
    public Theme theme;
    private Texture2D currentCursor;


    public void UpdateCycle()
    {
        float maxDistance = Camera.main.farClipPlane;


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject hitObj = null;

        // Cursor Tipi Degismesi


        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            hitObj = hit.collider.gameObject;
            if ((hitObj.tag != "CesiumMap") && (hitObj.tag != "Untagged"))
            {
                SetCursorSafe(theme.mouseHover);
            }
            else
            {
                SetCursorSafe(theme.mouseDefault);
            }
        }
        else
        {
            SetCursorSafe(theme.mouseDefault);
        }

        // Sol mouse tusuna tiklanmasi
        if (Input.GetMouseButtonDown(0))
        {
            if (hitObj != null) { 
                if ((hitObj.tag != "CesiumMap") && (hitObj.tag != "Untagged"))
                {                                
                    if (selectedObj != null)
                    {
                        selectedObj.GetComponent<Renderer>().material = originalMat;
                    }

                
                    selectedObj = hitObj;
                    Renderer rend = selectedObj.GetComponent<Renderer>();

                    if (rend != null)
                    {
                        originalMat = rend.material;
                        rend.material = theme.highlightedMaterial; 
                    }
                }
            }
        }

    }
    private void SetCursorSafe(Texture2D tex)
    {
        if (currentCursor == tex) return; // aynıysa değiştirme
        currentCursor = tex;
        Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
    }
}
