using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateController : MonoBehaviour {
    
    public State currentState;
    public State remainState;

    [HideInInspector] public EnemyStats stats;
    [HideInInspector] public EnemyView view;

    public GameObject Player;
    public Graph Graph;


    // If we switch to an avoidance behaviour, use this to determine the bullet to avoid
    [HideInInspector] public Collider2D bulletToAvoid;

    //[HideInInspector] public float stateTimeElapsed;


    // Transition to the next state
    public void TransitionToState(State nextState) {
        if (nextState != remainState)
            currentState = nextState;
    }



    /* Method: OnCollisionEnter2D
     * --------------------------
     * If we run into anything with the tag, "laser", destroy the 
     * laser and this game object simultaneously
     */
    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "Laser") {
            Debug.Log("destroy");
            Destroy(this);
            Destroy(col.gameObject);
        }

    }

    void LateUpdate() {
        Debug.Log("late: ~" + currentState + "~"); // bad
    }

    // Update what the character can see, then update the current state
    // Regardless of what state we are in, we apply better jump multipliers

    private int updateCount = 0;
    void Update() {
        updateCount++;
        Debug.Log("update " + updateCount + " " + this);
        view.UpdateView();
        currentState.UpdateState(this);
        Debug.Log("after2: ~" + currentState + "~"); // good

    }


    void Start() {
        // Initialize view and stats
        view = new EnemyView(this); // pass the game object for the enemy that is tied to this script
        stats = new EnemyStats();
        GetComponent<Rigidbody2D>().freezeRotation = true;
    }




}