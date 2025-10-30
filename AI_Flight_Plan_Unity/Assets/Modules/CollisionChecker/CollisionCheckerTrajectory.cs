using UnityEngine;
using System.Collections.Generic;
public class CollisionCheckerTrajectory : MonoBehaviour
{
    [SerializeField] private AircraftFactory aircraftFactory;
    [SerializeField] private GameObject collisionMarkerPrefab;
    public List<GameObject> markers { get; private set; }
    public List<CollisionInfo> all_collisionInfoList { get; private set; }
    public List<TrajectoryDrawer> allTraj { get; private set; }
    void Awake()
    {
        markers = new List<GameObject>();
        all_collisionInfoList = new List<CollisionInfo>();
        allTraj = new List<TrajectoryDrawer>();
        GameEvents.Instance.OnSplineChanged += OnSplineChanged;
    }


    public void OnDestroy()
    {
        GameEvents.Instance.OnSplineChanged -= OnSplineChanged;
    }

    public void OnSplineChanged(BSplineDrawer splineDrawer)
    {
        ClearCollisions();
        CheckCollisions();
    }
    // Update is called once per frame
    public void CheckCollisions()
    {
        allTraj = aircraftFactory.GetAllTrajectories();
        all_collisionInfoList.Clear();
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
                List<CollisionInfo> current_collisionInfoList = allTraj[i].CheckCollisionWithAnotherTrajectory(allTraj[j]);

                if (current_collisionInfoList.Count != 0 && current_collisionInfoList != null)
                {
                    all_collisionInfoList.AddRange(current_collisionInfoList);
                }
            }
        }
        for (int i = 0; i < all_collisionInfoList.Count; i++)
        {
            all_collisionInfoList[i].segment1.tubeManager.SetIsCollided(true);
            all_collisionInfoList[i].segment2.tubeManager.SetIsCollided(true);
        }

        int ct = 0;
        foreach (var collision in all_collisionInfoList)
        {
            if (ct + 1 < all_collisionInfoList.Count)
            {
                if (all_collisionInfoList[ct].segment1.midPoint != all_collisionInfoList[ct + 1].segment1.midPoint)
                {
                    var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    marker.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
                    marker.transform.position = collision.segment1.midPoint;
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
    public CurveSegment segment1;
    public CurveSegment segment2;
}
