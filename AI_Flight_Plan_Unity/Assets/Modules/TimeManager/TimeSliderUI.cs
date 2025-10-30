using UnityEngine;
using UnityEngine.UIElements;

public class TimeSliderUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private TimeManager timeManager;
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

    public void SetTimeSliderMinValue(float val)
    {
        timeSlider.lowValue = val;
    }
    public void SetTimeSliderMaxValue(float val)
    {
        timeSlider.highValue = val;
        maxTimeField.value = val;
    }
    public void SetTimeSliderValue(float val)
    {
        timeSlider.value = val;
    }
    public float GetTime()
    {
        return timeSlider.value;
    }
    // void OnValidate()
    // {
    //     AssignData();
    // }

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
            timeSlider.value = timeSlider.lowValue + 0.001f;
            timeManager.SetCurrentTime(timeSlider.value);
            timeManager.timeIsChanging = true;
        };
        forwardStepBtn.clicked += () =>
        {
            timeSlider.value = timeSlider.highValue - 0.001f;
            timeManager.SetCurrentTime(timeSlider.value);
            timeManager.timeIsChanging = true;
        };
        timeSlider.RegisterValueChangedCallback(_ => SliderValueChanged());
    }

    void Update()
    {
        currentTimeField.value = timeSlider.value;
    }

    private void OnPlayPauseBtnClick()
    {
        if (playFlag)
        {
            playFlag = false;
            playPauseBtn.AddToClassList("timeSliderPauseState");
        }
        else
        {
            playFlag = true;
            playPauseBtn.RemoveFromClassList("timeSliderPauseState");
        }
    }

    private void SliderValueChanged()
    {
        timeManager.SetCurrentTime(timeSlider.value);
        timeManager.timeIsChanging = true;
    }


}
