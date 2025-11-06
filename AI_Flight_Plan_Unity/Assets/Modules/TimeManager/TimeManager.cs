using UnityEngine;
// [DefaultExecutionOrder(-999)] // 

//[ExecuteAlways]
public class TimeManager : MonoBehaviour
{
    public AircraftFactory aircraftFactory;
    public static TimeManager Instance { get; private set; }
    public TimeSO time;
    public int updateCount = 0;
    private bool isInitialized = false;
    void OnEnable()
    {
        if (isInitialized) return;
        if (!Application.isPlaying)
        {
            AssignData();
        }
    }

    void Awake()
    {
        AssignData();
        updateCount = 0;
    }
    void Start()
    {
        GameEvents.Instance.StartEndTimeChanged(time.startTime, time.endTime);
    }
    void AssignData()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("A TimeManager already exists in the scene. Removing duplicate.", this);
            Destroy(this);
        }
        GameEvents.Instance.OnTrajectoryCreated += (_traj) => OnTrajectoryCreated();
        GameEvents.Instance.OnTrajectoryTimeChanged += (_traj) => OnTrajectoryCreated();
        isInitialized = true;
        time.startTime = 0f;
        time.endTime = 1f;
        GameEvents.Instance.StartEndTimeChanged(time.startTime, time.endTime);
    }
    void OnDestroy()
    {
        GameEvents.Instance.OnTrajectoryCreated -= (_traj) => OnTrajectoryCreated();
        GameEvents.Instance.OnTrajectoryTimeChanged -= (_traj) => OnTrajectoryCreated();
    }

    public void OnTrajectoryCreated()
    {

        aircraftFactory.aircraftList.ForEach(aircraft =>
        {
            if (aircraft.trajectory.startTime.second < time.startTime)
            {
                time.SetStartTime(aircraft.trajectory.startTime.second);
            }
            if (aircraft.trajectory.endTime.second > time.endTime)
            {
                time.SetEndTime(aircraft.trajectory.endTime.second);
            }
        });

    }


    void Update()
    {

        if (Application.isPlaying)
        {

            if (updateCount < 4)
            {
                if (updateCount == 3)
                {
                    time.SendEvents();
                }
                updateCount++;
            }
        }

        switch (time.currentTimeState)
        {
            case TimeState.Playing:
                time.SetCurrentTime(Mathf.Clamp(time.currentTime + Time.deltaTime * time.timeScale, time.startTime, time.endTime));
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

    // void UpdateTimeWithTrajectoryTimes(TrajectoryDrawer[] trajectoryDrawers)
    // {
    //     float minTime = Mathf.Infinity;
    //     float maxTime = Mathf.NegativeInfinity;
    //     foreach (TrajectoryDrawer trajectory in trajectoryDrawers)
    //     {
    //         if (minTime > trajectory.startTime.second)
    //         {
    //             minTime = trajectory.startTime.second;
    //         }
    //         if (maxTime < trajectory.endTime.second)
    //         {
    //             maxTime = trajectory.endTime.second;
    //         }
    //     }
    //     startTime_s = minTime;
    //     endTime_s = maxTime;
    // }



}
