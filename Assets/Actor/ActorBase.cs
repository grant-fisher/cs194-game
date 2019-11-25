using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Class: ActorBase
 * ----------------
 * The base class for all of the people and animals in the game.
 */
public class ActorBase : MonoBehaviour
{

  // Require these components
  public Rigidbody2D rb;
  public BoxCollider2D bc;
  public Camera ThisActorCamera;
  public StateMachine StateMachine;

  // Maintain the equipment that we have at any given point
  public ListOfEquipment Equipment;

}
