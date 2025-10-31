using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ConflictChecker : MonoBehaviour
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

    public void SolveConflicts()
    {
        StopCoroutine(SolveConflictsCoroutine());
        StartCoroutine(SolveConflictsCoroutine());
    //   var conflictsSnapshot = all_collisionInfoList.ToArray();

    //     foreach (var collision in conflictsSnapshot)
    //     {
    //         float deltaAltitude = 0.1f;
    //         var s1 = collision.segment1;
    //         var s2 = collision.segment2;

    //         s1.controlPoint1.SetPosition(s1.controlPoint1.transform.position + new Vector3(0, deltaAltitude, 0));
    //         s1.controlPoint2?.SetPosition(s1.controlPoint2.transform.position + new Vector3(0, deltaAltitude, 0));
    //         s2.controlPoint1.SetPosition(s2.controlPoint1.transform.position + new Vector3(0, -deltaAltitude, 0));
    //         s2.controlPoint2?.SetPosition(s2.controlPoint2.transform.position + new Vector3(0, -deltaAltitude, 0));
    //     }
    }
    IEnumerator SolveConflictsCoroutine()
    {
        int maxIterations = 200; // güvenlik sınırı
        int iteration = 0;
        float deltaTime = 0f; // saniye
        while (!areAllConflictsResolved() && iteration < maxIterations)
        {
            // snapshot al — listeyi değiştirirsek iterasyon etkilenmesin
            var conflictsSnapshot = all_collisionInfoList.ToArray();

            for (int i = 0; i < conflictsSnapshot.Length; i++)
            {
                var collision = conflictsSnapshot[i];

                // Position Adjustment
                float deltaAltitude = 0.1f;
                var s1 = collision.segment1;
                var s2 = collision.segment2;

                float deltaAltitudeS1 = deltaAltitude * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                float deltaAltitudeS2 = deltaAltitude * s2.aircraft.timeOrPositionChangeVal * s2.aircraft.nonEditableOrEditableVal *(-1f);

                s1.controlPoint1.SetPosition(s1.controlPoint1.transform.position + deltaAltitudeS1 * Vector3.up);
                s1.controlPoint2?.SetPosition(s1.controlPoint2.transform.position + deltaAltitudeS1 * Vector3.up);

                s2.controlPoint1.SetPosition(s2.controlPoint1.transform.position + deltaAltitudeS2 * Vector3.up);
                s2.controlPoint2?.SetPosition(s2.controlPoint2.transform.position + deltaAltitudeS2 * Vector3.up);

                // Time Adjustment
                deltaTime += 0.1f; // saniye
                s1 = collision.segment1;
                s2 = collision.segment2;

                float timeAdjustmentS1 = deltaTime * (1f - s1.aircraft.timeOrPositionChangeVal) * (s1.aircraft.nonEditableOrEditableVal);
                float timeAdjustmentS2 = deltaTime * (1f - s2.aircraft.timeOrPositionChangeVal) * (s2.aircraft.nonEditableOrEditableVal) * (-1f);

                s1.aircraft.SetDeltaTime(new TimeGame(timeAdjustmentS1));
                s2.aircraft.SetDeltaTime(new TimeGame(timeAdjustmentS2));

            }

            CheckConflicts(); // güncelle
            iteration++;
            yield return null;
        }

        if (iteration >= maxIterations)
            Debug.LogWarning($"[{GetType().Name}] SolveConflictsCoroutine stopped after {maxIterations} iterations (possible unresolved conflicts).");
    }
    bool areAllConflictsResolved()
    {
        return all_collisionInfoList.Count == 0;
    }

    public void OnDestroy()
    {
        GameEvents.Instance.OnSplineChanged -= OnSplineChanged;
    }

    public void OnSplineChanged(BSplineDrawer splineDrawer)
    {
        ClearConflicts();
        CheckConflicts();
    }




    // Update is called once per frame
    public void CheckConflicts()
    {
        allTraj = aircraftFactory.GetAllTrajectories();
        all_collisionInfoList.Clear();
        for (int i = 0; i < allTraj.Count; i++)
        {
            for (int j = i + 1; j < allTraj.Count; j++) // j=i+1 ile çift sayımı engelle
            {
                List<CollisionInfo> current_collisionInfoList = allTraj[i].CheckCollisionWithAnotherTrajectory(allTraj[j]);

                if (current_collisionInfoList != null && current_collisionInfoList.Count > 0)
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
    }
    public void ClearConflicts()
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
