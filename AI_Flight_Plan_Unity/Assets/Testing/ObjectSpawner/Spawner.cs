using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] public KeyCode selectionKey = KeyCode.Return;
    [HideInInspector] public GameObject spawnedObject;
    [field: SerializeField] public ISpawnerState currentState { get; private set; }
    public IdleSpawnerState idleSpawnerState = new IdleSpawnerState();
    public PositionSelectionSpawnerState positionSelectionSpawnerState = new PositionSelectionSpawnerState();
    public PropertySelectionSpawnerState propertySelectionSpawnerState = new PropertySelectionSpawnerState();

    void Start()
    {
        currentState = positionSelectionSpawnerState;
        spawnedObject = Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity);
        currentState.OnEnter(this);
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



}