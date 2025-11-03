using UnityEngine;

public class MoveAircraftsWithTime : MonoBehaviour
{
    [SerializeField]
    private AircraftFactory aircraftFactory;

    void Update()
    {
        // if (TimeManager.Instance.timeIsChanging)
        // {
        //     foreach(Aircraft aircraft in aircraftFactory.AircraftList)
        //     {
        //         aircraft.MoveAircraftWithTime(TimeManager.Instance.currentTime_s);
        //     }
        // }
    }
}
