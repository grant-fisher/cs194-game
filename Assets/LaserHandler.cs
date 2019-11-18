using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHandler : MonoBehaviour
{

  public Camera mainCamera;

  void OnCollisionEnter2D(Collision2D col)
  {

      // Only destroy the laser here if it doesn't destroy its target,
      // as if it does destroy the target, then we delete both at the same time
      if (col.gameObject.tag == "Player" || col.gameObject.tag == "Laser")
      {
        return;
      }
      Destroy(gameObject);

  }

  void Update()
  {

  }

  void FixedUpdate()
  {
    // If out of range of the screen, destroy this laser
    Vector3 viewportPoint = Camera.main.WorldToViewportPoint(GetComponent<Transform>().position);
    if (!(viewportPoint[0] >= 0.0 && viewportPoint[0] <= 1.0 && viewportPoint[1] >= 0.0 && viewportPoint[1] <= 1.0))
    {
      Destroy(gameObject);
    }
  }
}
