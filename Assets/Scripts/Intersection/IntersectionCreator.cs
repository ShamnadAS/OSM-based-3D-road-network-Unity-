using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionCreator : MonoBehaviour
{
    [HideInInspector]
    public Intersection intersection;

    public void CreateIntersection()
    {
        intersection = new Intersection();
    }
}
