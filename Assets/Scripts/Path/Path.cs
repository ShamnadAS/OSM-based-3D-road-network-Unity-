using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<List<Vector3>> pathSections { get; set; }
    OsmReader osmReader = OsmReader.Instance;

    public void CreatePath()
    {
        pathSections = new List<List<Vector3>>();

        var ways = osmReader.ways;
        var nodes = osmReader.nodes;
        var center = osmReader.bounds.Centre;

        foreach (var w in ways)
        {
            if (w.IsRoad)
            {
                var nodeIds = w.NodeIDs;
                //Points on the section
                var points = new List<Vector3>();

                int startIndex = 0;

                //Check if the way is looping and the start (or end) is not at an intersection.
                if (nodeIds[0] == nodeIds[nodeIds.Count - 1] && nodes[nodeIds[0]].NodeCount == 1)
                {
                    for (int i = 1; i < nodeIds.Count - 1; i++)
                    {
                        if (nodes[nodeIds[i]].NodeCount > 1)
                        {
                            //Move the start (or end) of the loop to an intersection on that loop
                            startIndex = i;
                            break;
                        }
                    }
                }

                //Initialize first spline segment of the way
                InitializeFirstSegment(nodeIds[startIndex], nodeIds[startIndex + 1], points, nodes);

                for (int i = startIndex + 1; i < nodeIds.Count - 1 + startIndex; i++)
                {
                    int index = i % (nodeIds.Count - 1);

                    //If the node is an intersection, Start a new road section from there on.
                    if (nodes[nodeIds[index]].NodeCount > 1)
                    {
                        pathSections.Add(points);
                        points = new List<Vector3>();
                        InitializeFirstSegment(nodeIds[index], nodeIds[index + 1], points, nodes);
                    }
                    else
                    {
                        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                        points.Add((points[points.Count - 1] + (nodes[nodeIds[index + 1]]) - center) * .5f);
                        points.Add(nodes[nodeIds[index + 1]] - center);
                    }
                }

                pathSections.Add(points);
            }
        }

        //Autoset control points of the spline
        for (int i = 0; i < pathSections.Count; i++)
        {
            for (int j = 0; j < pathSections[i].Count; j += 3)
            {
                AutoSetAnchorControlPoints(i, j);
            }

            AutoSetStartAndEndControls(i);
        }
    }
    private void InitializeFirstSegment(ulong s, ulong v, List<Vector3> points, Dictionary<ulong, OsmNode> nodes)
    {
        var center = osmReader.bounds.Centre;
        points.Add(nodes[s] - center);
        points.Add((nodes[s] - center) + (Vector3.forward + Vector3.right) * 0.5f);
        points.Add((nodes[v] - center) + (Vector3.back + Vector3.left) * 0.5f);
        points.Add(nodes[v] - center);
    }

    public Vector3[] CalculateEvenlySpacedPoints(int section, float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        var points = pathSections[section];

        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments(section); segmentIndex++)
        {
            Vector3[] p = GetPointsInSegment(section, segmentIndex);
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        if ((evenlySpacedPoints[evenlySpacedPoints.Count - 1] - points[points.Count - 1]).magnitude > 0.1)
        {
            evenlySpacedPoints.Add(points[points.Count - 1]);
        }

        return evenlySpacedPoints.ToArray();
    }
    public int NumSegments(int i)
    {
        return (pathSections[i].Count - 4) / 3 + 1;
    }
    public Vector3[] GetPointsInSegment(int currentSegment, int i)
    {
        var points = pathSections[currentSegment];

        return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }
    private void AutoSetAnchorControlPoints(int section, int anchorIndex)
    {
        List<Vector3> points = pathSections[section];
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0)
        {
            Vector3 offset = points[anchorIndex - 3] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude * 0.25f;
        }
        if (anchorIndex + 3 < points.Count)
        {
            Vector3 offset = points[anchorIndex + 3] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude * 0.25f;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count)
            {
                points[controlIndex] = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }
    private void AutoSetStartAndEndControls(int section)
    {
        List<Vector3> points = pathSections[section];

        points[1] = (points[0] + points[2]) * 0.5f;
        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
    }
}
