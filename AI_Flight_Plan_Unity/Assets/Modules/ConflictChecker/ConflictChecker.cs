using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Barracuda;

public class ConflictChecker : MonoBehaviour
{
    [SerializeField] private AircraftFactory aircraftFactory;
    public List<CollisionInfo> all_collisionInfoList = new List<CollisionInfo>();
    public List<CollisionInfoRestrictedArea> all_collisionInfoRestrictedAreaList = new List<CollisionInfoRestrictedArea>();
    public List<TrajectoryDrawer> allTraj = new List<TrajectoryDrawer>();
    public List<AbsoluteRestrictedArea> allAbsoluteRestrictedAreas = new List<AbsoluteRestrictedArea>();
    private bool _eventsBound;
    void Start()
    {
        CheckConflicts();
    }
    void Awake()
    {
        if (!_eventsBound && GameEvents.Instance != null)
        {
            GameEvents.Instance.OnSplineChanged += OnSplineChanged;
            GameEvents.Instance.OnTrajectoryCreated += OnTrajectoryCreated;
            GameEvents.Instance.OnWaypointTimeChanged += (wp, oldTime) => OnWaypointTimeChanged(wp);
            GameEvents.Instance.OnAbsoluteRestrictedAreaCreated += OnAbsoluteRestrictedAreaCreated;
            _eventsBound = true;
        }
    }
    public void OnTrajectoryCreated(TrajectoryDrawer trajectoryDrawer)
    {
        allTraj.Clear();
        allTraj.AddRange(aircraftFactory.GetAllTrajectories());
        CheckConflicts();
    }
    public void OnWaypointTimeChanged(Waypoint waypoint)
    {
        ClearConflicts();
        CheckConflicts();
    }
    void OnDisable()
    {
        if (_eventsBound && GameEvents.Instance != null)
        {
            GameEvents.Instance.OnSplineChanged -= OnSplineChanged;
            GameEvents.Instance.OnTrajectoryCreated -= OnTrajectoryCreated;
            GameEvents.Instance.OnAbsoluteRestrictedAreaCreated -= OnAbsoluteRestrictedAreaCreated;
            GameEvents.Instance.OnWaypointTimeChanged -= (wp, oldTime) => OnWaypointTimeChanged(wp);
            _eventsBound = false;
        }
    }

    public void SolveConflictsWithRuleBased()
    {
        StopCoroutine(SolveConflictsCoroutine());
        StartCoroutine(SolveConflictsCoroutine());

        StopCoroutine(SolveRestrictedAreaConflictsCoroutine());
        StartCoroutine(SolveRestrictedAreaConflictsCoroutine());
    }

    public void SolveConflictsWithAI()
    {
        StopCoroutine(SolveConflictsCoroutineWithAI());
        StartCoroutine(SolveConflictsCoroutineWithAI());

        StopCoroutine(SolveRestrictedAreaConflictsCoroutineWithAI());
        StartCoroutine(SolveRestrictedAreaConflictsCoroutineWithAI());
    }



    IEnumerator SolveConflictsCoroutineWithAI()
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
                float dist = 1f;
                var s1 = collision.segment2;
                var s2 = collision.segment1;
                Vector3 _dir = CylinderIntersectNormal(s1.tubeManager.GetStartPosition(), s1.tubeManager.GetEndPosition(), s1.tubeManager.GetRadius(), s2.tubeManager.GetStartPosition(), s2.tubeManager.GetEndPosition(), s2.tubeManager.GetRadius());


                float randNum2 = Random.Range(-1f, 1f);
                float randNum3 = Random.Range(-1f, 1f);
                float randNum4 = Random.Range(-1f, 1f);
                Vector3 _randDir1 = (_dir + new Vector3(randNum2, randNum3, randNum4) * 1f).normalized;



                randNum2 = Random.Range(-1f, 1f);
                randNum3 = Random.Range(-1f, 1f);
                randNum4 = Random.Range(-1f, 1f);
                Vector3 _randDir2 = (_dir + new Vector3(randNum2, randNum3, randNum4) * 1f).normalized;

