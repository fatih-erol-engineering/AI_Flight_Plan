using UnityEngine;
// [DefaultExecutionOrder(-999)] // 

[ExecuteAlways]
public class TimeManager : MonoBehaviour
{
    public AircraftFactory aircraftFactory;
    public static TimeManager Instance { get; private set; }
    public TimeSO time;
    void OnEnable()
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
        GameEvents.Instance.OnTrajectoryCreated -= OnTrajectoryCreated;
        GameEvents.Instance.OnTrajectoryCreated += OnTrajectoryCreated;
    }
    public void OnTrajectoryCreated(TrajectoryDrawer trajectoryDrawer)
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
