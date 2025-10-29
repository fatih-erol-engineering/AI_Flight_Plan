using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    [field: SerializeField] public Theme theme { get; private set; }

    void OnEnable()
    {
        AssignData();
    }
    void OnValidate()
    {
        AssignData();
    }
    void Awake()
    {
        AssignData();
    }

    void AssignData()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("A ThemeManager already exists in the scene. Removing duplicate.", this);
#if UNITY_EDITOR
            // Safe to remove component immediately in editor
            DestroyImmediate(this);
#else
            Destroy(this);
#endif
        }
    }


}