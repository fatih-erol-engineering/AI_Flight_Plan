using UnityEngine;

// [ExecuteAlways]
// [DefaultExecutionOrder(-999)] // 
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    [field: SerializeField] public TimeState currentTimeState { get; private set; } = TimeState.Paused;
    [field: SerializeField] public float currentTime_s { get; private set; } = 0f;
    [field: SerializeField] public float startTime_s { get; private set; } = 0f;
    [field: SerializeField] public float endTime_s { get; private set; } = 10f;
    [field: SerializeField] public float timeScale { get; private set; } = 1f;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        AssignData();
        SetTimeState(currentTimeState, false, true);
        SetCurrentTime(currentTime_s, false, true);
        SetStartTime(startTime_s, true);
        SetEndTime(endTime_s, true);
        SetTimeScale(timeScale, false, true);
    }
    void AssignData()
    {
        // Ensure a single instance
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // GameEvents.Instance.OnTrajectoryCreated -= OnTrajectoryCreated;
        // GameEvents.Instance.OnTrajectoryCreated += OnTrajectoryCreated;
        // GameEvents.Instance.OnSplineChanged -= OnSplineChanged;
        // GameEvents.Instance.OnSplineChanged += OnSplineChanged;

        GameEvents.Instance.OnTimeChangedInUI -= (_time) => SetCurrentTime(_time, true);
        GameEvents.Instance.OnTimeChangedInUI += (_time) => SetCurrentTime(_time, true);

        GameEvents.Instance.OnTimePausedInUI -= () => SetTimeState(TimeState.Paused, true);
        GameEvents.Instance.OnTimePausedInUI += () => SetTimeState(TimeState.Paused, true);

        GameEvents.Instance.OnTimePlayedInUI -= () => SetTimeState(TimeState.Playing, true);
        GameEvents.Instance.OnTimePlayedInUI += () => SetTimeState(TimeState.Playing, true);
    }
    void Update()
    {
        switch (currentTimeState)
        {
            case TimeState.Playing:
                SetCurrentTime(Mathf.Clamp(currentTime_s + Time.deltaTime * timeScale, startTime_s, endTime_s), false);
                break;
            case TimeState.Paused:
                break;
        }
    }

    // public void OnTrajectoryCreated(TrajectoryDrawer trajectoryDrawer)
    // {
    //     UpdateTimeWithTrajectoryTimes();
    // }
    // public void OnSplineChanged(BSplineDrawer _)
    // {
    //     UpdateTimeWithTrajectoryTimes();
    // }

    void UpdateTimeWithTrajectoryTimes(TrajectoryDrawer[] trajectoryDrawers)
    {
        float minTime = Mathf.Infinity;
        float maxTime = Mathf.NegativeInfinity;
        foreach (TrajectoryDrawer trajectory in trajectoryDrawers)
        {
            if (minTime > trajectory.startTime.second)
            {
                minTime = trajectory.startTime.second;
            }
            if (maxTime < trajectory.endTime.second)
            {
                maxTime = trajectory.endTime.second;
            }
        }
        startTime_s = minTime;
        endTime_s = maxTime;
    }
    public void SetCurrentTime(float _val, bool _isUIChange = false, bool _isImmediate = false)
    {
        if (_val != currentTime_s || _isImmediate)
        {
            currentTime_s = _val;
            GameEvents.Instance.TimeChanged(this, _isUIChange);
        }
    }
    public void SetStartTime(float _val, bool _isImmediate = false)
    {
        if (_val != startTime_s || _isImmediate)
        {
            startTime_s = _val;
            GameEvents.Instance.StartTimeChanged(this);
        }
    }
    public void SetEndTime(float _val, bool _isImmediate = false)
    {
        if (_val != endTime_s || _isImmediate)
        {
            endTime_s = _val;
            GameEvents.Instance.EndTimeChanged(this);
        }
    }
    public void SetTimeScale(float _val, bool _isUIChange = false, bool _isImmediate = false)
    {
        if (_val != timeScale || _isImmediate)
        {
            timeScale = _val;
            GameEvents.Instance.TimeScaleChanged(this, _isUIChange);
        }
    }

    public void SetTimeState(TimeState _state, bool _isUIChange = false, bool _isImmediate = false)
    {
        if (_state != currentTimeState || _isImmediate)
        {
            currentTimeState = _state;
            GameEvents.Instance.TimeStateChanged(this, _isUIChange);
        }
    }


}

public enum TimeState
{
    Playing,
    Paused
}
