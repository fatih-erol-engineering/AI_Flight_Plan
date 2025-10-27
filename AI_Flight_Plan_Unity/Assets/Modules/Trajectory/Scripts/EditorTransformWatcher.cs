using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects transform changes in the Editor (when not playing) and invokes an event.
/// Attach to a GameObject and hook the OnTransformChanged UnityEvent in the Inspector,
/// or implement IEditorTransformListener on a component on the same GameObject to get a callback.
/// This component does nothing at runtime (Application.isPlaying == true).
/// </summary>
[ExecuteAlways]
public class EditorTransformWatcher : MonoBehaviour
{
    [Tooltip("Invoked when the transform changes in the Editor (position/rotation/scale).")]
    public UnityEvent OnTransformChanged;

    [Header("What to watch (Editor only)")]
    public bool watchPosition = true;
    public bool watchRotation = true;
    public bool watchScale = true;

    Vector3 _prevLocalPos;
    Quaternion _prevLocalRot;
    Vector3 _prevLocalScale;

    void OnEnable()
    {
        CacheValues();
    }

    void Reset()
    {
        CacheValues();
    }

    void CacheValues()
    {
        _prevLocalPos = transform.localPosition;
        _prevLocalRot = transform.localRotation;
        _prevLocalScale = transform.localScale;
    }

    void Update()
    {
#if UNITY_EDITOR
        // Only operate in Editor and when not playing
        if (Application.isPlaying) return;

        bool changed = false;

        if (watchPosition && transform.localPosition != _prevLocalPos) changed = true;
        if (watchRotation && transform.localRotation != _prevLocalRot) changed = true;
        if (watchScale && transform.localScale != _prevLocalScale) changed = true;

        if (changed)
        {
            _prevLocalPos = transform.localPosition;
            _prevLocalRot = transform.localRotation;
            _prevLocalScale = transform.localScale;

            // Invoke inspector-wired callbacks
            OnTransformChanged?.Invoke();

            // Also call any local components implementing IEditorTransformListener
            var listeners = GetComponents<IEditorTransformListener>();
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnEditorTransformChanged();
            }
        }
#endif
    }
}

/// <summary>
/// Implement this on a component to get a direct callback when the GameObject's transform
/// changes in the Editor (only when not playing).
/// </summary>
public interface IEditorTransformListener
{
    void OnEditorTransformChanged();
}
