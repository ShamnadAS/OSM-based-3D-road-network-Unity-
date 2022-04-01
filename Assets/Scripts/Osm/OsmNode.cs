using System.Xml;
using UnityEngine;

/*
    Copyright (c) 2017 Sloan Kelly
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

public class OsmNode : BaseOsm
{
    //Node ID.
    public ulong ID { get; private set; }
    // Latitude position of the node.
    public float Latitude { get; private set; }
    // Longitude position of the node.
    public float Longitude { get; private set; }
    public float X { get; private set; }
    public float Y { get; private set; }
    //Number of adjacent nodes
    public float NodeCount { get;  set; }
    //Temporary flag to count NodeCount
    public bool EndFlag { get; set; }

    public static implicit operator Vector3 (OsmNode node)
    {
        return new Vector3(node.X, 0, node.Y);
    }

    public OsmNode(XmlNode node)
    {
        NodeCount = 0;

        // Get the attribute values
        ID = GetAttribute<ulong>("id", node.Attributes);
        Latitude = GetAttribute<float>("lat", node.Attributes);
        Longitude = GetAttribute<float>("lon", node.Attributes);

        // Calculate the position in Unity units
        X = (float)MercatorProjection.lonToX(Longitude);
        Y = (float)MercatorProjection.latToY(Latitude);
    }
}
