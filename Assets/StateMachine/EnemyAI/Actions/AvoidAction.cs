using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "PluggableAI/Actions/Avoid")]

public class AvoidAction : Action {
    
    public override void Act (StateController controller) {

        Avoid(controller);

    }

    private Tuple<bool, Vector2> Evade_Jump(StateController controller) {
        
        Collider2D bullet = controller.bulletToAvoid;
        Vector2 bulletVelocity = bullet.gameObject.GetComponent<Rigidbody2D>().velocity;
        Vector2 bulletPosition = bullet.gameObject.GetComponent<Rigidbody2D>().position;
        Vector2 selfPosition = Utils.Vec3ToVec2(controller.gameObject.GetComponent<Transform>().position);
        
        // x, y, z, as in a grid
        float deltaT = Mathf.Abs((bulletPosition[0] - selfPosition[0]) / bulletVelocity[0]); // how long the bullet will take to reach this x coord

        Vector2 futureBulletPosition = bulletPosition + bulletVelocity * deltaT;

        // If we jump now, where will we be after deltaT time passes?
        // y2 = y1 + vy1 * deltaT + 1/2 * G * deltaT^2
        float y2 = selfPosition[1] + controller.stats.jumpSpeed * deltaT + (1/2) * Mathf.Pow(Physics2D.gravity[1], 2);

        float futureBulletTopY = futureBulletPosition[1] + Utils.GetActualBC2DSize(bullet.gameObject)[1] / 2;
        float futureSelfBottomY = y2 - Utils.GetActualBC2DSize(controller.gameObject)[1] / 2;

        if (futureSelfBottomY > futureBulletTopY)
            return new Tuple<bool, Vector2>(true, Vector2.up);
        else
            return new Tuple<bool, Vector2>(false, Vector2.up); // the particular vector returned doesn't matter in this instance
    }

    private Tuple<bool, Vector2> Evade_Move(Collider2D bullet) {
        return new Tuple<bool, Vector2>(false, Vector2.zero); // for now, ignore moving and just do jumping
    }

    private void Avoid(StateController controller) {
        // Use StateController member variable bulletToAvoid to determine movement behaviour
        // Prioritize jumping over horizontal movement

        Tuple<bool, Vector2> canAvoidByJumping = Evade_Jump(controller);
        Tuple<bool, Vector2> canAvoidByMoving = Evade_Move(controller.bulletToAvoid);   

        if (canAvoidByJumping.One) {
            Vector2 dir = canAvoidByJumping.Two;
            controller.GetComponent<Rigidbody2D>().velocity = Vector2.up * controller.stats.jumpSpeed;
        } else if (canAvoidByMoving.One) {
            Vector2 dir = canAvoidByMoving.Two;
            controller.GetComponent<Rigidbody2D>().velocity = dir * controller.stats.runSpeed;
        } else {
            Vector2 dir = Vector2.zero; // we're screwed 
            controller.GetComponent<Rigidbody2D>().velocity = dir;
        }
    }

}