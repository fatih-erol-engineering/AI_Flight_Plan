    using UnityEngine;

public class RotateProp : MonoBehaviour
{
    public float rotationSpeed_rpm = 2000f; 

    void Update()
    {
        // transform.Rotate(Vector3.up, rotationSpeed_rpm * Time.deltaTime * 6f, Space.Self);
        transform.rotation = Quaternion.AngleAxis(rotationSpeed_rpm * 6f * Time.time, Vector3.up);
    }
}
