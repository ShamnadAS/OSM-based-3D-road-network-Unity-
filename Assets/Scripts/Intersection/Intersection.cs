using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Intersection
{
    public float sphereRadius = 0.5f;
    public float bufferSpacing = 20;
    public float bufferOffset = 2;
    public float bufferAngle = 120f;

    //Intersections of the road network
    public List<Vector3> intersections { get; set; }
    //Edge intersections of road sections at each intertersection point
    public Vector3[][] edgesIntersection { get; set; }
    public Vector3[][] intersectionBounds { get; set; }
    public Vector3[][] buffer { get; set; }
    public IntersectingRoadStats[][] intersectingRoads { get; set; }

    private OsmReader osmReader = OsmReader.Instance;

    public void CreateIntersection()
    {
        CalculateIntersection();

        //Initialize fields.
        edgesIntersection = new Vector3[intersections.Count][];
        intersectionBounds = new Vector3[intersections.Count][];
        intersectingRoads = new IntersectingRoadStats[intersections.Count][];
        buffer = new Vector3[intersections.Count][];

        for (int i = 0; i < intersections.Count; i++)
        {
            //Road sections meeting the intersection is identified.
            Collider[] intersectingRoadsCollider = Physics.OverlapSphere(intersections[i], sphereRadius);
            int n = intersectingRoadsCollider.Length;
            
            List<IntersectingRoadStats> roadStatsList = new List<IntersectingRoadStats>();
            //Direction vectors of the intersecting roads
            List<Vector3> dirRoad = new List<Vector3>();
            //Relative angle of the road sections
            List<float> angles = new List<float>();

            for (int j = 0; j < n; j++)
            {
                roadStatsList.Add(new IntersectingRoadStats());
                roadStatsList[roadStatsList.Count - 1].road = intersectingRoadsCollider[j].gameObject;

                var verts = intersectingRoadsCollider[j].gameObject.GetComponent<MeshCollider>().sharedMesh.vertices;
                //Start position of the road section.
                var start = Vector3.Lerp(verts[0], verts[1], 0.5f);
                //End position of the road section.
                var end = Vector3.Lerp(verts[verts.Length - 1], verts[verts.Length - 2], 0.5f);

                //If the road section is looping, both ends of the road is consider as two seperate roads 
                if (start == end)
                {
                    roadStatsList[roadStatsList.Count - 1].startCloseToIntersection = true;
                    dirRoad.Add((start - Vector3.Lerp(verts[2], verts[3], 0.5f)).normalized);
                    angles.Add(Utilities.DirAngle(dirRoad[0], dirRoad[dirRoad.Count - 1]));

                    //New road is created with startCloseToIntersection opposite to the exisiting road.
                    roadStatsList.Add(new IntersectingRoadStats());
                    roadStatsList[roadStatsList.Count - 1].road = intersectingRoadsCollider[j].gameObject;
                    roadStatsList[roadStatsList.Count - 1].startCloseToIntersection = false;
                    dirRoad.Add((end - Vector3.Lerp(verts[verts.Length - 3], verts[verts.Length - 4], 0.5f)).normalized);
                    angles.Add(Utilities.DirAngle(dirRoad[0], dirRoad[dirRoad.Count - 1]));
                }
                else
                {
                    int startingIndex = (start - intersections[i]).sqrMagnitude > (end - intersections[i]).sqrMagnitude ?
                                        verts.Length - 1 : 0;

                    if (startingIndex == 0)
                    {
                        roadStatsList[roadStatsList.Count - 1].startCloseToIntersection = true;
                        dirRoad.Add((start - Vector3.Lerp(verts[2], verts[3], 0.5f)).normalized);
                    }
                    else
                    {
                        roadStatsList[roadStatsList.Count - 1].startCloseToIntersection = false;
                        dirRoad.Add((end - Vector3.Lerp(verts[verts.Length - 3], verts[verts.Length - 4], 0.5f)).normalized);
                    }

                    angles.Add(Utilities.DirAngle(dirRoad[0], dirRoad[dirRoad.Count - 1]));
                }
            }

            intersectingRoads[i] = roadStatsList.ToArray();
            n = intersectingRoads[i].Length;

            //Sort intersecting roads based on angle
            for (int j = 0; j < n - 1; j++)
            {
                for (int k = 0; k < n - j - 1; k++)
                {
                    if (angles[k] > angles[k + 1])
                    {
                        float tempAngle = angles[k];
                        var tempStats = intersectingRoads[i][k];

                        angles[k] = angles[k + 1];
                        intersectingRoads[i][k] = intersectingRoads[i][k + 1];

                        angles[k + 1] = tempAngle;
                        intersectingRoads[i][k + 1] = tempStats;
                    }
                }
            }

            //All the points needed to generate the intersection mesh are calculated
            edgesIntersection[i] = CalculateEdgeIntersections(intersectingRoads[i]);
            intersectionBounds[i] = CalculateIntersectionBounds(intersectingRoads[i], edgesIntersection[i], intersections[i]);
            buffer[i] = CalculateBuffer(edgesIntersection[i], intersectionBounds[i]);
        }
    }

    private void CalculateIntersection()
    {
        var ways = osmReader.ways;
        var nodes = osmReader.nodes;
        var center = osmReader.bounds.Centre;

        intersections = new List<Vector3>();

        foreach (var w in ways)
        {
            foreach (var n in w.NodeIDs)
            {
                if (nodes[n].NodeCount > 1 && !intersections.Contains(nodes[n] - center))
                {
                    intersections.Add(nodes[n] - center);
                }
            }
        }
    }

    private Vector3[] CalculateIntersectionBounds(IntersectingRoadStats[] roads, Vector3[] edgeIntersection, Vector3 intersection)
    {
        int n = roads.Length;
        Vector3[] points = new Vector3[2 * n];

        int pointsIndex = 0;

        for (int i = 0; i < n; i++)
        {
            int numVertsToRemove = 0;

            //Edge intersection points on the road section
            Vector3 a = edgeIntersection[i];
            Vector3 b = edgeIntersection[(i + 1) % n];

            //Position of the intersecting point relative to the line ab.
            bool intersectionRelativePosition = Utilities.PointRelativePositon(a, b, intersection);

            Vector3[] verts = roads[i].road.GetComponent<MeshFilter>().sharedMesh.vertices;
            bool startCloseToIntersection = roads[i].startCloseToIntersection;

            if (startCloseToIntersection)
            {
                for (int j = 0; j < verts.Length; j += 2)
                {
                    //Select the points if both the vertices are on the opposite side of the intersection
                    //Relative to the line ab
                    if (Utilities.PointRelativePositon(a, b, verts[j]) != intersectionRelativePosition &&
                       Utilities.PointRelativePositon(a, b, verts[j + 1]) != intersectionRelativePosition)
                    {
                        points[pointsIndex] = verts[j];
                        points[pointsIndex + 1] = verts[j + 1];
                        pointsIndex += 2;
                        break;
                    }
                    numVertsToRemove += 2;
                }
            }
            else
            {
                for (int j = verts.Length - 1; j >= 0; j -= 2)
                {
                    if (Utilities.PointRelativePositon(a, b, verts[j]) != intersectionRelativePosition &&
                       Utilities.PointRelativePositon(a, b, verts[j - 1]) != intersectionRelativePosition)
                    {
                        points[pointsIndex] = verts[j];
                        points[pointsIndex + 1] = verts[j - 1];
                        pointsIndex += 2;
                        break;
                    }
                    numVertsToRemove += 2;
                }
            }
            roads[i].numVertsToRemove = numVertsToRemove;
        }

        return points;
    }

    private Vector3[] CalculateBuffer(Vector3[] points, Vector3[] bounds)
    {
        List<Vector3> vertsList = new List<Vector3>();

        int boundIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 startPos = (bounds[((bounds.Length - 1) + boundIndex) % bounds.Length] - points[i]).normalized;
            Vector3 endPos = (bounds[(bounds.Length + boundIndex) % bounds.Length] - points[i]).normalized;
            float angle = Vector3.Angle(startPos, endPos);

            if (angle < bufferAngle)
            {
                vertsList.Add(points[i]);

                float radius = Mathf.Tan((angle / 2) * Mathf.Deg2Rad) * bufferOffset;
                float h = bufferOffset / Mathf.Cos((angle / 2) * Mathf.Deg2Rad);
                Vector3 center = points[i] + (startPos + endPos).normalized * h;

                Vector3 arcStart = (startPos * bufferOffset + points[i]) - center;
                Vector3 arcEnd = (endPos * bufferOffset + points[i]) - center;

                //0 is counterClockwise, 1 is clockwise
                int direction = Vector3.SignedAngle(arcStart, arcEnd, Vector3.down) < 0 ? 0 : 1;
                float startAngle = Vector3.SignedAngle(Vector3.right, arcStart, Vector3.down);
                float deltaAngle = bufferSpacing / radius;
                float arcAngle = 180 - angle;

                float t = startAngle;

                //Discreate points calculated on the arc
                while (direction > 0 ? t < startAngle + arcAngle : t > startAngle - arcAngle)
                {
                    float x = radius * Mathf.Cos(t * Mathf.Deg2Rad);
                    float y = radius * Mathf.Sin(t * Mathf.Deg2Rad);

                    Vector3 point = new Vector3(x, 0, y) + center;
                    vertsList.Add(point);

                    t = direction > 0 ? t += deltaAngle : t -= deltaAngle;
                }

                vertsList.Add(arcEnd + center);
            }

            boundIndex += 2;
        }

        return vertsList.ToArray();
    }

    private Vector3[] CalculateEdgeIntersections(IntersectingRoadStats[] roads)
    {
        int n = roads.Length;

        Vector3[] points = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            int r1 = (n + i - 1) % n;
            int r2 = (n + i) % n;

            //Adjacent roads 
            var firstRoad = roads[r1].road.GetComponent<MeshFilter>().sharedMesh.vertices;
            var secondRoad = roads[r2].road.GetComponent<MeshFilter>().sharedMesh.vertices;

            //Indexing of the road vertices is odd on one edge along the lenght and even on the other.
            //Intersecting edges are identified based on the ordering of the roads.
            Vector3 a = roads[r1].startCloseToIntersection ? firstRoad[1] : firstRoad[firstRoad.Length - 2];
            Vector3 b = roads[r1].startCloseToIntersection ? firstRoad[3] : firstRoad[firstRoad.Length - 4];
             
            Vector3 c = roads[r2].startCloseToIntersection ? secondRoad[0] : secondRoad[secondRoad.Length - 1];
            Vector3 d = roads[r2].startCloseToIntersection ? secondRoad[2] : secondRoad[secondRoad.Length - 3];

            //If the lines are parellel
            if((a - b).normalized == (c - d).normalized || -(a - b).normalized == (c - d).normalized)
            {
                //To be implemented
            }
            else
            {
                points[i] = Utilities.LineIntersection(a, b, c, d); 
            }
        }

        return points;
    }
}

public class IntersectingRoadStats
{
    //Road section game object
    public GameObject road { get; set; }
    //True if start is close to the intersection
    public bool startCloseToIntersection { get; set; }
    //Number of vertices to remove to avoid overlapping.
    public int numVertsToRemove { get; set; }
}
