using UnityEngine;
using System.Collections.Generic;
public class CollisionCheckerTrajectory : MonoBehaviour
{
    [SerializeField] private AircraftFactory aircraftFactory;
    [SerializeField] private float geometricCollisionTreshold_m = 5f;
    [SerializeField] private float timeCollision_s = 5f;
    public bool collisionFlag { get; private set; } = false;
    public List<(GameObject, GameObject)> collidedGameObjects { get; private set; }
    public List<CollidedObjects> collidedGameObjectsList { get; private set; }
    public List<GameObject> markers { get; private set; }
    void Awake()
    {
        collidedGameObjects = new List<(GameObject, GameObject)>();
        collidedGameObjectsList = new List<CollidedObjects>();
        markers = new List<GameObject>();   
    }

    // Update is called once per frame
    public void CheckCollisions()
    {
        List<Trajectory> allTraj = aircraftFactory.GetAllTrajectories();
        for (int i = 0; i < allTraj.Count; i++)
        {
            for (int j = i + 1; j < allTraj.Count; j++)
            {
                var collisions = allTraj[i].CheckCollisionWithAnotherTrajectory(allTraj[j], geometricCollisionTreshold_m, timeCollision_s);
                if (collisions.Count > 0)
                {
                    collisionFlag = true;
                    foreach (var collision in collisions)
                    {
                        collidedGameObjects.Add((allTraj[i].gameObject, allTraj[j].gameObject));
                        collidedGameObjectsList.Add(new CollidedObjects(allTraj[i].gameObject, allTraj[j].gameObject));
                        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        marker.transform.position = collision.point;
                        marker.transform.rotation = Quaternion.identity;
                        markers.Add(marker);
                    }
                }
            }
        }
    }
    public void ClearCollisions()
    {
        collisionFlag = false;
        collidedGameObjects.Clear();
        collidedGameObjectsList.Clear();
        foreach (GameObject marker in markers)
        {
            Destroy(marker);
        }
        markers.Clear();
    }
    
}
public class CollidedObjects
{
    public GameObject obj1;
    public GameObject obj2;

    public CollidedObjects(GameObject obj1_, GameObject obj2_)
    {
        obj1 = obj1_;
        obj2 = obj2_;
    }
}

public class CollisionInfo
{
    public Vector3 point;
    public float time;
}
