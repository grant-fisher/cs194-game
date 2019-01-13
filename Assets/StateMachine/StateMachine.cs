using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        public UDelegateFn update, begin, end;
        public System.Action coroutine;
        public StateStruct(UDelegateFn a, System.Action b, UDelegateFn c, UDelegateFn d)
        {
            this.update = a; this.coroutine = b; this.begin = c; this.end = d;
        }
    }

    public StateMachine(Player _player) 
    {
        callbacks = new Dictionary<int, StateStruct>();
        this.player = _player;
    }

    // Add the callback functions for a new state to our callbacks dictioanry
    public void SetCallbacks(int i, UDelegateFn update, System.Action coroutine, UDelegateFn begin, UDelegateFn end) 
    {
        callbacks[i] = new StateStruct(update, coroutine, begin, end);
    }

    public void Update() 
    {
        // Check if the state has changed. If so, then execute end() and begin() of
        // the previous and current states, respectively.
        Debug.Log(State + " " + PrevState);
        if (State != PrevState) 
        {
            if (callbacks[PrevState].end != null) 
            {
                callbacks[PrevState].end();        
            }
            PrevState = State;

            UDelegateFn begin = callbacks[State].begin;
            if (begin != null)
            {
                begin();
            }

            System.Action coroutine = callbacks[State].coroutine;
            if (coroutine != null)
            {
                coroutine();
                // Reset the coroutine
                // callbacks[State].coroutine.Reset();

                // Start the coroutine
                //player.StartCoroutine(coroutine);
            }
        }
        // Execute the update function of the current state
        UDelegateFn update = callbacks[State].update;
        if (update != null)
        {
            update();
        }

        
    }


}
