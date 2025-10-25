using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    [field: SerializeField] public Theme theme { get; private set; }

    void OnValidate()
    {
        // In editor, prefer the first created instance as the singleton and remove duplicates
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
            return;
        }
    }
    void Awake()
    {
        // Runtime enforce singleton: keep the first instance, destroy later duplicates
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("A ThemeManager already exists. Destroying duplicate.", this);
            Destroy(this);
            return;
        }
    }
}