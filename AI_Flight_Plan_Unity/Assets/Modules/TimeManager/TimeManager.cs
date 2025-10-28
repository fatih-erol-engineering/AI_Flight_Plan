using UnityEngine;

[ExecuteAlways]
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
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
    public bool isUpdated = false;
    public bool timeIsChanging = false;


    void Awake()
    {
        // Ensure a single instance
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    void OnValidate()
    {
        // Ensure a single instance
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    void Update()
    {
        isUpdated = false;
        timeIsChanging = false;
        playFlag = timeSliderUI.playFlag;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playFlag = !playFlag;
        }
        if (playFlag)
        {
            currentTime_s += Time.deltaTime * timeScale;
            currentTime_s = Mathf.Clamp(currentTime_s, startTime_s, endTime_s);
            timeSliderUI.SetTimeSliderValue(currentTime_s);
            timeIsChanging = true;
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
        foreach (Aircraft aircraft in aircraftFactory.AircraftList)
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
        isUpdated = true;
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
