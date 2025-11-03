using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
public class PropertySelectionSpawnerState : MonoBehaviour, ISpawnerState
{

    [SerializeField] private Camera cam;
    [SerializeField] private KeyCode[] selectionKeys = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter };
    [SerializeField] private KeyCode[] cancelKeys = new KeyCode[] { KeyCode.Escape };
    private bool isApplyCoroutineRunning = false;
    private bool isSetPositionCoroutineRunning = false;
    private Vector3 prev_positionFromUI;
    private Vector3 positionFromUI;

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
        isApplyCoroutineRunning = false;
        prev_positionFromUI = spawner.spawnerPropertyPopupUI.GetPositionFromUI();
    }

    public void OnExit(Spawner spawner, bool isApplied)
    {
        spawner.spawnerPositionWindowUI.HidePopup();        
        if (!isApplied)
        {
            spawner.Cancel();
        }
        else
        {
            spawner.Apply();
        }
    }

    public void Tick(Spawner spawner)
    {
        spawner.spawnerPropertyPopupUI.ShowPopupOnTransform(spawner.spawnedObject.transform);
        positionFromUI = spawner.spawnerPropertyPopupUI.GetPositionFromUI();
        if (!isApplyCoroutineRunning && (prev_positionFromUI != positionFromUI))
        {
            StartCoroutine(SetPositionCoroutine(spawner, positionFromUI, 0.2f));
        }


        foreach (var key in selectionKeys)
        {
            if (Input.GetKeyDown(key))
            {
                StartCoroutine(ApplyCoroutine(spawner, spawner.spawnerPropertyPopupUI.GetPositionFromUI(), 0.2f));
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
        prev_positionFromUI = spawner.spawnerPropertyPopupUI.GetPositionFromUI();
    }
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }


    IEnumerator SetPositionCoroutine(Spawner _spawner, Vector3 _position, float _duration)
    {
        Vector3 startPosition = _spawner.spawnedObject.transform.position;
        float elapsed = 0f;
        Debug.Log("Corutine Start");
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _spawner.SetObjectPosition(Vector3.Lerp(startPosition, _position, t));
            isSetPositionCoroutineRunning = true;
            yield return null;
        }
        _spawner.SetObjectPosition(_position);
        Debug.Log("Corutine End");
        isSetPositionCoroutineRunning = false;
    }
    IEnumerator ApplyCoroutine(Spawner _spawner, Vector3 _position, float _duration)
    {
        Vector3 startPosition = _spawner.spawnedObject.transform.position;
        float elapsed = 0f;
        if (_position != startPosition)
        {            
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                _spawner.SetObjectPosition(Vector3.Lerp(startPosition, _position, t));
                isApplyCoroutineRunning = true;
                yield return null;
            }
        }
        isApplyCoroutineRunning = false;
        OnExit(_spawner, true);
    }

}