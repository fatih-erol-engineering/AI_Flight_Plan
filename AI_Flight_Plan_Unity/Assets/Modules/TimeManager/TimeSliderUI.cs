using UnityEngine;
using UnityEngine.UIElements;

public class TimeSliderUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;    
    [SerializeField] private TimeManager timeManager;
    private VisualElement root;
    private Button playPauseBtn;
    private FloatField maxTimeField,currentTimeField;

    [field: SerializeField]
    public bool playFlag { get; private set; } = false;
    private Slider timeSlider;
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

    void Awake()
    {
        float minVal = 0f;
        float maxVal = 10f;

        root = uiDocument.rootVisualElement;
        timeSlider = root.Q<Slider>("timeSlider");
        currentTimeField = root.Q<FloatField>("currentTimeField");
        maxTimeField = root.Q<FloatField>("maxTimeField");
        playPauseBtn = root.Q<Button>("playPauseBtn");

        currentTimeField.formatString = "F1";
        maxTimeField.formatString = "F1";

        timeSlider.value = (minVal + maxVal) / 2;

        SetTimeSliderMinValue(minVal);
        SetTimeSliderMaxValue(maxVal);

        playPauseBtn.clicked += OnPlayPauseBtnClick;
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
    }
    

}
