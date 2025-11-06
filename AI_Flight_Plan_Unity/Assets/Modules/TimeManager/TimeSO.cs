using UnityEngine;


[CreateAssetMenu(fileName = "TimeSO", menuName = "ScriptableObjects/TimeSO", order = 1)]
//[ExecuteAlways]
public class TimeSO : ScriptableObject
{
    public TimeState currentTimeState = TimeState.Paused;
    public float currentTime = 0f;
    public float startTime = 0f;
    public float endTime = 10f;
    public float timeScale = 1f;

    void Awake()
    {
        SetTimeState(currentTimeState, true);
        SetCurrentTime(currentTime, true);
        SetStartTime(startTime, true);
        SetEndTime(endTime, true);
        SetTimeScale(timeScale, true);
    }


    public void SetTimeState(TimeState _val, bool isImmediate = false)
    {
        if (currentTimeState != _val || isImmediate)
        {
            currentTimeState = _val;
            if (GameEvents.Instance != null) GameEvents.Instance.TimeStateChanged(currentTimeState);
        }
    }
    public void SendEvents()
    {
        if (GameEvents.Instance != null) return;
        GameEvents.Instance.TimeStateChanged(currentTimeState);
        GameEvents.Instance.TimeChanged(currentTime);
        GameEvents.Instance.StartEndTimeChanged(startTime, endTime);
        GameEvents.Instance.TimeScaleChanged(timeScale);
    }
    public void SetCurrentTime(float _val, bool isImmediate = false)
    {
        if (currentTime != _val || isImmediate)
        {
            currentTime = Mathf.Clamp(_val, startTime, endTime);
            if (GameEvents.Instance != null)  GameEvents.Instance.TimeChanged(currentTime);
           
        }
    }
    public void SetStartTime(float _val, bool isImmediate = false)
    {
        if (startTime != _val || isImmediate)
        {
            startTime = _val;
            if (GameEvents.Instance != null)GameEvents.Instance.StartEndTimeChanged(startTime, endTime);
        }
    }
    public void SetEndTime(float _val, bool isImmediate = false)
    {
        if (endTime != _val || isImmediate)
        {
            endTime = _val;
            if (GameEvents.Instance != null)GameEvents.Instance.StartEndTimeChanged(startTime, endTime);
        }
    }
    public void SetTimeScale(float _val, bool isImmediate = false)
    {
        if (timeScale != _val || isImmediate)
        {
            timeScale = _val;
            if (GameEvents.Instance != null)GameEvents.Instance.TimeScaleChanged(timeScale);
        }
    }

}

public enum TimeState
{
    Playing,
    Paused
}