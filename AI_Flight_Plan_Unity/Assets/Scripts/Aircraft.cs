using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;


public class Aircraft : SelectableMonoBehaviour
{
    public AircraftSpec spec;
    public Trajectory trajectory;
    public Theme theme;
    public GameObject aircraftVisualObject;
    protected MeshRenderer[] aircraftMeshRenderers;

    protected void OnEnable()
    {
        if (aircraftMeshRenderers == null)
        {
            aircraftVisualObject = transform.Find("Aircraft").gameObject;
        }
        aircraftMeshRenderers = aircraftVisualObject.GetComponentsInChildren<MeshRenderer>();
    }
    public void MoveAircraftWithTime(float sec)
    {
        int ct = 0;
        foreach (BSplineSegment segment in trajectory.bSplineSegments)
        {
            float startTime_s = segment.startPoint.time.second;
            float endTime_s = segment.endPoint.time.second;

            if ((sec <= endTime_s) && (sec >= startTime_s))
            {
                int n = segment.lr.positionCount;
                float lerpVal = (sec - startTime_s) / (endTime_s - startTime_s);
                lerpVal = Mathf.Clamp(lerpVal, 0, 1);
                float currentIdxFloat = Mathf.Lerp(0, n - 1, lerpVal);
                int currentIdx = Mathf.RoundToInt(currentIdxFloat);
                aircraftVisualObject.transform.position = segment.lr.GetPosition(currentIdx);
                break;
            }
            ct++;
        }
    }

    public Waypoint CreateWaypoint(Vector3 globalPosition)
    {
        if (trajectory == null)
        {
            GameObject trajParentGO = Instantiate(theme.trajectoryPrefab, transform.position, transform.rotation, this.transform);

            trajectory = trajParentGO.GetComponent<Trajectory>();
            if (trajectory == null)
            {
                trajectory = trajParentGO.AddComponent<Trajectory>();
                trajectory.theme = theme;
            }
        }
        Waypoint wp = trajectory.CreateWaypoint(globalPosition);
        return wp;
    }
    public Waypoint CreateWaypoint(Vector3 globalPosition, float time_s)
    {
        if (trajectory == null)
        {
            GameObject trajParentGO = Instantiate(theme.trajectoryPrefab, transform.position, transform.rotation, this.transform);
            trajectory = trajParentGO.GetComponent<Trajectory>();
            if (trajectory == null)
            {
                trajectory = trajParentGO.AddComponent<Trajectory>();
                trajectory.theme = theme;
            }
        }
        Waypoint wp = trajectory.CreateWaypoint(globalPosition, time_s);
        return wp;
    }
    public void UpdateColor(Color color)
    {
        foreach (MeshRenderer renderer in aircraftMeshRenderers)
        {
            renderer.material.color = color;
        }
    }
    public void UpdateMaterial(Material material)
    {
        foreach (MeshRenderer renderer in aircraftMeshRenderers)
        {
            renderer.material = material;
            if (renderer.materials.Length>1)
            {
                var mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = material;
                renderer.materials = mats;                  
            }        
        }
    }



}

