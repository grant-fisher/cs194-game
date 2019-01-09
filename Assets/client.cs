using System;
using System.Net.Sockets;
using System.Text; 
using System.IO;
//using System.Diagnostics;
using System.ComponentModel;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*  
public class CodeString
{
    static TcpClient clientSocket = new TcpClient();

    private static void Run() {

        string userCode = File.ReadAllText("test-addition/sample-solution.py");
        
        clientSocket.Connect("127.0.0.1", 8888);
        NetworkStream serverStream = clientSocket.GetStream();
        byte[] outStream = Encoding.ASCII.GetBytes(userCode + "$");
        serverStream.Write(outStream, 0, outStream.Length);
        serverStream.Flush();
        byte[] inStream = new byte[10025];
        serverStream.Read(inStream, 0, inStream.Length);
        string result = Encoding.ASCII.GetString(inStream);
        Console.WriteLine("Result = " + result);
    }

    static void Main(string[] args) {
        Run();
    }
}
*/


/* This will be the class utilized by the codepad */

public class client : MonoBehaviour {

    static TcpClient clientSocket = new TcpClient();

    public Button runButton;

    void Run() {

        //string userCode = File.ReadAllText("test-addition/sample-solution.py");
        //string userCode = GetComponent<InputField>().text;
        InputField inputField = GetComponentInChildren(typeof(InputField)) as InputField;
        string userCode = inputField.text;
        Debug.Log("text = " + userCode);
        return;

        clientSocket.Connect("127.0.0.1", 8888);
        NetworkStream serverStream = clientSocket.GetStream();
        byte[] outStream = Encoding.ASCII.GetBytes(userCode + "$");
        serverStream.Write(outStream, 0, outStream.Length);
        serverStream.Flush();
        byte[] inStream = new byte[10025];
        serverStream.Read(inStream, 0, inStream.Length);
        string result = Encoding.ASCII.GetString(inStream);
        Console.WriteLine("Result = " + result);
    }


    void Update() {

    }


    void Start() {
        runButton.onClick.AddListener( delegate { 
            Run(); 
        });
    }

}
