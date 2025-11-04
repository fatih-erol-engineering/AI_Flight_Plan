using UnityEngine;

public class AbsoluteRestrictedArea : MonoBehaviour, ICollidable
{
    public float radius = 10f;
    public bool isCollided { get; set; }
    void OnValidate()
    {
        if (Application.isPlaying) return;
        SetRadius(radius, true);
    }

    public void SetRadius(float _val, bool isImmediate = false)
    {
        if (radius != _val || isImmediate)
        {
            radius = _val;
            transform.localScale = Vector3.one * radius * 2f;
        }
    }

    public void SetIsCollided(bool _val, bool isImmediate = false)
    {
        if (ThemeManager.Instance != null && _val != isCollided || isImmediate)
        {
            isCollided = _val;
            GetComponent<MeshRenderer>().material = isCollided ?
                ThemeManager.Instance.theme.collidedRestrictedAreaMaterial :
                ThemeManager.Instance.theme.nonCollidedRestrictedAreaMaterial;
        }
    }
}
