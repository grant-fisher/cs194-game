using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ******************************************************************
// StateMachine is responsible for tracking the current player state
// ******************************************************************
public class StateMachine {

    private Player player;

    private const int UPDATE = 0;
    private const int COROUTINE = 1;
    private const int BEGIN = 2;
    private const int END = 3;

    public int State;

    private int PrevState;
    private Dictionary<int, StateStruct> callbacks; 

    public class StateStruct 
    {
        public UDelegateUpdate update, begin, end;
        public IEnumerator coroutine;
        public StateStruct(UDelegateUpdate a, IEnumerator b, UDelegateUpdate c, UDelegateUpdate d)
        {
            this.update = a; this.coroutine = b; this.begin = c; this.end = d;
        }
    }

    public StateMachine(Player _player) {
        callbacks = new Dictionary<int, StateStruct>();
        this.player = _player;
    }

    // Add the callback functions for a new state to our callbacks dictioanry
    public void SetCallbacks(int i, UDelegateUpdate update, IEnumerator coroutine, UDelegateUpdate begin, UDelegateUpdate end) {
        callbacks[i] = new StateStruct(update, coroutine, begin, end);
    }

    public void Update() 
    {
        // Check if the state has changed. If so, then execute end() and begin() of
        // the previous and current states, respectively.
        if (State != PrevState) {
            if (callbacks[PrevState].end != null) {
                callbacks[PrevState].end();        
            }
            PrevState = State;
            IEnumerator coroutine = callbacks[State].coroutine;
            if (coroutine != null)
            {
                player.StartCoroutine(coroutine);
            }
        }
        // Execute the update function of the current state
        if (callbacks[State].update != null)
        {
            Debug.Log("calling update");
            callbacks[State].update();
        }

        
    }


}
