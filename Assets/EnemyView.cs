using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyView
{

  private StateController enemyController;

  [HideInInspector] public List<Collider2D> liBullets;
  [HideInInspector] public List<Collider2D> liPlayer;
  [HideInInspector] public Collider2D[] nearbyColliders;


  public EnemyView(StateController ec)
  {
      enemyController = ec;
  }

  private void ClearLists()
  {
      liBullets = new List<Collider2D>();
      liPlayer = new List<Collider2D>();
  }

  // Update the variables that this enemy sees
  public void UpdateView() {

      ClearLists();
      Vector2 pos = Utils.Vec3ToVec2(enemyController.gameObject.GetComponent<Transform>().position);

      // Todo: does this return in order of distance? If so, then that's great since our AvoidDecision
      // function would then check against the nearest bullet that poses a risk
      nearbyColliders = Physics2D.OverlapCircleAll(pos, enemyController.stats.lineOfSightDistance);

      // Add all players and bullets within the enemies FOV
      foreach (Collider2D c in nearbyColliders)
      {
          switch (c.gameObject.tag)
          {
              case "Laser": liBullets.Add(c); break;
              case "Player": liPlayer.Add(c); break;
          }
      }


  }

}
