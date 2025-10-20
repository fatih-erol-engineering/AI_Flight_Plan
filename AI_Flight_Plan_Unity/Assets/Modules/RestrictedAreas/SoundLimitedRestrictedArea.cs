using UnityEngine;

public class SoundLimitedRestrictedArea : MonoBehaviour
{
    [field:SerializeField] public float currentSound_dBa{ get; private set; }
    [SerializeField] private AircraftFactory aircraftFactory;
    public float radius = 100f;

    [SerializeField] private float soundLimit_dBa = 50f;
    [SerializeField] private Color minSoundColor;
    [SerializeField] private Color maxSoundColor;
    // [SerializeField] GameObject prefab;

    public void Update()
    {
        UpdateSound();
        UpdateColor();
    }
    public void UpdateSound()
    {
        currentSound_dBa = 0f;
        float deltaSound_dBa = 0f;
        if(aircraftFactory.AircraftList != null) 
        {
            foreach (Aircraft aircraft in aircraftFactory.AircraftList)
            {
                float dist = Vector3.Distance(transform.position, aircraft.transform.position);
                if (dist >= radius)
                {
                    deltaSound_dBa = 0f;
                }
                else
                {
                    deltaSound_dBa = Mathf.Lerp(0f, aircraft.aircraftProperties.noise_dBA, 1 - (dist / radius));
                }
                currentSound_dBa += deltaSound_dBa;
            }
        }
    }
    public void UpdateColor()
    {
        Color currentColor = Color.Lerp(minSoundColor, maxSoundColor, currentSound_dBa / soundLimit_dBa);
        GetComponent<MeshRenderer>().material.color = currentColor;
    }


}
