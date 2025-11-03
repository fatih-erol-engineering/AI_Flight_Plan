using UnityEngine;
using System.Collections.Generic;
using System.Collections;
[ExecuteAlways]
public class ConflictCheckerForEditor : MonoBehaviour
{
    public List<CollisionInfo> all_collisionInfoList { get; private set; }
    [SerializeField] List<TrajectoryDrawer> allTraj;
    void AssignData()
    {
        all_collisionInfoList = new List<CollisionInfo>();
        GameEvents.Instance.OnSplineChanged -= OnSplineChanged;
        GameEvents.Instance.OnSplineChanged += OnSplineChanged;
    }
    void Awake()
    {
        AssignData();
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
        while (!areAllConflictsResolved() && iteration < maxIterations)
        {
            // snapshot al — listeyi değiştirirsek iterasyon etkilenmesin
            var conflictsSnapshot = all_collisionInfoList.ToArray();

            for (int i = 0; i < conflictsSnapshot.Length; i++)
            {
                var collision = conflictsSnapshot[i];

                // Position Adjustment
                float dist = 0.1f;
                var s1 = collision.segment1;
                var s2 = collision.segment2;
                Vector3 deltaPos = dist * CylinderIntersectNormal(s1.tubeManager.GetStartPosition(), s1.tubeManager.GetEndPosition(), s1.tubeManager.GetRadius(),
                 s2.tubeManager.GetStartPosition(), s2.tubeManager.GetEndPosition(), s2.tubeManager.GetRadius());

                Vector3 deltaPos1 = deltaPos * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                Vector3 deltaPos2 = deltaPos * s2.aircraft.timeOrPositionChangeVal * s2.aircraft.nonEditableOrEditableVal * (-1f);

                s1.controlPoint1.SetPosition(s1.controlPoint1.transform.position + deltaPos1);
                s1.controlPoint2?.SetPosition(s1.controlPoint2.transform.position + deltaPos1);

                s2.controlPoint1.SetPosition(s2.controlPoint1.transform.position + deltaPos2);
                s2.controlPoint2?.SetPosition(s2.controlPoint2.transform.position + deltaPos2);

                // Time Adjustment
                float deltaTime = 0.1f; // saniye
                s1 = collision.segment1;
                s2 = collision.segment2;

                float timeAdjustmentS1 = deltaTime * (1f - s1.aircraft.timeOrPositionChangeVal) * (s1.aircraft.nonEditableOrEditableVal);
                float timeAdjustmentS2 = deltaTime * (1f - s2.aircraft.timeOrPositionChangeVal) * (s2.aircraft.nonEditableOrEditableVal) * (-1f);

                s1.aircraft.AddDeltaTime(new TimeGame(timeAdjustmentS1));
                s2.aircraft.AddDeltaTime(new TimeGame(timeAdjustmentS2));

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
        CheckConflicts();
    }




    // Update is called once per frame
    public void CheckConflicts()
    {
        AssignData();
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
    public static Vector3 CylinderIntersectNormal(
       Vector3 startA, Vector3 endA, float radiusA,
       Vector3 startB, Vector3 endB, float radiusB)
    {
        // --- 1. Eksen yön vektörlerini ve uzunlukları hesapla
        Vector3 uA = (endA - startA);
        Vector3 uB = (endB - startB);
        float lenA = uA.magnitude;
        float lenB = uB.magnitude;
        uA.Normalize();
        uB.Normalize();

        // --- 2. Eksenler arası en kısa mesafeyi bul
        Vector3 n = Vector3.Cross(uA, uB);
        return n.normalized;
    }
}
