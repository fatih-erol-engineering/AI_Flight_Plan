using UnityEngine;
using UnityEngine.UIElements;
public class PropertySelectionSpawnerState : MonoBehaviour, ISpawnerState
{

    [SerializeField] private Camera cam;
    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] { KeyCode.Mouse0, KeyCode.Return };
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };

    void AssignData()
    {
        if (cam == null) cam = Camera.current;
    }


    public void OnEnter(Spawner spawner)
    {
        AssignData();
        spawner.spawnerPropertyPopupUI.ShowPopup();
        spawner.spawnerPropertyPopupUI.ShowPopup();
        spawner.spawnerPropertyPopupUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
    }

    public void OnExit(Spawner spawner, bool isCancelled)
    {
        if (isCancelled)
        {
            spawner.CancelSpawning();
        }
        else
        {
            spawner.SetObjectPreview(false);
        }

    }

    public void Tick(Spawner spawner)
    {
        spawner.spawnerPropertyPopupUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
        spawner.SetObjectPosition(spawner.spawnerPropertyPopupUI.GetPositionFields());

        foreach (var key in selectionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log("Apllied Spawning");
                OnExit(spawner, false);
                spawner.SetCurrentState(spawner.idleSpawnerState);
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
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }



}