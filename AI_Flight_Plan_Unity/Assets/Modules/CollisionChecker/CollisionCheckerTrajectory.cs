using UnityEngine;
using System.Collections.Generic;
public class CollisionCheckerTrajectory : MonoBehaviour
{
    [SerializeField] private AircraftFactory aircraftFactory;
    [SerializeField] private float geometricCollisionTreshold_m = 5f;
    [SerializeField] private float timeCollision_s = 5f;
    public List<GameObject> markers { get; private set; }
    void Awake()
    {
        markers = new List<GameObject>();   
    }

    // Update is called once per frame
    public void CheckCollisions()
    {
        List<Trajectory> allTraj = aircraftFactory.GetAllTrajectories();
        List<CollisionInfo> all_collisionInfoList = new List<CollisionInfo>();
        for (int i = 0; i < allTraj.Count; i++)
        {
            // for (int j = i + 1; j < allTraj.Count; j++)
            // {
            //     current_collisionInfoList = allTraj[i].CheckCollisionWithAnotherTrajectory(allTraj[j], geometricCollisionTreshold_m, timeCollision_s);
            //     all_collisionInfoList.AddRange(current_collisionInfoList);
            // }
            for (int j = 0; j < allTraj.Count; j++)
            {
                if(allTraj[j] == allTraj[i]) continue;
                List<CollisionInfo> current_collisionInfoList = allTraj[i].CheckCollisionWithAnotherTrajectory(allTraj[j], geometricCollisionTreshold_m, timeCollision_s);
                all_collisionInfoList.AddRange(current_collisionInfoList);
            }
        }

        foreach (var collision in all_collisionInfoList)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.GetComponent<MeshRenderer>().material.color = Color.red;
            marker.transform.position = collision.point;
            marker.transform.rotation = Quaternion.identity;
            marker.transform.localScale = Vector3.one * geometricCollisionTreshold_m;
            markers.Add(marker);
        }
    }
    public void ClearCollisions()
    {
        foreach (GameObject marker in markers)
        {
            Destroy(marker);
        }
        markers.Clear();
    }
    
}

public class CollisionInfo
{
    public GameObject objCurrent;
    public GameObject objCollidedWith;
    public Vector3 point;
    public float time;
}
