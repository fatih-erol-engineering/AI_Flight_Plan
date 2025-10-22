using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class BSplineDrawer : MonoBehaviour
{
    [field: SerializeField] public Transform controlPointContainer { get; private set; }
    [field: SerializeField] public GameObject controlPointPrefab { get; private set; }
    [field: SerializeField] public Waypoint waypointStart { get; private set; }
    [field: SerializeField] public Waypoint waypointEnd { get; private set; }        
    [SerializeField] public TimeGame startTime { get => waypointStart.time;}
    [SerializeField] public TimeGame endTime { get => waypointEnd.time;}
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField, HideInInspector] private ControlPoint[] controlPoints;
    [SerializeField] private int segmentCount = 32;
    [SerializeField, HideInInspector] private LineRenderer lineRenderer;
    [SerializeField, HideInInspector] private Transform[] points;
    [SerializeField] private Transform tube;
    public void AssignData()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        lineRenderer.colorGradient = new Gradient();

        int ct = 0;
        controlPoints = new ControlPoint[controlPointContainer.childCount];
        if (controlPointContainer != null)
        {
            for (int i = 0; i < controlPointContainer.childCount; i++)
            {
                Transform child = controlPointContainer.GetChild(i);
                var controlPoint = child.GetComponent<ControlPoint>();
                if (controlPoint != null)
                {
                    controlPoints[ct] = controlPoint;
                    ct++;
                }
            }
        }
        points = new Transform[controlPoints.Length + 2]; // 2 comes from start and end waypoints
        points[0] = waypointStart.transform;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            points[i + 1] = controlPoints[i].transform;
        }
        points[points.Length - 1] = waypointEnd.transform;
    }
    public void UpdateColor()
    {
        if (!lineRenderer) return;

        int n = 2;
        if (n < 2) n = 2;

        var cKeys = new GradientColorKey[n];
        var aKeys = new GradientAlphaKey[n];

        // HDR yoğunluk: RGB’yi çarpıyoruz, alpha’yı ayrı yönetiyoruz
        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 1f : (float)i / (n - 1);
            Color c = Color.Lerp(startColor, endColor, t);
            c = new Color(c.r, c.g, c.b, 1f);

            float a = Mathf.Lerp(startColor.a, endColor.a, t);
            
            cKeys[i] = new GradientColorKey(c, t);
            aKeys[i] = new GradientAlphaKey(a, t);
        }

        var g = new Gradient { mode = GradientMode.Blend };
        try
        {
            g.SetKeys(cKeys, aKeys);            
        }
        catch (System.Exception)
        {            
            throw;
        }
        lineRenderer.colorGradient = g;

    }

    public void SetStartColor(Color _color)
    {
        startColor = _color;
        UpdateColor();
    }
    public void SetEndColor(Color _color)
    {
        endColor = _color;
        UpdateColor();
    }
    public void UpdateCurve()
    {
        DrawCurve(points, lineRenderer, segmentCount);
    }
    public void SetStartWaypoint(Waypoint _waypoint)
    {
        waypointStart = _waypoint;
    }
    public void SetEndWaypoint(Waypoint _waypoint)
    {
        waypointEnd = _waypoint;
    }
    public void SetControlPoints(Vector3[] _controlPointPositions)
    {
        DeleteControlPoints();
        controlPoints = new ControlPoint[_controlPointPositions.Length];
        int ct = 0;
        foreach (Vector3 _controlPointPosition in _controlPointPositions)
        {
            ControlPoint controlPoint = Instantiate(controlPointPrefab, _controlPointPosition, Quaternion.identity, controlPointContainer).GetComponent<ControlPoint>();
            controlPoints[ct] = controlPoint;
            ct++;
        }
    }
    
     public void SetControlPoints(ControlPoint[] _controlPoints)
    {
        DeleteControlPoints();
        int ct = 0;
        controlPoints = new ControlPoint[_controlPoints.Length];
        foreach (ControlPoint _controlPoint in _controlPoints)
        {            
            _controlPoint.transform.parent = controlPointContainer;
            controlPoints[ct] = _controlPoint;
            ct++;
        }
    }
    
    
    
    public void DeleteControlPoints()
    {
        foreach (Transform child in controlPointContainer)
        {
            var controlPoint = child.GetComponent<ControlPoint>();
            if (controlPoint != null)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(controlPoint);
#else
                Destroy(controlPoint);
#endif
            }
        }
        controlPoints = new ControlPoint[0];
    }

    public void Create()
    {
        AssignData();
        DrawCurve(points, lineRenderer, segmentCount);
    }
    public void Clear()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }


    public void DrawCurve(Transform[] _points, LineRenderer lineRenderer, int segmentCount)
    {
        // need at least degree+1 control points for cubic B-spline
        int degree = 2;
        if (_points.Length <= degree)
        {
            // fallback: draw straight polyline through control points
            lineRenderer.positionCount = _points.Length;
            for (int i = 0; i < _points.Length; i++)
                lineRenderer.SetPosition(i, _points[i].transform.position);
            return;
        }

        segmentCount = Mathf.Max(1, segmentCount);
        lineRenderer.positionCount = segmentCount + 1;
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 position = DeBoorCox(_points, t, degree);
            lineRenderer.SetPosition(i, position);
        }
    }

    /// <summary>
    /// De Boor algorithm for clamped uniform B-spline
    /// points = control points (P0..Pn)
    /// t in [0,1]
    /// degree = spline degree (e.g. 3 for cubic)
    /// </summary>
    private Vector3 DeBoorCox(Transform[] _points, float t, int degree)
    {
        int n = _points.Length - 1;
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
            d[j] = _points[idx].transform.position;
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
