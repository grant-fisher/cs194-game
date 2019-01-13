using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger
{
    // usage: platformGraph[startPlatform][endPlatform] => list of paths from start to end platforms
    // This is added to at the end of each logging
    public Dictionary<string, Dictionary<string, List<Path>>> platformGraph;
    
    public Logger()
    {
        platformGraph = new Dictionary<string, Dictionary<string, List<Path>>>();
    }

    private Path path; // This is the path we're building
    private List<Dictionary<string, Input_.KeyStatus>> recording; // These are the key strokes being recorded

    // Each ground-type should be named with a string that we
    // can use as a key
    public void StartLogging(string platform, Vector2 velocity, Vector2 position, int state)
    {
        path = new Path();
        path.startPlatform = platform;
        path.startPosition = position;
        path.startVelocity = velocity;
        path.startState = state;

        recording = new List<Dictionary<string, Input_.KeyStatus>>();
    }

    public void LogStep(Input_ Input_)
    {
        recording.Add(Input_.RecordInputState());
    }


    public void EndLogging(string platform)
    {
        path.endPlatform = platform;
        path.path.Add(recording);

        // Add this path to the list of paths from startPlatform to endPlatform
        if (platformGraph.ContainsKey(path.startPlatform))
        {
            if (platformGraph[path.startPlatform].ContainsKey(path.endPlatform))
            {
                platformGraph[path.startPlatform][path.endPlatform].Add(path);
            }
            else 
            {
                platformGraph[path.startPlatform][path.endPlatform] = new List<Path>() { path };
            }
        } 
        else 
        {
            platformGraph[path.startPlatform] = new Dictionary<string, List<Path>>();
            platformGraph[path.startPlatform][path.endPlatform] = new List<Path>() { path };
        }
    }
}  

public class Path 
{
    public string startPlatform, endPlatform;
    public Vector2 startPosition, startVelocity;
    public int startState;

    // List of all paths from startPlatform to endPlatform
    // Each path represented by a list of key states
    public List<List<Dictionary<string, Input_.KeyStatus>>> path;

    public Path()
    {
        path = new List<List<Dictionary<string, Input_.KeyStatus>>>();
    }
}


