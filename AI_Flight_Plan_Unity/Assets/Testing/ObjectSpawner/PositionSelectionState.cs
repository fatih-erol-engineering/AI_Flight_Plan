using UnityEngine;
public class PositionSelectionSpawnerState : MonoBehaviour, ISpawnerState
{

    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] { KeyCode.Mouse0, KeyCode.Return, KeyCode.KeypadEnter };
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Camera mainCamera;

    private bool foundBounds = false;
    private Bounds objectBounds;



    public void OnEnter(Spawner spawner)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        spawner.SetActivePreviewMode(true);
        spawner.spawnerPositionWindowUI.ShowPopup();
        spawner.spawnerPositionWindowUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
        foundBounds  = TryGetWorldBounds(spawner.spawnedObject, out objectBounds);

    }

    public void OnExit(Spawner spawner, bool isApplied = false)
    {
        if (!isApplied)
        {
            spawner.Cancel();
            spawner.SetCurrentState(spawner.idleState);
        }
        else
        {
            spawner.SetObjectPosition(spawner.spawnerPositionWindowUI.GetPositionFromUI());            
            spawner.SetCurrentState(spawner.propertySelectionState);
        }
        spawner.spawnerPositionWindowUI.HidePopup();
    }


    public void Tick(Spawner spawner)
    {        
        Ray hitPoint = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(hitPoint, out RaycastHit hoverHit, Mathf.Infinity, hitMask))
        {
            Vector3 targetPos = hoverHit.point;

            if (spawner.spawnedObject != null && foundBounds)
            {
                float bottomY = objectBounds.extents.y;
                targetPos.y = targetPos.y + bottomY;
            }


            spawner.SetObjectPosition(new Vector3(targetPos.x, targetPos.y, targetPos.z));
            spawner.spawnerPositionWindowUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
            spawner.spawnerPositionWindowUI.SetPositionToUI(spawner.spawnedObject.transform);
        }

        foreach (var key in selectionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                OnExit(spawner, true);    
                Debug.Log("Applied Spawning");
            }
        }

        foreach (var key in cancelKeys)
        {
            if (Input.GetKeyDown(key))
            {
                OnExit(spawner, false);
                Debug.Log("Cancelled Spawning");
            }
        }
    }

    // Objeye ait dünya uzayındaki toplam bounds’u bulur (öncelik Collider’larda)
    private bool TryGetWorldBounds(GameObject go, out Bounds bounds)
    {
        bounds = default;

        var cols = go.GetComponentsInChildren<Collider>(true);
        if (cols != null && cols.Length > 0)
        {
            bool init = false;
            foreach (var c in cols)
            {
                if (!c.enabled) continue;
                if (!init) { bounds = c.bounds; init = true; }
                else bounds.Encapsulate(c.bounds);
            }
            if (init) return true;
        }

        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends != null && rends.Length > 0)
        {
            bool init = false;
            foreach (var r in rends)
            {
                if (!r.enabled) continue;
                if (!init) { bounds = r.bounds; init = true; }
                else bounds.Encapsulate(r.bounds);
            }
            if (init) return true;
        }

        return false;
    }
}
