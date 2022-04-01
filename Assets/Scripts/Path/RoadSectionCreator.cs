using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadSectionCreator : MonoBehaviour
{
    [Range(.05f, 1.5f)]
    public float spacing = 1;
    public float roadWidth = 1;
    public float tiling = 1;
    public Texture roadTexture;
    [HideInInspector]
    public bool generatePath = false;

    public void GenerateRoad()
    {
        Path path = GetComponent<PathCreator>().path;

        //gameObject "Sections" is created to organize the road sections.
        var sectionParent = new GameObject();
        sectionParent.name = "Sections";
        sectionParent.transform.parent = gameObject.transform;

        for (int i = 0; i < path.pathSections.Count; i++)
        {
            Vector3[] points = path.CalculateEvenlySpacedPoints(i,spacing);
            var obj = new GameObject();
            obj.name = "Section " + i ;
            obj.transform.parent = sectionParent.transform;

            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            var meshcoll = obj.AddComponent<MeshCollider>();
            meshcoll.sharedMesh = CreateRoadMesh(points);
            obj.GetComponent<MeshFilter>().mesh = CreateRoadMesh(points);
            
            //Material of the road section
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetTexture("_MainTex", roadTexture);
            obj.GetComponent<MeshRenderer>().material = mat;

            int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * .05f);
            obj.GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        }
    }

    private Mesh CreateRoadMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];

        int[] tris = new int[2*(points.Length -1) * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1)
            {
                forward += points[i + 1] - points[i];
            }
            if (i > 0)
            {
                forward += points[i] - points[i - 1];
            }

            forward.Normalize();
            Vector3 left = new Vector3(-forward.z, 0, forward.x);

            verts[vertIndex] = points[i] + left * roadWidth * .5f;
            verts[vertIndex + 1] = points[i] - left * roadWidth * .5f;

            float completionPercent = i / (float)(points.Length - 1);

            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 2;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
}