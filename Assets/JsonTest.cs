using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using Newtonsoft.Json;

public class JsonTest
{
    private static Dictionary<int, string> di = new Dictionary<int, string>() {
        {1, "one"},
        {2, "two"}
    };

    public static void Main(string[] args)
    {
        string result = JsonConvert.SerializeObject(di);
        Console.WriteLine(result);
    }
}