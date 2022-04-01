using System.Collections.Generic;
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

public class OsmReader
{
    public string resourceFile;
    public Dictionary<ulong, OsmNode> nodes;
    public OsmBounds bounds;
    public List<OsmWay> ways;

    private static OsmReader _instance;
    private static readonly object _syncLock = new object();
    public static OsmReader Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_syncLock)
            {
                if (_instance == null)
                {
                    _instance = new OsmReader();
                }
            }
            return _instance;
        }
    }

    // Load the OpenMap data resource file.
    public void ReadMap()
    {
        nodes = new Dictionary<ulong, OsmNode>();
        ways = new List<OsmWay>();
        var txtAsset = Resources.Load<TextAsset>(resourceFile);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);

        SetBounds(doc.SelectSingleNode("/osm/bounds"));
        GetNodes(doc.SelectNodes("/osm/node"));
        GetWays(doc.SelectNodes("/osm/way"));
        MergeWays();
    }

    void GetNodes(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode n in xmlNodeList)
        {
            OsmNode node = new OsmNode(n);
            nodes[node.ID] = node;
        }
    }
    void SetBounds(XmlNode xmlNode)
    {
        bounds = new OsmBounds(xmlNode);
    }
    void GetWays(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode xmlNode in xmlNodeList)
        {
            OsmWay way = new OsmWay(xmlNode, nodes);
            ways.Add(way);
        }
    }

    //Merge Osm way which starts (or ends) in between a road section to the adjacent one so that 
    //the newly formed way does not starts (or ends) in between a road section
    void MergeWays()
    {
        foreach (var n in nodes)
        {
            if(n.Value.NodeCount == -1)
            {
                n.Value.NodeCount = 1;

                //Get the way objects
                ulong id = n.Value.ID;
                int k = 0;
                OsmWay[] activeWays = new OsmWay[2];
                bool[] activeWaysStart = new bool[2];

                foreach (var w in ways)
                {
                    if(w.NodeIDs.Contains(id))
                    {
                        activeWays[k] = w;
                        if (w.NodeIDs[0] == id) activeWaysStart[k] = true;

                        k++;
                        if (k == 2) break;
                    }
                }

                if(activeWays[0] !=  null && activeWays[1] != null)
                {
                    //Merge the way objects
                    List<ulong>[] nodeIds = new List<ulong>[2];
                    nodeIds[0] = activeWays[0].NodeIDs;
                    nodeIds[1] = activeWays[1].NodeIDs;

                    if (activeWaysStart[0] && activeWaysStart[1])
                    {
                        nodeIds[0].RemoveAt(0);
                        nodeIds[0].Reverse();
                    }
                    else if (!activeWaysStart[0] && !activeWaysStart[1])
                    {
                        nodeIds[1].RemoveAt(nodeIds[1].Count - 1);
                        nodeIds[1].Reverse();
                    }
                    else if (!activeWaysStart[0] && activeWaysStart[1])
                    {
                        nodeIds[1].RemoveAt(0);
                    }
                    else if (activeWaysStart[0] && !activeWaysStart[1])
                    {
                        nodeIds[0].Reverse();
                        nodeIds[1].Reverse();

                        nodeIds[1].RemoveAt(0);
                    }

                    foreach (var nd in nodeIds[1])
                    {
                        nodeIds[0].Add(nd);
                    }
                    ways.Remove(activeWays[1]);
                }
            }
        }
    }
}
