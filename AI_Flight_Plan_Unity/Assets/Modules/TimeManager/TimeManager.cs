using UnityEngine;

// [ExecuteAlways]
// [DefaultExecutionOrder(-999)] // 
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public TimeSO time;
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
