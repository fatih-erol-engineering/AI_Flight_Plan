using UnityEngine;

[System.Serializable]
public class TimeGame
{
    [field: SerializeField]
    public float second { get; private set; } = 0f;
    public void SetTime(float _second)
    {
        second = _second;
    }
    public TimeGame(float _second)
    {
        second = _second;
    }
    //public float day;
    //[Range(0, 24)]
    //public float hour;
    //[Range(0, 60)]
    //public float minute;
    //[Range(0, 60)]
    //public float second;
    //[Range(0, 100)]
    //public float miliSecond;

    //public void Normalize()
    //{
    //    // Milisecond -> Second
    //    if (miliSecond >= 100)
    //    {
    //        second += Mathf.Floor(miliSecond / 100f);
    //        miliSecond = miliSecond % 100f;
    //    }

    //    // Second -> Minute
    //    if (second >= 60)
    //    {
    //        minute += Mathf.Floor(second / 60f);
    //        second = second % 60f;
    //    }

    //    // Minute -> Hour
    //    if (minute >= 60)
    //    {
    //        hour += Mathf.Floor(minute / 60f);
    //        minute = minute % 60f;
    //    }

    //    // Hour -> Day
    //    if (hour >= 24)
    //    {
    //        day += Mathf.Floor(hour / 24f);
    //        hour = hour % 24f;
    //    }
    //}


}


