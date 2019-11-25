using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorNPC : ActorBase
{
  // Additional components needed for an ActorNPC
  public ActorAI NpcAI;
  public Priorities Priorities;


  // Define all coroutines that should be available to all NPCs


  #region npcMovement

  // Define xy coordinates that mark our movement destination
  protected int TargetX;
  protected int TargetY;

  protected IEnumerator Coroutine_MoveTo()
  {
    if (true)
    {
      bool reached = Mathf.Abs(TargetX - rb.position.x) < 0.1 && Mathf.Abs(TargetY - rb.position.y) < 0.1;
      if (reached)
      {
        yield break;
      }
      else
      {
        var v = new Vector2(TargetX - rb.position.x, TargetY - rb.position.y);
        var magnitude = Mathf.Sqrt(v.x * v.x + v.y * v.y);
        v.x = v.x / magnitude;
        v.y = v.y / magnitude;
        var speed = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y);
        rb.velocity = v * speed;
      }
    }
  }
  #endregion


}
