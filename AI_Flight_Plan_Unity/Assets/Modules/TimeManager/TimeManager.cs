using UnityEngine;

public class TimeManager : MonoBehaviour
{
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


    
    void Update()
    {
        playFlag = timeSliderUI.playFlag;
        if (playFlag)
        {
            currentTime_s += Time.deltaTime * timeScale;
            currentTime_s = Mathf.Clamp(currentTime_s, startTime_s, endTime_s);
            timeSliderUI.SetTimeSliderValue(currentTime_s);
            timeSliderUI.SetTimeSliderMinValue(startTime_s);
            timeSliderUI.SetTimeSliderMaxValue(endTime_s);
        }
        else
        {
            currentTime_s = timeSliderUI.GetTime();            
        }
        
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
