using UnityEngine;
public class PositionSelectionSpawnerState : MonoBehaviour, ISpawnerState
{

    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] { KeyCode.Mouse0, KeyCode.Return };
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };
    [SerializeField] private LayerMask hitMask;

    private Camera mainCamera;


    public void OnEnter(Spawner spawner)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        spawner.SetObjectPreview(true);
        spawner.spawnerPositionWindowUI.ShowPopup();
        spawner.spawnerPositionWindowUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
    }

    public void OnExit(Spawner spawner, bool isCancelled = false)
    {
        if (isCancelled)
        {
            spawner.CancelSpawning();
        }
        spawner.spawnerPositionWindowUI.HidePopup();
    }


    public void Tick(Spawner spawner)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Ray hitPoint = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(hitPoint, out RaycastHit hoverHit, Mathf.Infinity, hitMask))
        {
            spawner.SetObjectPosition(hoverHit.point);
            spawner.spawnerPositionWindowUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
            spawner.spawnerPositionWindowUI.SetPositionFields(spawner.spawnedObject.transform);
        }
        foreach (var key in selectionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log("Applied Spawning");
                OnExit(spawner, false);
                spawner.SetCurrentState(spawner.propertySelectionSpawnerState);
            }
        }

        foreach (var key in cancelKeys)
        {
            if (Input.GetKeyDown(key))
            {
                OnExit(spawner, true);
                Debug.Log("Cancelled Spawning");
                spawner.SetCurrentState(spawner.idleSpawnerState);
            }
        }
    }

}
