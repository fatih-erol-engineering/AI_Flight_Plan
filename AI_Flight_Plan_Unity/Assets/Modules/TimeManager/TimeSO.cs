using UnityEngine;


[CreateAssetMenu(fileName = "TimeSO", menuName = "ScriptableObjects/TimeSO", order = 1)]
public class TimeSO : ScriptableObject
{
    [field: SerializeField] public float currentTime { get; private set; } = 0f;
    [field: SerializeField] public float startTime { get; private set; } = 0f;
    [field: SerializeField] public float endTime { get; private set; } = 10f;
    [field: SerializeField] public float timeScale { get; private set; } = 1f;
}
