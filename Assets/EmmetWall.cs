using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmmetWall : MonoBehaviour {

	public Transform emmet;

	void Start ()
	{

		/*
		 * Creates 9 new instances of the emmet transform, which is provided via the inspector
	 	 */

		Vector3 minBound = emmet.GetComponent<Renderer>().bounds.min;
		Vector3 maxBound = emmet.GetComponent<Renderer>().bounds.max;
		float width = maxBound[0] - minBound[0];
		float height = maxBound[1] - minBound[1];

		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				Instantiate(emmet, new Vector3(i * width, j * height, 0), Quaternion.identity);
			}
		}


	}


}
