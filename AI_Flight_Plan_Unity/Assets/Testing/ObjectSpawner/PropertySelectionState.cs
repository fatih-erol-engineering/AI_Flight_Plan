using UnityEngine;
using UnityEngine.UIElements;
public class PropertySelectionSpawnerState : MonoBehaviour, ISpawnerState
{

    [SerializeField] private Camera cam;
    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] {KeyCode.Return , KeyCode.KeypadEnter};
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };

    void AssignData()
    {
        if (cam == null) cam = Camera.current;
    }


    public void OnEnter(Spawner spawner)
    {
        AssignData();
        spawner.spawnerPropertyPopupUI.ShowPopup();

        spawner.spawnerPropertyPopupUI.SetPositionToUI(spawner.spawnedObject.transform);
        spawner.spawnerPropertyPopupUI.createBtn.clicked -= () =>
        {
            OnExit(spawner, true);                        
        };
        spawner.spawnerPropertyPopupUI.createBtn.clicked += () =>
        {
            OnExit(spawner, true);
        };                
    }

    public void OnExit(Spawner spawner, bool isApplied)
    {
        if (!isApplied)
        {
            spawner.Cancel();            
        }
        else
        {            
            spawner.Apply();            
        }
            spawner.spawnerPositionWindowUI.HidePopup();
            spawner.SetCurrentState(spawner.idleState);
    }

    public void Tick(Spawner spawner)
    {
        spawner.spawnerPropertyPopupUI.ShowPopupOnTransform(spawner.spawnedObject.transform);                
        spawner.SetObjectPositionWithAnim(spawner.spawnerPropertyPopupUI.GetPositionFromUI(), 0.2f);
        

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
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }



}