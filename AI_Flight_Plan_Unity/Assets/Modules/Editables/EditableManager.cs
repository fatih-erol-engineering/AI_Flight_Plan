using UnityEngine;

[ExecuteAlways]
public class EditableManager : MonoBehaviour
{


    ////////////////////////////////////////////////

    void OnValidate()
    {
        AssignData();
    }
    void Awake()
    {
        AssignData();
    }
    void OnEnable()
    {
        AssignData();
    }

    ////////////////////////////////////////////////

    void AssignData()
    {
        // Ensure we don't register the same handler multiple times.
        if (GameEvents.Instance != null)
        {
            // Safe idempotent subscription: remove then add to guarantee a single registration
            GameEvents.Instance.OnSelectionChanged -= ShowEditableProperties;
            GameEvents.Instance.OnSelectionChanged += ShowEditableProperties;
        }
    }
    void OnDisable()
    {
        // Clean up event subscription when this component is disabled/destroyed
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnSelectionChanged -= ShowEditableProperties;
        }
    }

    void OnDestroy()
    {
        // If this instance was the singleton, clear it on destroy
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnSelectionChanged -= ShowEditableProperties;
        }
    }


    ////////////////////////////////////////////////

    void ShowEditableProperties(IEditable editable)
    {
        if (editable != null)
        {
            editable.ShowEditableProperties();
        }
    }












    ////////////////////////////////////////////////
}
