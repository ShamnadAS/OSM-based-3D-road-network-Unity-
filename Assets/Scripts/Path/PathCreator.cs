using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;
    public void CreatePath()
    {
        path = new Path();
    }

    
}
