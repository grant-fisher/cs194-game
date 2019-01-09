using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehaviour : MonoBehaviour {

	/* 
	 * Handle all behaviour pertaining to one of our build blocks
	 */

	private Vector3 mouseDownPosition;


	void OnMouseDown() {

		/* Set the position which will be how we compute deltaPosition during a drag */
		mouseDownPosition = Input.mousePosition;

	}

	void OnCollisionEnter2D(Collision2D col) {

		/* Ignore collisions between build blocks */
		if (col.gameObject.tag == "BuildBlock") {
			Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col.gameObject.GetComponent<Collider2D>());
		}

	}

	void OnMouseDrag() {

		/* Move the transform 0.1 units in the direction of the mouse drag */
		Transform transform = GetComponent<Transform>();
		Vector3 direction = Input.mousePosition - mouseDownPosition;
		Vector3 deltaPosition = direction.normalized * 0.1f;
		Vector3 updatedPosition = transform.position + deltaPosition;
		transform.position = updatedPosition;

	} 


}
