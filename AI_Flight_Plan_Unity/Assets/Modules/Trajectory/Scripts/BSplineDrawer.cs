using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BSplineDrawer : MonoBehaviour
{
    [field: SerializeField] public Transform waypointContainer { get; private set; }
    [SerializeField] private int segmentCount=100;
    [SerializeField, HideInInspector] private List<Waypoint> waypointList = new List<Waypoint>();
    [SerializeField, HideInInspector] private Vector3[] waypointPositionList;
    [SerializeField, HideInInspector] private LineRenderer lineRenderer;
    void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }
    public void SetWaypoints(List<Waypoint> _waypoints)
    {
        waypointList = _waypoints;
        foreach (Waypoint waypoint in _waypoints)
        {
            waypoint.transform.SetParent(waypointContainer);
        }
    }
    public void SetWaypointContainer(Transform _container)
    {
        waypointContainer =_container;
    }
    public void Create()
    {
        DrawCurve(waypointPositionList, lineRenderer, segmentCount);
    }
    public void Clear()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }


    public void DrawCurve(Vector3[] points, LineRenderer lineRenderer, int segmentCount)
    {
        // rebuild control points from waypointContainer (clear previous list)
        waypointList.Clear();
        int ct = 0;
        var pts = new List<Vector3>();
        if (waypointContainer != null)
        {
            for (int i = 0; i < waypointContainer.childCount; i++)
            {
                Transform child = waypointContainer.GetChild(i);
                var waypoint = child.GetComponent<Waypoint>();
                if (waypoint != null)
                {
                    waypointList.Add(waypoint);
                    pts.Add(child.position);
                    ct++;
                }
            }
        }

        if (pts.Count == 0)
        {
            lineRenderer.positionCount = 0;
            waypointPositionList = Array.Empty<Vector3>();
            return;
        }

        waypointPositionList = pts.ToArray();

        // need at least degree+1 control points for cubic B-spline
        int degree = 3;
        if (waypointPositionList.Length <= degree)
        {
            // fallback: draw straight polyline through control points
            lineRenderer.positionCount = waypointPositionList.Length;
            for (int i = 0; i < waypointPositionList.Length; i++)
                lineRenderer.SetPosition(i, waypointPositionList[i]);
            return;
        }

        segmentCount = Mathf.Max(1, segmentCount);
        lineRenderer.positionCount = segmentCount + 1;
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 position = DeBoorCox(waypointPositionList, t, degree);
            lineRenderer.SetPosition(i, position);
        }
    }

    /// <summary>
    /// De Boor algorithm for clamped uniform B-spline
    /// points = control points (P0..Pn)
    /// t in [0,1]
    /// degree = spline degree (e.g. 3 for cubic)
    /// </summary>
    private Vector3 DeBoorCox(Vector3[] points, float t, int degree)
    {
        int n = points.Length - 1;
        int p = degree;
        int m = n + p + 1; // highest knot index
        // build clamped uniform knot vector in [0,1]
        float[] knots = new float[m + 1];
        for (int i = 0; i <= m; i++)
        {
            if (i <= p) knots[i] = 0f;
            else if (i >= m - p) knots[i] = 1f;
            else knots[i] = (float)(i - p) / (float)(m - 2 * p);
        }

        // clamp t into [0,1]
        t = Mathf.Clamp01(t);

        // find knot span k such that knots[k] <= t < knots[k+1]
        int k = p; // default
        if (t >= 1f - Mathf.Epsilon)
        {
            k = m - p - 1; // last valid span
        }
        else
        {
            for (int i = p; i <= m - p - 1; i++)
            {
                if (t >= knots[i] && t < knots[i + 1])
                {
                    k = i;
                    break;
                }
            }
        }

        // initialize d[0..p] = P_{k-p} .. P_{k}
        Vector3[] d = new Vector3[p + 1];
        for (int j = 0; j <= p; j++)
        {
            int idx = k - p + j;
            idx = Mathf.Clamp(idx, 0, n);
            d[j] = points[idx];
        }

        // de Boor recursion
        for (int r = 1; r <= p; r++)
        {
            for (int j = p; j >= r; j--)
            {
                float denom = knots[k + j - r + 1] - knots[k - p + j];
                float alpha = 0f;
                if (Mathf.Abs(denom) > Mathf.Epsilon)
                    alpha = (t - knots[k - p + j]) / denom;
                d[j] = (1f - alpha) * d[j - 1] + alpha * d[j];
            }
        }

        return d[p];
    }

}
