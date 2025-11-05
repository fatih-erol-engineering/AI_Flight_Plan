using UnityEngine;
using System.Collections.Generic;

public class AbsoluteRestrictedAreaFactory : MonoBehaviour
{
    [SerializeField] private GameObject absoluteRestrictedAreaPrefab;
    [SerializeField] private List<GameObject> spawned_AbsoluteRestrictedAreas = new List<GameObject>();
    public GameObject Spawn(Vector3 position, float radius)
    {
        GameObject restrictedArea = null;

        restrictedArea = Instantiate(absoluteRestrictedAreaPrefab, position, Quaternion.identity, transform);
        AbsoluteRestrictedArea absoluteRestrictedArea = restrictedArea.GetComponent<AbsoluteRestrictedArea>();
        absoluteRestrictedArea.SetRadius(radius);
        spawned_AbsoluteRestrictedAreas.Add(restrictedArea);

        return restrictedArea;
    }
    public void Clear()
    {
        foreach (var area in spawned_AbsoluteRestrictedAreas)
        {
            DestroyImmediate(area);
        }
        spawned_AbsoluteRestrictedAreas.Clear();
    }
}
