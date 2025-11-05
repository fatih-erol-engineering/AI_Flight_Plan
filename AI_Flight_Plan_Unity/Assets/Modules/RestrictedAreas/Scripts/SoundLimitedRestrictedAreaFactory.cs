using UnityEngine;
using System.Collections.Generic;

public class SoundLimitedRestrictedAreaFactory : MonoBehaviour
{
    [SerializeField] private GameObject soundLimitedRestrictedAreaPrefab;
    [SerializeField] private List<GameObject> spawned_SoundLimitedRestrictedAreas = new List<GameObject>();
    public GameObject Spawn(Vector3 position, float radius, float soundLimit_dBa = 50f)
    {
        GameObject restrictedArea = null;

        restrictedArea = Instantiate(soundLimitedRestrictedAreaPrefab, position, Quaternion.identity, transform);
        SoundLimitedRestrictedArea soundLimitedRestrictedArea = restrictedArea.GetComponent<SoundLimitedRestrictedArea>();
        soundLimitedRestrictedArea.SetRadius(radius, true);
        soundLimitedRestrictedArea.SetSoundLimit(soundLimit_dBa, true);
        // if (minSoundColor.HasValue)
        // {
        //     soundLimitedRestrictedArea.SetMinSoundColor(minSoundColor.Value);
        // }
        // if (maxSoundColor.HasValue)
        // {
        //     soundLimitedRestrictedArea.SetMaxSoundColor(maxSoundColor.Value);
        // }
        spawned_SoundLimitedRestrictedAreas.Add(restrictedArea);

        return restrictedArea;
    }
    public void Clear()
    {
        foreach (var area in spawned_SoundLimitedRestrictedAreas)
        {
            DestroyImmediate(area);
        }
        spawned_SoundLimitedRestrictedAreas.Clear();
    }
}
