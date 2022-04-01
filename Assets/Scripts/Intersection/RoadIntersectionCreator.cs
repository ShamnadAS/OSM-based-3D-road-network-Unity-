using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(IntersectionCreator))]
public class RoadIntersectionCreator : MonoBehaviour
{
    [HideInInspector]
    public bool generateIntersection = false;
    public Material material;

    public void GenerateIntersection()
    {
        Intersection intersection = GetComponent<IntersectionCreator>().intersection;
        IntersectingRoadStats[][] intersectingRoads = intersection.intersectingRoads;
        Vector3[][] edgesIntersection = intersection.edgesIntersection;
        Vector3[][] intersectionBounds = intersection.intersectionBounds;
        List<Vector3> intersections = intersection.intersections;
        Vector3[][] buffer = intersection.buffer;

        GameObject intersectionParent = new GameObject();
        intersectionParent.name = "Intersections";
        intersectionParent.transform.parent = gameObject.transform;

        for (int i = 0; i < intersections.Count; i++)
        {
            RemoveOverlapping(intersectingRoads[i]);

            var obj = new GameObject();
            obj.transform.parent = intersectionParent.transform;
            obj.name = "Intersection " + i;
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();

            obj.GetComponent<MeshFilter>().mesh = CreateIntersectionMesh(edgesIntersection[i], intersectionBounds[i], intersections[i]);
            obj.GetComponent<MeshRenderer>().material = material;

            var obj2 = new GameObject();
            obj2.transform.parent = intersectionParent.transform;
            obj2.name = "Buffer " + i;
            obj2.AddComponent<MeshFilter>();
            obj2.AddComponent<MeshRenderer>();

            obj2.GetComponent<MeshFilter>().mesh = CreateBufferMesh(edgesIntersection[i], buffer[i]);
            obj2.GetComponent<MeshRenderer>().material = material;
        }
    }

    Mesh CreateIntersectionMesh(Vector3[] points, Vector3[] bounds, Vector3 intersection)
    {
        Vector3[] verts = new Vector3[points.Length + bounds.Length + 1];
        int[] tris = new int[points.Length * 9];
        verts[verts.Length - 1] = intersection;

        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            verts[i] = points[i];

            tris[triIndex] = i;
            tris[triIndex + 1] = (i + 1) % points.Length;
            tris[triIndex + 2] = verts.Length - 1;

            triIndex += 3;
        }

        for (int i = 0; i < bounds.Length; i++)
        {
            verts[i + points.Length] = bounds[i];
        }

        int boundIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            tris[triIndex] = i;
            tris[triIndex + 1] = points.Length + boundIndex;
            tris[triIndex + 2] = points.Length + boundIndex + 1;

            tris[triIndex + 3] = i;
            tris[triIndex + 4] = points.Length + boundIndex + 1;
            tris[triIndex + 5] = (i + 1) % points.Length;

            boundIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;

        return mesh;
    }
     
    Mesh CreateBufferMesh(Vector3[] points, Vector3[] bufferPoints)
    {
        //verts 'equals' bufferPoints
        List<int> tris = new List<int>();

        int activePointIndex = 0;

        for (int i = 1; i < bufferPoints.Length - 1; i++)
        {
            if (points.Contains(bufferPoints[i]))
            {
                activePointIndex = i;
            }
            else if(!points.Contains(bufferPoints[i + 1]))
            {
                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(activePointIndex);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = bufferPoints;
        mesh.triangles = tris.ToArray();

        return mesh;
    }

    void RemoveOverlapping(IntersectingRoadStats[] intersectingRoads)
    {
        for (int i = 0; i < intersectingRoads.Length; i++)
        {
            int numVertsToRemove = intersectingRoads[i].numVertsToRemove;
            bool startCloseToIntersection = intersectingRoads[i].startCloseToIntersection;

            List<int> trisList = intersectingRoads[i].road.GetComponent<MeshFilter>().sharedMesh.triangles.ToList();

            if (trisList.Count - 3 * numVertsToRemove >= 0)
            {
                if (startCloseToIntersection)
                {
                    trisList.RemoveRange(0, 3 * numVertsToRemove);
                }
                else
                {
                    trisList.RemoveRange(trisList.Count - 3 * numVertsToRemove, 3 * numVertsToRemove);
                } 
            }

            intersectingRoads[i].road.GetComponent<MeshFilter>().sharedMesh.triangles = trisList.ToArray();
        }

    }
}
