using UnityEngine;
using UnityEngine.UIElements;

public class TimeSliderUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private Button playPauseBtn, backwardStepBtn, forwardStepBtn;
    private FloatField maxTimeField, currentTimeField;

    [field: SerializeField]
    public bool playFlag { get; private set; } = false;
    private Slider timeSlider;

    void Awake()
    {
        AssignData();

    }



    public void AssignData()
    {
        float minVal = 0f;
        float maxVal = 10f;

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

        SetTimeSliderMinValue(minVal);
        SetTimeSliderMaxValue(maxVal);

        playPauseBtn.clicked += OnPlayPauseBtnClick;
        backwardStepBtn.clicked += () =>
        {
            timeSlider.value = timeSlider.lowValue;
            GameEvents.Instance.TimeChangedInUI(timeSlider.value);
        };
        forwardStepBtn.clicked += () =>
        {
            timeSlider.value = timeSlider.highValue;
            GameEvents.Instance.TimeChangedInUI(timeSlider.value);
        };
        timeSlider.RegisterValueChangedCallback(_ => SliderValueChanged());

        GameEvents.Instance.OnTimeChanged -= SetTimeSliderValueFromManager;
        GameEvents.Instance.OnTimeChanged += SetTimeSliderValueFromManager;
        GameEvents.Instance.OnTimeStateChanged += SetTimeStateFromManager;
        GameEvents.Instance.OnStartTimeChanged -= (_timeManager) => SetTimeSliderMinValue(_timeManager.startTime_s);
        GameEvents.Instance.OnStartTimeChanged += (_timeManager) => SetTimeSliderMinValue(_timeManager.startTime_s);
        GameEvents.Instance.OnEndTimeChanged -= (_timeManager) => SetTimeSliderMaxValue(_timeManager.endTime_s);
        GameEvents.Instance.OnEndTimeChanged += (_timeManager) => SetTimeSliderMaxValue(_timeManager.endTime_s);

    }

    private void OnPlayPauseBtnClick()
    {
        if (playFlag)
        {
            playFlag = false;
            playPauseBtn.AddToClassList("timeSliderPauseState");
            GameEvents.Instance.TimePausedInUI();
        }
        else
        {
            playFlag = true;
            playPauseBtn.RemoveFromClassList("timeSliderPauseState");
            GameEvents.Instance.TimePlayedInUI();
        }
    }
    private void SetTimeStateFromManager(TimeManager _timeManager, bool _isFromUI)
    {
        if (!_isFromUI)
        {
            if (_timeManager.currentTimeState == TimeState.Playing && !playFlag)
            {
                playFlag = true;
                playPauseBtn.RemoveFromClassList("timeSliderPauseState");
            }
            else if (_timeManager.currentTimeState == TimeState.Paused && playFlag)
            {
                playFlag = false;
                playPauseBtn.AddToClassList("timeSliderPauseState");
            }
        }
    }

    private void SetTimeSliderValue(float _time)
    {
        if (_time != timeSlider.value)
        {
            timeSlider.value = _time;
        }
    }
    public void SetTimeSliderMinValue(float val, bool)
    {
        timeSlider.lowValue = val;
    }
    public void SetTimeSliderMaxValue(float val)
    {
        timeSlider.highValue = val;
        maxTimeField.value = val;
    }
    public void SetTimeSliderValueFromManager(TimeManager _timeManager, bool _isFromUI)
    {
        if (!_isFromUI)
        {
            SetTimeSliderValue(_timeManager.currentTime_s);
        }
    }

    private void SliderValueChanged()
    {
        GameEvents.Instance.TimeChangedInUI(timeSlider.value);
    }


}
