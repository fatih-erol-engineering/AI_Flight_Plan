using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField]
    private AircraftFactory aircraftFactory;
    [SerializeField]
    private CreateModeManager createModeManager;

    [field: SerializeField]
    private TimeSliderUI timeSliderUI;
    [field: SerializeField]
    public float currentTime_s { get; private set; }
    [field: SerializeField]
    public float startTime_s { get; private set; }
    [field: SerializeField]
    public float endTime_s { get; private set; }

    [field: SerializeField] 
    public bool playFlag { get; private set; }
    [field: SerializeField]
    public float timeScale { get; private set; } = 1f;
    private bool prev_trajectoryCreatedFlag = false;

    void Update()
    {
        playFlag = timeSliderUI.playFlag;
        if (playFlag)
        {
            currentTime_s += Time.deltaTime * timeScale;
            currentTime_s = Mathf.Clamp(currentTime_s, startTime_s, endTime_s);
            timeSliderUI.SetTimeSliderValue(currentTime_s);
        }

        if (createModeManager.trajectoryCreatedFlag != prev_trajectoryCreatedFlag)
        {
            if (createModeManager.trajectoryCreatedFlag)
            {                
                UpdateTimeWithTrajectoryTimes();
            }
        }

        prev_trajectoryCreatedFlag = createModeManager.trajectoryCreatedFlag;
    }

    void UpdateTimeWithTrajectoryTimes()
    {
        float minTime = Mathf.Infinity;
        float maxTime = Mathf.NegativeInfinity;
        foreach (Aircraft aircraft in aircraftFactory.aircraftList)
        {
            if (minTime > aircraft.trajectory.startTime.second)
            {
                minTime = aircraft.trajectory.startTime.second;
            }
            if (maxTime < aircraft.trajectory.endTime.second)
            {
                maxTime = aircraft.trajectory.endTime.second;
            }
        }
        startTime_s = minTime;
        endTime_s = maxTime;        
        timeSliderUI.SetTimeSliderMinValue(startTime_s);
        timeSliderUI.SetTimeSliderMaxValue(endTime_s);

    }
    public void SetCurrentTime(float second)
    {
        currentTime_s = second;
    }
    public void SetStartTime(float second)
    {
        startTime_s = second;
    }
    public void SetEndTime(float second)
    {
        endTime_s = second;
    }

}
