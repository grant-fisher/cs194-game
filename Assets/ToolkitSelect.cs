using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolkitSelect : MonoBehaviour {

	/* Handle opening and closing of tools */

	public GameObject buildPad;
	public GameObject codePad;

	/* Store a separate reference here so we can exit if need be 
	 * Behaviour initialized in CodePadControls.Start() */
	public Button codePadExitButton;

	private bool codePadShowing;
	private bool buildPadShowing;


	public bool GetCodePadActive() { return codePadShowing; }
	public bool GetBuildPadActive() { return buildPadShowing; }

	public GameObject player;

	private void CloseCodePad() {

		codePadShowing = false;
		codePad.SetActive(codePadShowing);

		/* Allow player updates to proceed */
		player.GetComponent<PlayerControls>().enabled = true;

		/* Delete whatever text was currently stored */
		InputField inputField = codePad.GetComponentInChildren(typeof(InputField)) as InputField;
		inputField.text = "";

	}

	void Start () {

		buildPadShowing = false;
		codePadShowing = false;
		buildPad.SetActive(buildPadShowing);
		codePad.SetActive(codePadShowing);
		codePadExitButton.onClick.AddListener( delegate {
			CloseCodePad();
		});
	}
	
	void Update () {

		if (Input.GetKeyUp(KeyCode.B)) {

			/* Code pad takes precedence */
			if (codePadShowing) { 
				return;
			}
			
			/* Switch to buildpad */
			buildPadShowing = !buildPadShowing;
			buildPad.SetActive(buildPadShowing);

			/* Pause updates on player */
			player.GetComponent<PlayerControls>().enabled = !buildPadShowing;

		} else if (Input.GetKeyUp(KeyCode.C)) {

			/* Don't toggle - use a button for that */
			if (codePadShowing) {
				return;
			}

			/* Switch to codepad */
			if (buildPadShowing) {
				buildPadShowing = false;
				buildPad.SetActive(buildPadShowing);
			}
			codePadShowing = !codePadShowing;
			codePad.SetActive(codePadShowing);

			/* Pause updates on player */
			GameObject player = GameObject.Find("Player");
			player.GetComponent<PlayerControls>().enabled = false;

		} 
		
	}
}
