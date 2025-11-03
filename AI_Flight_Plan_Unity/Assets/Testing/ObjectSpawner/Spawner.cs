using System.Collections;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


public class Spawner : MonoBehaviour
{
    #region Prefabs
    [Header("Prefabs")]
    [SerializeField] private GameObject spawnedObjectPrefab;
    [SerializeField] private GameObject neonWavePrefab;
    [SerializeField] public GameObject idleStatePrefab;
    [SerializeField] public GameObject positionSelectionStatePrefab;
    [SerializeField] public GameObject propertySelectionStatePrefab;
    #endregion

    #region Prefab Instances
    [HideInInspector] public GameObject spawnedObject;
    [HideInInspector] public NeonWave neonWave;
    [HideInInspector] public IdleSpawnerState idleState;
    [HideInInspector] public PositionSelectionSpawnerState positionSelectionState;
    [HideInInspector] public PropertySelectionSpawnerState propertySelectionState;
    [HideInInspector] public SpawnerPositionWindowUI spawnerPositionWindowUI;
    [HideInInspector] public SpawnerPropertyPopupUI spawnerPropertyPopupUI;
    #endregion

    #region State Instances
    [SerializeField] public ISpawnerState currentState
    {
        get => _currentState;
        private set
        {
            SetCurrentState(value);
        }
    }

    private ISpawnerState _currentState;


    #endregion

    #region Other Variables
    [Header("Other Variables")]
    [SerializeField] public KeyCode selectionKey = KeyCode.Return;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Material originalMat;
    [SerializeField] private Material previewMat;
    #endregion



    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        spawnedObject = Instantiate(spawnedObjectPrefab, transform);        

        neonWave = Instantiate(neonWavePrefab, transform).GetComponent<NeonWave>();

        idleState = Instantiate(idleStatePrefab, transform).GetComponent<IdleSpawnerState>();
        CheckAssignment(idleState,"idleSpawnerState");

        positionSelectionState = Instantiate(positionSelectionStatePrefab, transform).GetComponent<PositionSelectionSpawnerState>();
        CheckAssignment(positionSelectionState, "positionSelectionSpawnerState");
        
        propertySelectionState = Instantiate(propertySelectionStatePrefab, transform).GetComponent<PropertySelectionSpawnerState>();
        CheckAssignment(propertySelectionState,"propertySelectionSpawnerState");

        spawnerPositionWindowUI = positionSelectionState.GetComponent<SpawnerPositionWindowUI>();
        CheckAssignment(spawnerPositionWindowUI, "spawnerPositionWindowUI");
        
        spawnerPropertyPopupUI = propertySelectionState.GetComponent<SpawnerPropertyPopupUI>();
        CheckAssignment(spawnerPropertyPopupUI, "spawnerPropertyPopupUI");
        
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;

        currentState = positionSelectionState;
        currentState.OnEnter(this);
    }

    public void Update() // Will be controller from GameManeger in future So it will Call Tick method
    {
        currentState.Tick(this);
        if (Input.GetKeyDown(selectionKey))
        {
            SetCurrentState(positionSelectionState);
        }
    }
    public void SetCurrentState(ISpawnerState newState)
    {
        _currentState = newState;
        _currentState.OnEnter(this);
    }

    public void Cancel()
    {
        Clear();
    }
    public void Clear()
    {        
        spawnedObject.SetActive(false);

        idleState.gameObject.SetActive(false);
        positionSelectionState.gameObject.SetActive(false);
        propertySelectionState.gameObject.SetActive(false);

        neonWave.gameObject.SetActive(false);
        lineRenderer.enabled = false;
    }
    public void Apply()
    {
        Clear();
        spawnedObject.SetActive(true);
        SetActivePreviewMode(false);
    }

    public virtual void SetActivePreviewMode(bool _isActive)
    {
        if (_isActive)
        {
            spawnedObject.GetComponent<Renderer>().sharedMaterial = previewMat;
            lineRenderer.material = previewMat;
            neonWave.SetColor(previewMat.color);
            lineRenderer.enabled = true;
            neonWave.gameObject.SetActive(true);
        }
        else
        {
            spawnedObject.GetComponent<Renderer>().sharedMaterial = originalMat;
            lineRenderer.material = originalMat;
            neonWave.SetColor(originalMat.color);
            lineRenderer.enabled = false;            
            neonWave.gameObject.SetActive(false);
        }
    }

    public virtual void SetObjectPosition(Vector3 _position)
    {
        if (spawnedObject.transform.position != _position)
        {
            spawnedObject.transform.position = _position;
            neonWave.transform.position = new Vector3(_position.x, 0f, _position.z);
            lineRenderer.SetPosition(0, neonWave.transform.position);
            lineRenderer.SetPosition(1, spawnedObject.transform.position);
        }
    }
    public virtual void SetObjectPositionWithAnim(Vector3 _position, float _duration)
    {
        if (spawnedObject.transform.position != _position)
        {
            StartCoroutine(SetPositionAnim(_position, _duration));
        }
    }

    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }

    IEnumerator SetPositionAnim(Vector3 _position, float _duration)
    {
        Vector3 startPosition = spawnedObject.transform.position;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            SetObjectPosition(Vector3.Lerp(startPosition, _position, t));
            yield return null;
        }
    }

}