using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public GameObject player;
	private Transform transform;

	// Offset between the player and the camera
	private Vector3 offset;

	void Start ()
	{
		transform = GetComponent<Transform>();
		offset = transform.position - player.transform.position;
	}

	// Called after Update() during each frame
	// Update transform position to keep a constant offset between the player and the camera
	void LateUpdate()
	{
		transform.position = player.transform.position + offset;
	}

}
