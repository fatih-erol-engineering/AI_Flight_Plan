using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class SoundLimitedRestrictedArea : MonoBehaviour
{
    [field: SerializeField] public float currentSound_dBa { get; private set; }
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
        if (aircraftFactory.aircraftList != null)
        {
            foreach (Aircraft aircraft in aircraftFactory.aircraftList)
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
    public void SetRadius(float _val, bool isImmediate = false)
    {
        if (radius != _val || isImmediate)
        {
            radius = _val;
            transform.localScale = Vector3.one * radius * 2f;
        }
    }
    public void SetSoundLimit(float _val, bool isImmediate = false)
    {
        if (soundLimit_dBa != _val || isImmediate)
        {
            soundLimit_dBa = _val;
        }
    }
    public void SetMinSoundColor(Color _val, bool isImmediate = false)
    {
        if (minSoundColor != _val || isImmediate)
        {
            minSoundColor = _val;
            UpdateColor();
        }
    }
    public void SetMaxSoundColor(Color _val, bool isImmediate = false)
    {
        if (maxSoundColor != _val || isImmediate)
        {
            maxSoundColor = _val;
            UpdateColor();
        }
    }
    public void UpdateColor()
    {
        Color currentColor = Color.Lerp(minSoundColor, maxSoundColor, currentSound_dBa / soundLimit_dBa);
        MeshRenderer rend = GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", currentColor); // URP/HDRP için _BaseColor, Standart Shader için _Color olabilir
        rend.SetPropertyBlock(mpb);
    }


}
