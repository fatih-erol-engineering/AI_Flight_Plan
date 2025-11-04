using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ConflictChecker : MonoBehaviour
{
    [SerializeField] private AircraftFactory aircraftFactory;
    public List<CollisionInfo> all_collisionInfoList = new List<CollisionInfo>();
    public List<CollisionInfoRestrictedArea> all_collisionInfoRestrictedAreaList = new List<CollisionInfoRestrictedArea>();
    public List<TrajectoryDrawer> allTraj = new List<TrajectoryDrawer>();
    public List<AbsoluteRestrictedArea> allAbsoluteRestrictedAreas = new List<AbsoluteRestrictedArea>();
    private bool _eventsBound;
    void OnEnable()
    {
        if (!_eventsBound && GameEvents.Instance != null)
        {
            GameEvents.Instance.OnSplineChanged += OnSplineChanged;
            _eventsBound = true;
        }
    }
    void OnDisable()
    {
        if (_eventsBound && GameEvents.Instance != null)
        {
            GameEvents.Instance.OnSplineChanged -= OnSplineChanged;
            _eventsBound = false;
        }
    }

    public void SolveConflicts()
    {
        StopCoroutine(SolveConflictsCoroutine());
        StartCoroutine(SolveConflictsCoroutine());

        StopCoroutine(SolveRestrictedAreaConflictsCoroutine());
        StartCoroutine(SolveRestrictedAreaConflictsCoroutine());
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


    IEnumerator SolveRestrictedAreaConflictsCoroutine()
    {
        int maxIterations = 200; // güvenlik sınırı
        int iteration = 0;
        while (!areAllRestrictedAreaConflictsResolved() && iteration < maxIterations)
        {
            // snapshot al — listeyi değiştirirsek iterasyon etkilenmesin
            var conflictsSnapshot = all_collisionInfoRestrictedAreaList.ToArray();

            for (int i = 0; i < conflictsSnapshot.Length; i++)
            {
                var collision = conflictsSnapshot[i];

                var s1 = collision.segment;
                var s2 = collision.restrictedArea;
                float dist = s2.radius / 20f;
                Vector3 deltaPos1 = s2.transform.position - s1.startPoint.position;
                Vector3 deltaPos2 = s2.transform.position - s1.endPoint.position;
                float t1 = deltaPos1.magnitude / (deltaPos1.magnitude + deltaPos2.magnitude);
                float t2 = deltaPos2.magnitude / (deltaPos1.magnitude + deltaPos2.magnitude);

                float dist1 = dist * t2; //Ters oran var
                float dist2 = dist * t1;//Ters oran var
                Vector3 movePos1 = deltaPos1.normalized * dist1 * (-1f) * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                Vector3 movePos2 = deltaPos2.normalized * dist2 * (-1f) * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;

                s1.controlPoint1.SetPosition(s1.controlPoint1.transform.position + movePos1);
                s1.controlPoint2?.SetPosition(s1.controlPoint2.transform.position + movePos2);
            }

            CheckRestrictedAreaConflicts(); // güncelle
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

    bool areAllRestrictedAreaConflictsResolved()
    {
        return all_collisionInfoRestrictedAreaList.Count == 0;
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
        if (all_collisionInfoList == null)
            all_collisionInfoList = new List<CollisionInfo>();
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
            all_collisionInfoList[i].segment1.SetIsCollided(true);
            all_collisionInfoList[i].segment2.SetIsCollided(true);
        }
    }


    public void CheckRestrictedAreaConflicts()
    {
        if (all_collisionInfoRestrictedAreaList == null)
            all_collisionInfoRestrictedAreaList = new List<CollisionInfoRestrictedArea>();
        all_collisionInfoRestrictedAreaList.Clear();
        for (int i = 0; i < allTraj.Count; i++)
        {
            for (int j = 0; j < allAbsoluteRestrictedAreas.Count; j++) // j=i+1 ile çift sayımı engelle
            {
                List<CollisionInfoRestrictedArea> current_collisionInfoRestrictedAreaList = allTraj[i].CheckCollisionWithRestrictedArea(allAbsoluteRestrictedAreas[j]);

                if (current_collisionInfoRestrictedAreaList != null && current_collisionInfoRestrictedAreaList.Count > 0)
                {
                    all_collisionInfoRestrictedAreaList.AddRange(current_collisionInfoRestrictedAreaList);
                }
            }
        }
        for (int i = 0; i < all_collisionInfoRestrictedAreaList.Count; i++)
        {
            all_collisionInfoRestrictedAreaList[i].segment.SetIsCollided(true, true);
            all_collisionInfoRestrictedAreaList[i].restrictedArea.SetIsCollided(true, true);
        }
    }


    public void ClearConflicts()
    {
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

public class CollisionInfo
{
    public CurveSegment segment1;
    public CurveSegment segment2;
}
public class CollisionInfoRestrictedArea
{
    public CurveSegment segment;
    public AbsoluteRestrictedArea restrictedArea;
}

