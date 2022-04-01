using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadIntersectionCreator))]
public class RoadIntersectionEditor : Editor
{
    private RoadIntersectionCreator creator;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        GUILayout.Label("First generate Path to generate intersection", style);

        GUI.enabled = creator.transform.childCount == 1 ? true : false;

        if (GUILayout.Button("Generate Intersection"))
        {
            creator.GenerateIntersection();
        }
    }

    private void OnEnable()
    {
        creator = (RoadIntersectionCreator)target;
    }
}
