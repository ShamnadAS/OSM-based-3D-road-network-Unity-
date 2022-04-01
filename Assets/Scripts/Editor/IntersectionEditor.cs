using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IntersectionCreator))]
public class IntersectionEditor : Editor
{
    Intersection intersection;
    IntersectionCreator creator;

    List<Vector3> intersections;

    void OnEnable()
    {
        creator = (IntersectionCreator)target;
        if (creator.intersection == null)
        {
            creator.CreateIntersection();
        }

        intersection = creator.intersection;
    }
    private void OnSceneGUI()
    {
        Draw();
    }

    void Draw()
    {
        if (OsmReader.Instance.nodes != null)
        {
            intersection.CreateIntersection();
            intersections = intersection.intersections;

            for (int i = 0; i < intersections.Count; i++)
            {
                Handles.color = Color.red;
                Handles.FreeMoveHandle(intersections[i], Quaternion.identity, 1f, Vector2.zero, Handles.CylinderHandleCap);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                Handles.Label(intersections[i], i.ToString(), style);
            }

            foreach (var u in intersection.edgesIntersection)
            {
                for (int i = 0; i < u.Length; i++)
                {
                    Handles.color = Color.green;
                    Handles.FreeMoveHandle(u[i], Quaternion.identity, 0.5f, Vector2.zero, Handles.CylinderHandleCap);

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.green;
                    Handles.Label(u[i], i.ToString(), style);
                }
            }

            //foreach (var u in intersection.intersectionBounds)
            //{
            //    foreach (var v in u)
            //    {
            //        Handles.color = Color.yellow;
            //        Handles.FreeMoveHandle(v, Quaternion.identity, 0.5f, Vector2.zero, Handles.CylinderHandleCap);
            //    }
            //}
        }

        //for (int j = 0; j < intersection.intersections.Count; j++)
        //{
        //    var u = intersection.intersectingRoads[j];
        //    var inter = intersection.intersections;

        //    for (int i = 0; i < u.Length; i++)
        //    {
        //        var verts = u[i].road.GetComponent<MeshFilter>().sharedMesh.vertices;
        //        GUIStyle style = new GUIStyle();
        //        style.normal.textColor = Color.red;
        //        Handles.Label((verts[verts.Length / 2] - inter[j]).normalized * 5 + inter[j], i.ToString(), style);
        //    }
        //}

        //for (int i = 0; i < intersections.Count; i++)
        //{
        //    var u2 = intersection.intersectingRoads[i];
        //    var inter = intersection.intersections;

        //    for (int j = 0; j < u2.Length; j++)
        //    {
        //        var verts = u2[j].road.GetComponent<MeshFilter>().sharedMesh.vertices;

        //        for (int k = 0; k < verts.Length; k++)
        //        {
        //            GUIStyle style = new GUIStyle();
        //            style.normal.textColor = Color.red;
        //            Handles.Label(verts[k], k.ToString(), style);
        //        }
        //        Handles.color = Color.cyan;
        //        Handles.FreeMoveHandle(verts[verts.Length - 1], Quaternion.identity, 0.5f, Vector2.zero, Handles.CylinderHandleCap);
        //    }

        //}

    }
}
