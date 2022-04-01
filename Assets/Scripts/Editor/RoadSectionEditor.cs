using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSectionCreator))]
public class RoadSectionEditor : Editor
{
    private RoadSectionCreator creator;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        GUILayout.Label("Delete existing roads and intersections to generate", style);

        GUI.enabled = creator.transform.childCount == 0 ? true : false;

        if (GUILayout.Button("Generate Path"))
        {
            creator.GenerateRoad();
        }
    }

    private void OnEnable()
    {
        creator = (RoadSectionCreator)target;
    }
}