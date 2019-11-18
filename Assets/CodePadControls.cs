using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Renci.SshNet; // Finish integrating

public class CodePadControls : MonoBehaviour
{

	/* Handle the input and execution of user code
	 * This involves ssh connection to server where we run the code */

	public InputField userText;

	/* For server connection */
  private string hostname = "myth@stanford.edu";
  private string username = "wlauer";
  private string password = "MGustave@GBH1";

	/* Only the run button is used from here - exit is handled by toolkit */
	public Button runButton, exitButton;

	private void RunCode() {
		Debug.Log("run code");
		GetResultOfCode("");
	}

	private void GetResultOfCode(string userCode) {

		// https://answers.unity.com/questions/980864/using-a-ssh-connection-in-a-unity-project.html
		/*
		Debug.Log("enter");
		try {
			PasswordConnectionInfo connection = PasswordConnectionInfo(hostname, username, password);

			SshClient client = new SshClient(connection);
			Debug.Log("connecting");
			client.Connect();
			Debug.Log("connected");

			SshCommand command = client.RunCommand("pwd");
			Debug.Log(command.Result);
			Debug.Log("ran command");
			client.Disconnect();
			Debug.Log("disconnected");

		} catch (System.Exception e) {
			Debug.Log("exception: " + e);
		}
		Debug.Log("exit");
	*/
	}


	void Start () {
		runButton.onClick.AddListener(delegate { RunCode(); });
	}

	void Update () {

	}
}
