using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "PluggableAI/State")]
public class State : ScriptableObject {

    public Action[] actions;
    public Transition[] transitions;

    // At each UpdateState step, we do all actions and check transitions
    // to see whether we should change to a new state 
    public void UpdateState(StateController controller) {
        DoActions(controller);
        CheckTransitions(controller);
    }

    private void DoActions(StateController controller) {
        for (int i = 0; i < actions.Length; i++) {
            actions[i].Act(controller);
        }
    }

    private void CheckTransitions(StateController controller) {
        for (int i = 0; i < transitions.Length; i++) {
            bool decisionSucceeded = transitions[i].decision.Decide(controller); // the order of decisions matters!
            Debug.Log("decision succeeded: " + decisionSucceeded);
            if (decisionSucceeded) {
                controller.TransitionToState(transitions[i].trueState);
            } else {
                controller.TransitionToState(transitions[i].falseState);
            }
        }
    }


}