using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace ServerSpace {
    
    class Program {

        /* Run the user code on the key-value data in the json file */
        private static bool RunCode(string input, string fname, string testInputFile, string testOutputFile) {

            /* Write everything in the provided string to a file that we can run */
            File.WriteAllText(fname, input);
            bool success = true;
            //ProcessStartInfo start = new ProcessStartInfo();
            /* start.FileName = "/usr/bin/python"; //cmd is full path to python.exe
            start.Arguments = args; //args is path to .py file and any cmd line args
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            */
            string inputs = File.ReadAllText(testInputFile);
            string expectedOutputs = File.ReadAllText(testOutputFile);
            string[] inputLines = inputs.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            string[] outputLines = expectedOutputs.Split(new[] { Environment.NewLine}, StringSplitOptions.None);

            ProcessStartInfo p;
            for (int i = 0; i < inputLines.Length; i++) {
                
                string a = inputLines[i];
                string b = outputLines[i];
                p = new ProcessStartInfo();
                p.FileName = "/usr/bin/python";
                p.Arguments = fname + " " + a;
                p.UseShellExecute = false;
                p.RedirectStandardOutput = true;

                using (Process process = Process.Start(p)) {
                    using (StreamReader reader = process.StandardOutput) {
                        string result = reader.ReadToEnd().Replace("\n", String.Empty);
                        if (result.Equals(b)) {
                            Console.WriteLine("pass " + result + " " + b);
                        } else {    
                            success = false;
                            Console.WriteLine("fail " + result + " " + b);
                        }
                    }
                }

            }

            return success;
        }

        private static void RunCodeTest() {
            /*
            string input = "print('hello world')" + Environment.NewLine;
            string fname = "hello-world-test.py";
            RunCode(input, fname);
            */
            string input = File.ReadAllText("test-addition/sample-solution.py");
            string fname = "test-addition/sample-sol.py";
            string testInputFile = "test-addition/input1.txt";
            string testOutputFile = "test-addition/input2.txt";
            RunCode(input, fname, testInputFile, testOutputFile);
        }

        private static int maxProgramLength = 1000;
        
        static void Main(string[] args) {

            // RunCodeTest();
            // return;


            IPAddress ipaddr = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(ipaddr, 8888);
            int requestCount = 0;
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            Console.WriteLine(" >> Server Started");
            // clientSocket = serverSocket.AcceptTcpClient();
            // Console.WriteLine(" >> Accept connection from client");
            requestCount = 0;

            while (true) {

                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine(" >> Accept connection from client");

                try {

                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[10025];
                    networkStream.Read(bytesFrom, 0, maxProgramLength);
                    string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    Console.WriteLine(dataFromClient);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" >> Data from client - " + dataFromClient);

                    if (dataFromClient.Equals("-1")) {
                        break;
                    }

                    string input = dataFromClient;
                    string fname = "test-addition/user-code.py";
                    string testInputFile = "test-addition/input1.txt";
                    string testOutputFile = "test-addition/input2.txt";
                    bool result = RunCode(input, fname, testInputFile, testOutputFile);
                    string response = result ? "1" : "0";

                    string serverResponse = "Last Message from client: " + result;
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >>" + serverResponse + "<<");
                }
                catch (Exception ex) {
                    break;
                    Console.WriteLine(ex.ToString());
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine(" >> exit");
            Console.ReadLine();

        }

    }

}
