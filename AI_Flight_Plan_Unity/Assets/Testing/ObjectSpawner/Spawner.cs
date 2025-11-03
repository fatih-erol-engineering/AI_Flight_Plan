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
    [SerializeField] public GameObject spawnedObject;
    [SerializeField] public NeonWave neonWave;
    [SerializeField] public IdleSpawnerState idleState;
    [SerializeField] public PositionSelectionSpawnerState positionSelectionState;
    [SerializeField] public PropertySelectionSpawnerState propertySelectionState;
    [SerializeField] public SpawnerPositionWindowUI spawnerPositionWindowUI;
    [SerializeField] public SpawnerPropertyPopupUI spawnerPropertyPopupUI;
    #endregion

    #region State Instances
    [SerializeField]
    public ISpawnerState currentState
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
        spawnedObject.SetActive(false);

        neonWave = Instantiate(neonWavePrefab, transform).GetComponent<NeonWave>();

        idleState = Instantiate(idleStatePrefab, transform).GetComponent<IdleSpawnerState>();
        CheckAssignment(idleState, "idleSpawnerState");

        positionSelectionState = Instantiate(positionSelectionStatePrefab, transform).GetComponent<PositionSelectionSpawnerState>();
        CheckAssignment(positionSelectionState, "positionSelectionSpawnerState");

        propertySelectionState = Instantiate(propertySelectionStatePrefab, transform).GetComponent<PropertySelectionSpawnerState>();
        CheckAssignment(propertySelectionState, "propertySelectionSpawnerState");

        spawnerPositionWindowUI = positionSelectionState.GetComponent<SpawnerPositionWindowUI>();
        CheckAssignment(spawnerPositionWindowUI, "spawnerPositionWindowUI");

        spawnerPropertyPopupUI = propertySelectionState.GetComponent<SpawnerPropertyPopupUI>();
        CheckAssignment(spawnerPropertyPopupUI, "spawnerPropertyPopupUI");

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;

        SetCurrentState(idleState);
        // SetCurrentState(positionSelectionState);
        spawnerPositionWindowUI.HidePopup();
        spawnerPropertyPopupUI.HidePopup();

    }

    public void Update() // Will be controller from GameManeger in future So it will Call Tick method
    {
        currentState.Tick(this);
        if (Input.GetKeyDown(selectionKey))
        {
            TriggerSpawning();
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

        neonWave.gameObject.SetActive(false);
        lineRenderer.enabled = false;
    }

    public void TriggerSpawning()
    {
        if (spawnedObject == null)
        {
            spawnedObject = Instantiate(spawnedObjectPrefab, transform);
        }
        spawnedObject.SetActive(true);

        neonWave.gameObject.SetActive(true);
        lineRenderer.enabled = true;

        SetCurrentState(positionSelectionState);
    }
    public void Apply()
    {
        // Clear();
        spawnedObject.SetActive(true);
        Instantiate(spawnedObjectPrefab, spawnedObject.transform.position, spawnedObject.transform.rotation, transform);
        spawnedObject.SetActive(false);
        SetActivePreviewMode(false);
        SetCurrentState(idleState);
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
    void CheckAssignment<T>(T obj, string name = "")
    {
        if (obj == null)
            Debug.LogError($"[{GetType().Name}]  Missing required dependency: (type: {typeof(T).Name}), name: {name} )");
    }

}