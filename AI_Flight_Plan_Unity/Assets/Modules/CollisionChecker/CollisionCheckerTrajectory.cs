using UnityEngine;
using System.Collections.Generic;
public class CollisionCheckerTrajectory : MonoBehaviour
{
    [SerializeField] private AircraftFactory aircraftFactory;
    [SerializeField] private GameObject collisionMarkerPrefab;
    [SerializeField] private float geometricCollisionTreshold_m = 5f;
    [SerializeField] private float timeCollision_s = 5f;
    public List<GameObject> markers { get; private set; }
    void Awake()
    {
        markers = new List<GameObject>();
        GameEvents.instance.OnSplineChanged += OnSplineChanged;
    }


    public void OnDestroy()
    {
        GameEvents.instance.OnSplineChanged -= OnSplineChanged;
    }

    public void OnSplineChanged(BSplineDrawer splineDrawer)
    {
        ClearCollisions();
        CheckCollisions();
    }
    // Update is called once per frame
    public void CheckCollisions()
    {
        List<TrajectoryDrawer> allTraj = aircraftFactory.GetAllTrajectories();
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
                if (allTraj[j] == allTraj[i]) continue;
                List<CollisionInfo> current_collisionInfoList = allTraj[i].CheckCollisionWithAnotherTrajectory(allTraj[j], geometricCollisionTreshold_m, timeCollision_s);

                if (current_collisionInfoList.Count != 0 && current_collisionInfoList != null)
                {
                    // Vector3 cumPos = Vector3.zero;
                    // Vector3 minPos = current_collisionInfoList[0].point;
                    // Vector3 maxPos = current_collisionInfoList[0].point;
                    // foreach (CollisionInfo collisionInfo in current_collisionInfoList)
                    // {
                    //     // Handle each collision info (e.g., log it, visualize it, etc.)
                    //     cumPos += collisionInfo.point;
                    //     minPos = Vector3.Min(minPos, collisionInfo.point);
                    //     maxPos = Vector3.Max(maxPos, collisionInfo.point);
                    // }
                    // float rangeOfCollision = Vector3.Distance(minPos, maxPos);
                    // Vector3 avgPos = cumPos / current_collisionInfoList.Count;
                    // var marker = Instantiate(collisionMarkerPrefab, transform);                                        
                    // marker.transform.position = avgPos;
                    // marker.transform.rotation = Quaternion.identity;
                    // marker.transform.localScale = Vector3.one * rangeOfCollision;
                    // marker.transform.parent = this.transform;
                    // markers.Add(marker);

                    all_collisionInfoList.AddRange(current_collisionInfoList);
                }
            }
        }

        int ct = 0;
        foreach (var collision in all_collisionInfoList)
        {
            if (ct + 1 < all_collisionInfoList.Count)
            {
                if (all_collisionInfoList[ct].point != all_collisionInfoList[ct + 1].point)
                {
                    var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    marker.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
                    marker.transform.position = collision.point;
                    marker.transform.rotation = Quaternion.identity;
                    // marker.transform.localScale = Vector3.one * geometricCollisionTreshold_m;
                    marker.transform.parent = this.transform;
                    markers.Add(marker);
                }
            }
            ct++;
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
    public bool isCollided;
    public GameObject objCurrent;
    public GameObject objCollidedWith;
    public Vector3 point;
    public TimeGame time;
}
