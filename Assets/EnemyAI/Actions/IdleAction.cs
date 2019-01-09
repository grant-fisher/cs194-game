using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allow access from Project -> Create
[CreateAssetMenu (menuName = "PluggableAI/Actions/Idle")]

public class IdleAction : Action {
    
    public override void Act (StateController controller) {
        Debug.Log("idle");
        Idle(controller);
    }    

    private void Idle(StateController controller) {
        // Let velocity = 0 and return
        // controller.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        return;
    }

}