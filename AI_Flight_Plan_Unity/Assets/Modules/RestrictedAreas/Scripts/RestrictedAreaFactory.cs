using System.Collections.Generic;
using UnityEngine;

public class RestrictedAreaFactory : MonoBehaviour
{
    [SerializeField] private GameObject soundRestrictedAreaPrefab;
    [SerializeField] private GameObject absoluteRestrictedAreaPrefab;
    [SerializeField] private List<GameObject> spawned_SoundRestrictedAreas;
    [SerializeField] private List<GameObject> spawned_AbsoluteRestrictedAreas;

    public GameObject CreateRestrictedArea(RestrictedAreaType areaType, Vector3 position, Quaternion rotation)
    {
        GameObject restrictedArea = null;

        switch (areaType)
        {
            case RestrictedAreaType.SoundRestricted:
                restrictedArea = Instantiate(soundRestrictedAreaPrefab, position, rotation, transform);
                spawned_SoundRestrictedAreas.Add(restrictedArea);
                break;

            case RestrictedAreaType.AbsoluteRestricted:
                restrictedArea = Instantiate(absoluteRestrictedAreaPrefab, position, rotation, transform);
                spawned_AbsoluteRestrictedAreas.Add(restrictedArea);
                break;
        }

        return restrictedArea;
    }

}


public enum RestrictedAreaType
{
    SoundRestricted,
    AbsoluteRestricted
}