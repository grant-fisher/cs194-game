using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPadButtons : MonoBehaviour {

	public GameObject spriteA, spriteB;
	public Button buttonA, buttonB, buttonC, buttonD;
	public Camera mainCamera;

	private GameObject spriteToAdd;
	private List<GameObject> tempAdded;
	private bool addingSprites;
	private int numAdded;

	private int playerCredits; // Currency
	private int tempSpent; 

	/* Method: AddSprite
	 * -----------------
	 */
	void AddSprite(GameObject sprite, Vector3 position) {
		if (tempSpent == playerCredits) {
			Debug.Log("not enough money");
		} else {
			GameObject clone = Instantiate(sprite, new Vector3(position[0], position[1], 0.0f), Quaternion.identity) as GameObject;

			/* Add rigidbody constraints */
			clone.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

			tempAdded.Add(clone);
			tempSpent++;
		}
	}

	/* Method: Checkout
	 * ----------------
	 * Update player assets and freeze changes.
	 */
	void Checkout() {

		playerCredits = playerCredits - tempSpent;
		tempAdded = new List<GameObject>();
		numAdded = 0;
		tempSpent = 0;
		spriteToAdd = null;

	}


	/*
	 * Method: InitiateJoinSprites
	 * -------------------
	 * Select two sprites, and snap sprite 2 to the nearest side of sprite 1
	 */
	void InitiateAddSprites() {

		addingSprites = true;

	}


	void Start (){
		playerCredits = 10;
		tempSpent = 0;
		numAdded = 0;
		addingSprites = false;
		spriteToAdd = null;
		tempAdded = new List<GameObject>();

		buttonA.onClick.AddListener(delegate { 
			if (addingSprites) {
				spriteToAdd = spriteA;
			}
		});

		buttonB.onClick.AddListener(delegate { 
			if (addingSprites) {
				spriteToAdd = spriteB;
			}
		});

		buttonC.onClick.AddListener(delegate { 
			InitiateAddSprites();	
		});

		buttonD.onClick.AddListener(delegate {
			CancelChanges();
		});

	

	}
	
	/* Method: SortByZCoordinate
	 * -------------------------
	 * Comparator to sort a list of GameObjects by their Z coordinate, so that the one
	 * with the most negative Z coordinate appears first in the sorted list, and the one with the 
	 * most positive Z coordinate appears last.
	 */
	private static int SortByZCoordinate(GameObject a, GameObject b) {

		if (a.transform.position[2] < b.transform.position[2]) {
			return -1;
		} else if (a.transform.position[2] == b.transform.position[2]) {
			return 0;
		} else {
			return 1;
		}
			
	}
	
	/* Method: Cancel Changes
	 * ----------------------
	 * Delete all temporary added sprites and restore user resources
	 */
	private void CancelChanges() {

		addingSprites = false;
		spriteToAdd = null;
		numAdded = 0;
		tempSpent = 0;
		for (int i = 0; i < tempAdded.Count; i++) {
			Destroy(tempAdded[i]);
		}
		tempAdded = new List<GameObject>();

	}


	
	void Update () {
		
		if (Input.GetKeyUp(KeyCode.Alpha0) && addingSprites && spriteToAdd != null) {
			Vector3 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
			Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPoint);
			if (viewportPoint[0] >= 0.0 && viewportPoint[0] <= 1.0 && viewportPoint[1] >= 0.0 && viewportPoint[1] <= 1.0) {
				AddSprite(spriteToAdd, worldPoint);
			}
		}

	}
}
