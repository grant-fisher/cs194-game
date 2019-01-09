using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject player;
	private Transform transform;

	/* Offset between player and camera */
	private Vector3 offset; 

	void Start () {
		transform = GetComponent<Transform>(); 
		offset = transform.position - player.transform.position;
	}
	
	/* Called after update during each frame. 
	 * Keeps the offset constant between the player and the camera */
	void LateUpdate() {
		transform.position = player.transform.position + offset;
	}

}
