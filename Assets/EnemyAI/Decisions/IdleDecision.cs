using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "PluggableAI/Decisions/IdleDecision")]

public class IdleDecision : Decision {

    public override bool Decide(StateController state) {
        return true; // If all other states have returned false, then idle
    }

}