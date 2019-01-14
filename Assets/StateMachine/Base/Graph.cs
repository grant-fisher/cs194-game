using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Graph
{
    // Implements a graph interface for AI navigation
    // To be initialized upon logging completion

    private Dictionary<string, Dictionary<string, List<Path>>> _internal;
    private System.Random rnd = new System.Random();
    private List<LineRenderer> pathVisualizations; // initialize in Start

    public Graph(Dictionary<string, Dictionary<string, List<Path>>> di)
    {
        this._internal = di;
    }


    private float HeuristicCost(string a, string b)
    {
        return 0f;
    }

    private List<string> ReconstructPath(Dictionary<string, string> cameFrom, string current)
    {
        return new List<string>();
    }

    private List<string> AStart(string from, string to) // incomplete
    { // https://en.wikipedia.org/wiki/A*_search_algorithm
        HashSet<string> closedSet = new HashSet<string>();
        HashSet<string> openSet = new HashSet<string>() { from };
        Dictionary<string, string> cameFrom = new Dictionary<string, string>(); 
        Dictionary<string, float> gScore = new Dictionary<string, float>();
        Dictionary<string, float> fScore = new Dictionary<string, float>();

        gScore[from] = 0f;
        fScore[from] = HeuristicCost(from, to);

        while (openSet.Count > 0)
        {
            string minNode = "";
            float minCost = Mathf.Infinity;
            foreach (string s in fScore.Keys)
            {
                if (fScore[s] < minCost)
                {
                    minNode = s;
                }
            }
            if (minNode == to)
            {
                return ReconstructPath(cameFrom, to);
            }
            // ... ... 


        }
        return new List<string>();


    }


    private bool RecursiveBacktrack(ref List<string> curPath, string curPlatform, string destPlatfom, ref HashSet<string> visited)
    {
        if (curPlatform == destPlatfom)
        {
            return true;
        }
        else
        {
            visited.Add(curPlatform);
            foreach (string neighbor in this._internal[curPlatform].Keys)
            {
                if (!visited.Contains(neighbor))
                {
                    curPath.Add(neighbor);
                    if (RecursiveBacktrack(ref curPath, neighbor, destPlatfom, ref visited))
                    {
                        return true; // Keep the modified list and return
                    }
                    else
                    {
                        curPath.RemoveAt(curPath.Count - 1); // pop the last neighbor added
                    }
                }
                
            }
            // If this is reached, then no path from this node leads to the destination
            visited.Remove(curPlatform);
            return false;
        }

    }


    public List<string> DFS(string from, string to)
    {
        // Return a list of the paths taken to navigate from platform [from] to platform [to]
        List<string> path = new List<string>();
        HashSet<string> visited = new HashSet<string>();
        if (RecursiveBacktrack(ref path, from, to, ref visited))
        {
            // If we find a path, return it
            return path;
        }
        else return new List<string>(); // Otherwise return an empty list
    }


    public Path GetPathBetween(string cur, string next)
    {
        // Randomly select one of the paths between platforms cur and next, and return it
        int npaths = this._internal[cur][next].Count;
        return this._internal[cur][next][rnd.Next(0, npaths)];
    }


}