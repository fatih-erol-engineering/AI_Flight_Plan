using UnityEngine;
using UnityEngine.UIElements;

public class TimeSliderUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private TimeSO time;
    private VisualElement root;
    private Button playPauseBtn, backwardStepBtn, forwardStepBtn;
    private FloatField maxTimeField, currentTimeField;
    private Slider timeSlider;
    private TimeState currentTimeState;
    void Awake()
    {
        AssignData();

    }
    void OnValidate()
    {
        AssignData();
        SetTimeSliderValue(time.currentTime, true);
        SetTimeSliderMaxValue(time.endTime, true);
        SetTimeSliderMinValue(time.startTime, true);
        SetTimeState(time.currentTimeState, true);
    }

    public void AssignData()
    {
        root = uiDocument.rootVisualElement;
        timeSlider = root.Q<Slider>("timeSlider");
        currentTimeField = root.Q<FloatField>("currentTimeField");
        maxTimeField = root.Q<FloatField>("maxTimeField");
        playPauseBtn = root.Q<Button>("playPauseBtn");
        backwardStepBtn = root.Q<Button>("backwardStepBtn");
        forwardStepBtn = root.Q<Button>("forwardStepBtn");

        currentTimeField.formatString = "F1";
        maxTimeField.formatString = "F1";

        timeSlider.value = 0f;

        playPauseBtn.clicked += OnPlayPauseBtnClick;
        backwardStepBtn.clicked += () =>
        {
            timeSlider.value = timeSlider.lowValue;
            time.SetCurrentTime(timeSlider.value);
        };
        forwardStepBtn.clicked += () =>
        {
            timeSlider.value = timeSlider.highValue;
            time.SetCurrentTime(timeSlider.value);
        };
        timeSlider.RegisterValueChangedCallback(_ => SliderValueChanged());
    }

    void Update()
    {
        SetTimeSliderValue(time.currentTime);
        SetTimeSliderMaxValue(time.endTime);
        SetTimeSliderMinValue(time.startTime);
        SetTimeState(time.currentTimeState);
    }

    private void OnPlayPauseBtnClick()
    {
        switch (time.currentTimeState)
        {
            case TimeState.Playing:
                SetTimeState(TimeState.Paused);
                break;
            case TimeState.Paused:
                SetTimeState(TimeState.Playing);
                break;
        }
    }

    private void SetTimeSliderValue(float _time, bool isImmediate = false)
    {
        if (isImmediate || _time != timeSlider.value)
        {
            timeSlider.value = _time;
            currentTimeField.value = _time;
        }
    }

    public void SetTimeSliderMaxValue(float _val, bool isImmediate = false)
    {
        if (isImmediate || _val != timeSlider.highValue)
        {
            timeSlider.highValue = _val;
            maxTimeField.value = _val;
        }
    }
    public void SetTimeState(TimeState _val, bool isImmediate = false)
    {
        if (isImmediate || _val != currentTimeState)
        {
            switch (_val)
            {
                case TimeState.Playing:
                    playPauseBtn.RemoveFromClassList("timeSliderPauseState");
                    break;
                case TimeState.Paused:
                    playPauseBtn.AddToClassList("timeSliderPauseState");
                    break;
            }
            currentTimeState = _val;
            time.SetTimeState(_val);
        }
    }
    public void SetTimeSliderMinValue(float _val, bool isImmediate = false)
    {
        if (isImmediate || _val != timeSlider.lowValue)
        {
            timeSlider.lowValue = _val;
            time.startTime = _val;
        }
    }

    private void SliderValueChanged()
    {
        time.SetCurrentTime(timeSlider.value);
        currentTimeField.value = timeSlider.value;
    }


}
