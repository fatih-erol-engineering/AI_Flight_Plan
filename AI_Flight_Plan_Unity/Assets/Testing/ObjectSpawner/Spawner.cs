using UnityEngine;


public class Spawner : MonoBehaviour
{
    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] public KeyCode selectionKey = KeyCode.Return;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject neonCirclePrefab;
    [HideInInspector] public GameObject spawnedObject;
    [SerializeField] private Material previewMat;
    [SerializeField] private Material originalMat;
    [SerializeField] public SpawnerPositionWindowUI spawnerPositionWindowUI;
    [SerializeField] public SpawnerPropertyPopupUI spawnerPropertyPopupUI;

    [field: SerializeField] public ISpawnerState currentState { get; private set; }
    public IdleSpawnerState idleSpawnerState = new IdleSpawnerState();
    public PositionSelectionSpawnerState positionSelectionSpawnerState = new PositionSelectionSpawnerState();
    public PropertySelectionSpawnerState propertySelectionSpawnerState = new PropertySelectionSpawnerState();

    void Start()
    {
        currentState = positionSelectionSpawnerState;
        spawnedObject = Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity);
        currentState.OnEnter(this);
        originalMat = spawnedObject.GetComponent<Renderer>().sharedMaterial;

    }
    public void Update() // Will be controller from GameManeger in future So it will Call Tick method
    {
        currentState.Tick(this);
        if (Input.GetKeyDown(selectionKey))
        {
            SetCurrentState(positionSelectionSpawnerState);
        }
    }
    public void SetCurrentState(ISpawnerState newState)
    {
        currentState = newState;
        currentState.OnEnter(this);
    }

    public void CancelSpawning()
    {
        Destroy(spawnedObject);
    }

    public virtual void SetObjectPreview(bool isPreview)
    {
        if (isPreview)
        {
            spawnedObject.GetComponent<Renderer>().sharedMaterial = previewMat;
        }
        else
        {
            spawnedObject.GetComponent<Renderer>().sharedMaterial = originalMat;
        }
    }
    public virtual void SetObjectPosition(Vector3 _position)
    {
        if (spawnedObject.transform.position != _position)
        {
            spawnedObject.transform.position = _position;
            neonCirclePrefab.transform.position = new Vector3(_position.x, 0f, _position.z);
            lineRenderer.SetPosition(0, neonCirclePrefab.transform.position);
            lineRenderer.SetPosition(1, spawnedObject.transform.position);
        }
    }




}