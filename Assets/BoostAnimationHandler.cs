using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoostAnimationHandler : MonoBehaviour {

    private float timer = 0f;
    private const float lifespan = 0.5f;

    void Update() {

        /* Destory the boost cloud after it completes its lifespan */
        if (timer > lifespan) 
            Destroy(gameObject);
        else timer += Time.deltaTime;
    }

}