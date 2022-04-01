using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private Path path;
    private PathCreator creator;
    private int selected = 0;

    private void OnEnable()
    {
        creator = (PathCreator)target;

        if (creator.path == null)
        {
            creator.CreatePath();
        }

        path = creator.path;
    }

    private void OnSceneGUI()
    {
        if (creator.path.pathSections != null)
        {
            Draw();
            //DrawOsm();
            //DebugDraw();
        }
    }

    private void DebugDraw()
    {
        var ways = OsmReader.Instance.ways;
        var nodes = OsmReader.Instance.nodes;
        var center = OsmReader.Instance.bounds.Centre;

        foreach (var n in nodes)
        {
            Handles.color = Color.cyan;
            if(n.Value.ID == 1636777002)
            Handles.FreeMoveHandle(n.Value - center, Quaternion.identity, 2, Vector2.zero, Handles.CylinderHandleCap);
        }
    }

    private void DrawOsm()
    {
        if (OsmReader.Instance.nodes != null)
        {
            var ways = OsmReader.Instance.ways;
            var nodes = OsmReader.Instance.nodes;
            var center = OsmReader.Instance.bounds.Centre;

            foreach (var n in nodes)
            {
                Handles.FreeMoveHandle(n.Value - center, Quaternion.identity, 1f, Vector2.zero, Handles.CylinderHandleCap);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                Handles.Label(n.Value - center, String.Format("(" + n.Value.NodeCount + ")"), style);
            }

            foreach (var w in ways)
            {
                for (int i = 0; i < w.NodeIDs.Count - 1; i++)
                {
                    Handles.DrawLine(nodes[w.NodeIDs[i]] - center, nodes[w.NodeIDs[i + 1]] - center);
                }
            }
        }
    }

    private void Draw()
    {
        for (int i = 0; i < path.pathSections.Count; i++)
        {
            var section = path.pathSections[i];

            for (int j = 0; j < path.NumSegments(i); j++)
            {
                Vector3[] points = path.GetPointsInSegment(i, j);
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
                Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
            }

            for (int j = 1; j < section.Count; j++)
            {
                Handles.color = j % 3 == 0 ? Color.red : Color.white;
                Vector2 newPos = Handles.FreeMoveHandle(section[j], Quaternion.identity, .075f, Vector2.zero, Handles.CylinderHandleCap);
                //Handles.Label(section[j], j.ToString());
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        selected = EditorGUILayout.Popup("Select Map", selected, ResourceFiles());

        if (GUILayout.Button("Load map"))
        {
            OsmReader.Instance.resourceFile = ResourceFiles()[selected];
            OsmReader.Instance.ReadMap();
            path.CreatePath();
            SceneView.RepaintAll();
        }
    }
    public string[] ResourceFiles()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "\\Resources");
        List<string> files = new List<string>();
        
        var filesInfo = dir.GetFiles().Where(x => x.Extension == ".txt");

        foreach (var fi in filesInfo)
        {
            files.Add(System.IO.Path.GetFileNameWithoutExtension(fi.Name));
        }

        return files.ToArray();
    }
}
