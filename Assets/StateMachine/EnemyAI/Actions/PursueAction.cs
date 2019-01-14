using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "PluggableAI/Actions/Pursue")]

public class PursuitAction : Action
{

    public override void Act(StateController controller)
    {
        Pursue(controller);
    }

    private void Pursue(StateController controller)
    {
        var target = controller.Player.GetComponent<Transform>().position;
        // where to store/update curplatform
        // how should we get the next platform
    }


}