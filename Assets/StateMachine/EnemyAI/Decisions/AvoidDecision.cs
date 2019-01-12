using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "PluggableAI/Decisions/AvoidDecision")]
public class AvoidDecision : Decision {
    
    public override bool Decide (StateController controller) {
        // If any of the bullets is going to hit us, then we should switch to the avoid behaviour
        return AtRisk(controller);
    }
    
    /* Method: IsGoingToHit
     * --------------------
     * Takes in the collider for a bullet, and returns true if the bullet is going to hit
     * thisEnemy
     */
    private bool IsGoingToHit(string enemyName, Collider2D bullet) {
        
        Vector2 bulletVelocity = bullet.gameObject.GetComponent<Rigidbody2D>().velocity;
        Vector2 sz = Utils.GetActualBC2DSize(bullet.gameObject); // size of the bullet box collider
        Vector2 bp = bullet.gameObject.GetComponent<Rigidbody2D>().position; // position of the bullet center

        Vector2[] positions = new Vector2[] { new Vector2(bp[0] - sz[0] / 2, bp[1] - sz[1] / 2), 
                                              new Vector2(bp[0] + sz[0] / 2, bp[1] + sz[1] / 2),
                                              new Vector2(bp[0] - sz[0] / 2, bp[0] + sz[1] / 2),
                                              new Vector2(bp[0] + sz[0] / 2, bp[0] - sz[1] / 2) };

        foreach (Vector2 bulletPosition in positions) {   
            RaycastHit2D res = Physics2D.Raycast(bulletPosition, bulletVelocity, Mathf.Infinity, ~(1 << 8));
            if (res.collider != null && (res.collider.gameObject.name == enemyName)) return true;
        }
        return false;
    }


    private bool AtRisk(StateController controller) {
        
        int numBullets = controller.view.liBullets.Count;
        for (int i = 0; i < numBullets; i++) {
            if (IsGoingToHit(controller.gameObject.name, controller.view.liBullets[i])) {
                
                // If the ith bullet is going to hit the enemy, then set this bullet as the bullet we
                // want to avoid and return true

                controller.bulletToAvoid = controller.view.liBullets[i];
                Debug.Log("true");
                return true;
            }
        }
        Debug.Log("false");
        return false;
    }

    
}