using UnityEngine;
using UnityEngine.UIElements;
public class PositionSelectionSpawnerState : MonoBehaviour, ISpawnerState
{
    [SerializeField] private Material previewMat;
    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] { KeyCode.Mouse0, KeyCode.Return };
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };
    [SerializeField] private LayerMask hitMask;


    private Material originalMat;
    private Camera mainCamera;


    public void OnEnter(Spawner spawner)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        originalMat = spawner.spawnedObject.GetComponent<MeshRenderer>().material;
        spawner.spawnedObject.GetComponent<MeshRenderer>().material = previewMat;
    }

    public void OnExit(Spawner spawner, bool isCancelled = false)
    {
        if (isCancelled)
        {
            GameObject.Destroy(spawner.spawnedObject);
        }
        else
        {
            spawner.spawnedObject.GetComponent<MeshRenderer>().material = originalMat;
        }

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
            spawner.spawnedObject.transform.position = hoverHit.point;
        }
        foreach (var key in selectionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log("Apllied Spawning");
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