                float randNum = Random.Range(0f, 1f);
                if (randNum < 0.25f)
                {
                    _randDir1 = -_randDir1;
                    _randDir2 = -_randDir2;
                }

                Vector3 deltaPos1 = dist * _randDir1 * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                // Vector3 deltaPos2 = dist * _randDir2 * s2.aircraft.timeOrPositionChangeVal * s2.aircraft.nonEditableOrEditableVal * (-1f);
                Vector3 deltaPos2 = dist * _randDir2 * s2.aircraft.timeOrPositionChangeVal * s2.aircraft.nonEditableOrEditableVal * (-1f);

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

            CheckTrajConflicts(); // güncelle
            iteration++;
            yield return null;
        }

        if (iteration >= maxIterations)
            Debug.LogWarning($"[{GetType().Name}] SolveConflictsCoroutine stopped after {maxIterations} iterations (possible unresolved conflicts).");
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
                float dist = 1f;
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

            CheckTrajConflicts(); // güncelle
            iteration++;
            yield return null;
        }

        if (iteration >= maxIterations)
            Debug.LogWarning($"[{GetType().Name}] SolveConflictsCoroutine stopped after {maxIterations} iterations (possible unresolved conflicts).");
    }
    IEnumerator SolveRestrictedAreaConflictsCoroutine()
    {
        int maxIterations = 400; // güvenlik sınırı
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
                float dist = s2.radius / 500f;


                Vector3 deltaPos1 = s2.transform.position - s1.startPoint.position;
                Vector3 deltaPos2 = s2.transform.position - s1.endPoint.position;
                Vector3 _dir3;
                float dist1 = deltaPos1.magnitude;//Ters oran var
                float dist2 = deltaPos2.magnitude;
                float dist3 = DistancePointToLine(s2.transform.position, s1.startPoint.position, s1.endPoint.position, out _dir3);
                float distTotal = dist1 + dist2;

                float t1 = dist1 / distTotal;
                float t2 = dist2 / distTotal;

                float distUnit1 = dist * t2;
                float distUnit2 = dist * t1;

                Vector3 movePos1 = deltaPos1.normalized * distUnit1 * (-1f) * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                Vector3 movePos2 = deltaPos2.normalized * distUnit2 * (-1f) * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                Vector3 movePos3 = _dir3 * dist * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;

                s1.controlPoint1.SetPosition(s1.controlPoint1.transform.position + movePos3);
                s1.controlPoint2?.SetPosition(s1.controlPoint2.transform.position + movePos3);
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


        IEnumerator SolveRestrictedAreaConflictsCoroutineWithAI()
    {
        int maxIterations = 400; // güvenlik sınırı
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
                float dist = s2.radius / 500f;


                Vector3 deltaPos1 = s2.transform.position - s1.startPoint.position;
                Vector3 deltaPos2 = s2.transform.position - s1.endPoint.position;
                Vector3 _dir3;
                float dist1 = deltaPos1.magnitude;//Ters oran var
                float dist2 = deltaPos2.magnitude;
                float dist3 = DistancePointToLine(s2.transform.position, s1.startPoint.position, s1.endPoint.position, out _dir3);
                float distTotal = dist1 + dist2;

                float t1 = dist1 / distTotal;
                float t2 = dist2 / distTotal;

                float distUnit1 = dist * t2;
                float distUnit2 = dist * t1;

                
                float randNum2 = Random.Range(-1f, 1f);
                float randNum3 = Random.Range(-1f, 1f);
                float randNum4 = Random.Range(-1f, 1f);
                Vector3 _randDir1 = (deltaPos1.normalized + new Vector3(randNum2, randNum3, randNum4).normalized * 5f).normalized;

                
                randNum2 = Random.Range(-1f, 1f);
                randNum3 = Random.Range(-1f, 1f);
                randNum4 = Random.Range(-1f, 1f);
                Vector3 _randDir2 = (deltaPos2.normalized + new Vector3(randNum2, randNum3, randNum4).normalized * 5f).normalized;

                randNum2 = Random.Range(-1f, 1f);
                randNum3 = Random.Range(-1f, 1f);
                randNum4 = Random.Range(-1f, 1f);
                Vector3 _randDir3 = (_dir3.normalized + new Vector3(randNum2, randNum3, randNum4).normalized * 5f).normalized;




                Vector3 movePos1 = _randDir1 * distUnit1 * (-1f) * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                Vector3 movePos2 = _randDir2 * distUnit2 * (-1f) * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;
                Vector3 movePos3 = _randDir3 * dist * s1.aircraft.timeOrPositionChangeVal * s1.aircraft.nonEditableOrEditableVal;

                s1.controlPoint1.SetPosition(s1.controlPoint1.transform.position + movePos3);
                s1.controlPoint2?.SetPosition(s1.controlPoint2.transform.position + movePos3);
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
        GameEvents.Instance.OnTrajectoryCreated -= OnTrajectoryCreated;
        GameEvents.Instance.OnAbsoluteRestrictedAreaCreated -= OnAbsoluteRestrictedAreaCreated;
    }

    public void OnSplineChanged(BSplineDrawer splineDrawer)
    {
        ClearConflicts();
        CheckConflicts();
    }
    public void OnAbsoluteRestrictedAreaCreated(AbsoluteRestrictedAreaFactory _val)
    {
        allAbsoluteRestrictedAreas.Clear();
        allAbsoluteRestrictedAreas.AddRange(_val.GetAllAbsoluteRestrictedAreas());
        CheckConflicts();
    }

    public void CheckConflicts()
    {
        if (allTraj != null)
        {

            foreach (TrajectoryDrawer traj in allTraj)
            {
                if (traj != null)
                {
                    foreach (BSplineDrawer bSplineDrawer in traj.bSplineDrawerArray)
                    {
                        if (bSplineDrawer != null)
                        {
                            foreach (CurveSegment segment in bSplineDrawer.curveSegments)
                            {
                                if (segment != null)
                                {
                                    segment.SetIsCollided(false);
                                }
                            }
                        }
                    }
                }
            }
        }
        CheckTrajConflicts();
        CheckRestrictedAreaConflicts();
    }


    // Update is called once per frame
    public void CheckTrajConflicts()
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
            all_collisionInfoList[i].segment1.SetIsCollided(true, true);
            all_collisionInfoList[i].segment2.SetIsCollided(true, true);
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
            // all_collisionInfoRestrictedAreaList[i].restrictedArea.SetIsCollided(true, true);
        }
    }


    public void ClearConflicts()
    {
    }
    public Vector3 CylinderIntersectNormal(
       Vector3 startA, Vector3 endA, float radiusA,
       Vector3 startB, Vector3 endB, float radiusB)
    {
        // --- 1. Eksen yön vektörlerini ve uzunlukları hesapla
        Vector3 uA = (endA - startA);
        Vector3 uB = (endB - startB);
        uA.Normalize();
        uB.Normalize();
        Vector3 midA = (startA + endA) * 0.5f;
        Vector3 midB = (startB + endB) * 0.5f;

        Vector3 n = Vector3.Cross(uA, uB);
        Vector3 v = midB - midA;
        float d = Vector3.Dot(n, v);
        n *= Mathf.Sign(d) * (-1f);
        if ((midB - midA).magnitude < 0.2f)
        {
            int value = Random.Range(0, 2) * 2 - 1;
            n *= (float)value;
        }
        // --- 2. Eksenler arası en kısa mesafeyi bul
        return n.normalized;
    }

    public static float DistancePointToLine(Vector3 point, Vector3 linePointA, Vector3 linePointB, out Vector3 direction)
    {
        Vector3 lineDir = linePointB - linePointA;
        Vector3 ap = point - linePointA;
        Vector3 cross = Vector3.Cross(lineDir, ap);


        float t = Vector3.Dot(ap, lineDir) / lineDir.sqrMagnitude;
        Vector3 closest = linePointA + t * lineDir;

        direction = (closest - point).normalized;
        return cross.magnitude / lineDir.magnitude;
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

