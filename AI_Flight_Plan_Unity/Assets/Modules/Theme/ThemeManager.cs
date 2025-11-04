using UnityEngine;

[ExecuteAlways]
public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [field: SerializeField]
    public Theme theme { get; private set; }

    private void OnEnable()
    {
        AssignData();
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    private void AssignData()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("A ThemeManager already exists in the scene. Removing duplicate.", this);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(this);
            else
                Destroy(this);
#else
            Destroy(this);
#endif
        }
    }
}
