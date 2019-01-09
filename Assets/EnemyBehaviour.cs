using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour {

    public GameObject enemy;
    private Rigidbody2D rb;
    private Collider2D collider;

    private float lineOfSightDistance = 8.0f; // How far the enemy can see
    private float enemyMaxSpeed = 7.0f;

    private float runSpeed = 12f;

    private int fixedUpdateCount = 0;

    /* May move all jumping modifiers to utils if we want some unified jump feel for player and npcs */
    private float jumpSpeed = 20f;
	private float fallMultiplier = 4f;
	private float lowJumpMultiplier = 3f;
	private float playerBonusGravity = 9.8f; // add additional gravity to get faster jumps

    private bool jumping = false;

    private bool currentlyAvoidingBullet = false;

    // Don't move further than this to the left or right while dodging a bullet 
    private float maxDodgeDistance = 5f; 

    public GameObject ghostPrefab;
    private GameObject ghostLeft;
    private GameObject ghostRight;


    void OnCollisionEnter2D(Collision2D col) {

        if (col.gameObject.tag == "Laser") {
            /* Destroy the enemy and the impacting laser simultaneously */
            Destroy(enemy);
            Destroy(col.gameObject);
        }

    }
    
    // migrated
    void Start() {
        rb = enemy.GetComponent<Rigidbody2D>();
        collider = enemy.GetComponent<Collider2D>();
        //rb.gravityScale = 0.0f;
        rb.freezeRotation = true;

        //Debug.Log("pre stateMachine");
        //stateMachine = new AIStateMachine();
        //stateMachine.GetCurState().Run();
        //Debug.Log("post state machine");

    }

    /* Method: IsGoingToHit
     * --------------------
     * For a given bullet, determine whether it will hit the player on continuation of its 
     * current trajectory. Since bullets do have width and height, we perform several raycasts, one from
     * each corner of the bullet.
     */
    // migrated
    bool IsGoingToHit(Collider2D nearest) {

        Vector2 bulletVelocity = nearest.gameObject.GetComponent<Rigidbody2D>().velocity;
        Vector2 sz = Utils.GetActualBC2DSize(nearest.gameObject); // size of the bullet box collider
        Vector2 bp = nearest.gameObject.GetComponent<Rigidbody2D>().position; // position of the bullet center

        Vector2[] positions = new Vector2[] { new Vector2(bp[0] - sz[0] / 2, bp[1] - sz[1] / 2), 
                                              new Vector2(bp[0] + sz[0] / 2, bp[1] + sz[1] / 2),
                                              new Vector2(bp[0] - sz[0] / 2, bp[0] + sz[1] / 2),
                                              new Vector2(bp[0] + sz[0] / 2, bp[0] - sz[1] / 2) };

        foreach (Vector2 bulletPosition in positions) {   
            RaycastHit2D res = Physics2D.Raycast(bulletPosition, bulletVelocity, Mathf.Infinity, ~(1 << 8));
            if (res.collider != null && (res.collider.gameObject.name == enemy.name)) return true;
        }

        return false;

    }


    /* Method: Evade_[Move/Jump]
     * -------------------------
     * For the given method of movement, return a tuple containing
     * (a) whether a successful escape direction exists and
     * (b) what that escape direction is 
     */
    // not complete migrated
    Tuple<bool, Vector2> Evade_Move(Collider2D nearest) {

        Vector2 bulletVelocity = Utils.Vec3ToVec2(nearest.gameObject.GetComponent<Rigidbody2D>().velocity);
        Vector2 bulletPosition = Utils.Vec3ToVec2(nearest.gameObject.GetComponent<Rigidbody2D>().position);
        Vector2 selfPosition = Utils.Vec3ToVec2(GetComponent<Transform>().position);
        Vector2 selfSize = Utils.GetActualBC2DSize(enemy);
        
        Vector2 bulletSize = Utils.GetActualBC2DSize(nearest.gameObject);

        float dist2TopY = Mathf.Abs((bulletPosition[1] - bulletSize[1] / 2) - (selfPosition[1] + selfSize[1] / 2)); // top y, bottom y of the enemy sprite box collider
        float dist2BottomY = Mathf.Abs((bulletPosition[1] + bulletSize[1] / 2) - (selfPosition[1] - selfSize[1] / 2));
        float time2TopY = Mathf.Abs(dist2TopY / bulletVelocity[1]);
        float time2BottomY = Mathf.Abs(dist2BottomY / bulletVelocity[1]);

        Vector2 futureLeftBound;
        Vector2 futureRightBound;

        if (bulletPosition[1] > selfPosition[1] && bulletPosition[0] > selfPosition[0]) { // bullet is top right

            Debug.Log("top right");
            futureLeftBound = selfPosition + Vector2.left * Mathf.Clamp(runSpeed * time2BottomY, 0f, maxDodgeDistance);
            futureRightBound = selfPosition + Vector2.right * Mathf.Clamp(runSpeed * time2TopY, 0f, maxDodgeDistance);

        } else if (bulletPosition[1] <= selfPosition[1] && bulletPosition[0] <= selfPosition[0]) { // bullet is lower left

            Debug.Log("lower left");
            futureLeftBound = selfPosition + Vector2.left * Mathf.Clamp(runSpeed * time2TopY, 0f, maxDodgeDistance);
            futureRightBound = selfPosition + Vector2.right * Mathf.Clamp(runSpeed * time2BottomY, 0f, maxDodgeDistance);

        } else if (bulletPosition[1] > selfPosition[1] && bulletPosition[0] <= selfPosition[0]) { // bullet is top left

            Debug.Log("top left");
            futureLeftBound = selfPosition + Vector2.left * Mathf.Clamp(runSpeed * time2BottomY, 0f, maxDodgeDistance);
            futureRightBound = selfPosition + Vector2.right * Mathf.Clamp(runSpeed * time2TopY, 0f, maxDodgeDistance);

        } else if (bulletPosition[1] <= selfPosition[1] && bulletPosition[0] > selfPosition[0]) { // bullet is bottom right

            Debug.Log("bottom right");
            futureLeftBound = selfPosition + Vector2.left * Mathf.Clamp(runSpeed * time2TopY, 0f, maxDodgeDistance);
            futureRightBound = selfPosition + Vector2.right * Mathf.Clamp(runSpeed * time2BottomY, 0f, maxDodgeDistance);

        } else {
            Debug.Log("No bullet orientation case filled");
            futureLeftBound = Vector2.zero;
            futureRightBound = Vector2.zero;
            return new Tuple<bool, Vector2>(false, Vector2.zero);
        } 

        /* Create two ghost colliders at the maximum positions the player can reach in the time allowed, then raycast to see
         * if the bullet will still hit those locations */
        
        Debug.Log(selfPosition + " " + futureLeftBound + " " + futureRightBound);

        ghostLeft = Instantiate(ghostPrefab, futureLeftBound, Quaternion.identity);
        ghostRight = Instantiate(ghostPrefab, futureRightBound, Quaternion.identity);
        
        RaycastHit2D[] cast = Physics2D.RaycastAll(bulletPosition, bulletVelocity, Mathf.Infinity, (1 << 9));
        if (cast.Length == 2) {
            Debug.Log("both hit");
            return new Tuple<bool, Vector2>(false, Vector2.zero); // both extremes are hit, so there is no way to escape
        } else {
            if (cast.Length == 0) {
                Debug.Log("neither hit");
                return new Tuple<bool, Vector2>(true, Vector2.left); // default to moving left if neither collider is hit
            } else {
                if (cast[0].collider.gameObject.name == ghostLeft.name) {
                    Debug.Log("left hit");
                    return new Tuple<bool, Vector2>(true, Vector2.right); // left is hit, so move right
                } else {
                    Debug.Log("right hit");
                    return new Tuple<bool, Vector2>(true, Vector2.left); // right is hit, so move left
                }
            }
        }

        // Remove the ghosts once we're done with them
        Destroy(ghostLeft);
        Destroy(ghostRight);

        Debug.Log("no case hit");
        return new Tuple<bool, Vector2>(false, Vector2.zero);
    }
    
    // migrated
    Tuple<bool, Vector2> Evade_Jump(Collider2D nearest) {
        
        /* Get the height to which the player could jump in the amount of time until the bullet reaches
         * the current x coordinate of the player, and whether this brings the bottom edge of the player 
         * collider above the y coordinate where the bullet collider will be */
        
        Vector2 bulletVelocity = nearest.gameObject.GetComponent<Rigidbody2D>().velocity;
        Vector2 bulletPosition = nearest.gameObject.GetComponent<Rigidbody2D>().position;
        Vector2 selfPosition = Utils.Vec3ToVec2(GetComponent<Transform>().position);
        
        // x, y, z, as in a grid
        float deltaT = Mathf.Abs((bulletPosition[0] - selfPosition[0]) / bulletVelocity[0]); // how long the bullet will take to reach this x coord

        Vector2 futureBulletPosition = bulletPosition + bulletVelocity * deltaT;

        // If we jump now, where will we be after deltaT time passes?
        // y2 = y1 + vy1 * deltaT + 1/2 * G * deltaT^2
        float y2 = selfPosition[1] + jumpSpeed * deltaT + (1/2) * Mathf.Pow(Physics2D.gravity[1], 2);

        float futureBulletTopY = futureBulletPosition[1] + Utils.GetActualBC2DSize(nearest.gameObject)[1] / 2;
        float futureSelfBottomY = y2 - Utils.GetActualBC2DSize(enemy)[1] / 2;

        if (futureSelfBottomY > futureBulletTopY)
            return new Tuple<bool, Vector2>(true, Vector2.up);
        else
            return new Tuple<bool, Vector2>(false, Vector2.up); // the particular vector returned doesn't matter in this instance

    }

    // migrated
    void TryToAvoidBullets(List<Collider2D> liBullets) {

        List<Collider2D> riskBullets = new List<Collider2D>();
        foreach (Collider2D bullet in liBullets) {
            if (IsGoingToHit(bullet)) {
                riskBullets.Add(bullet);
            }
        }

        if (riskBullets.Count == 0) { 
            currentlyAvoidingBullet = false;
        
        } else {

            if (currentlyAvoidingBullet) {
                return; // we've already taken action to dodge the nearest bullet
            }
            Vector2 selfLoc = Utils.Vec3ToVec2(GetComponent<Transform>().position);
            riskBullets.Sort( 
                delegate(Collider2D a, Collider2D b) 
                { 
                    float magnitudeA = (Utils.Vec3ToVec2(a.gameObject.GetComponent<Transform>().position) - selfLoc).magnitude;
                    float magnitudeB = (Utils.Vec3ToVec2(b.gameObject.GetComponent<Transform>().position) - selfLoc).magnitude;
                    return magnitudeA.CompareTo(magnitudeB);
                });
            
            Collider2D nearest = riskBullets[0];

            Tuple<bool, Vector2> canAvoidByMoving = Evade_Move(nearest);
            Tuple<bool, Vector2> canAvoidByJumping = Evade_Jump(nearest);
           
            Vector2 dir;
            if (canAvoidByJumping.One) {
                dir = canAvoidByJumping.Two;
                rb.velocity = Vector2.up * jumpSpeed;
                currentlyAvoidingBullet = true;
                Debug.Log("b");
            } else if (canAvoidByMoving.One) {
                dir = canAvoidByMoving.Two;
                rb.velocity = dir * runSpeed;
                currentlyAvoidingBullet = true;
                Debug.Log("a");
            } else {
                Debug.Log("c");
                dir = Vector2.zero; // we're screwed 
                currentlyAvoidingBullet = false;           
            }
        }

    }

    
    void ApplyGravityMultipliers() { // simplified version from player version

		if (rb.velocity[1] < 0) {
			/* Multiply the force of gravity by fallMultiplier when falling */
			rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
		} 
	}

    void FixedUpdate() {
        fixedUpdateCount++;
        if (jumping) ApplyGravityMultipliers();
    }


    void Update() {

        /* Doesn't deal with physics, so call from Update */
		Vector3 wp3 = GetComponent<Transform>().position; //mainCamera.ScreenToWorldPoint(GetComponent<Transform>().position);
		Vector2 wp2 = new Vector2(wp3[0], wp3[1]);
		Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(wp2, lineOfSightDistance);


        /* Organize all relevant colliders found in the line of sight */
        List<Collider2D> liBullets = new List<Collider2D>();
        List<Collider2D> liPlayer = new List<Collider2D>();

        foreach (Collider2D c in nearbyColliders) {
            switch (c.gameObject.tag) {
                case "Laser": liBullets.Add(c); break;
                case "Player": liPlayer.Add(c); break;
            }            
        }

        /* Given view, determine behaviour. Then use that behaviour to determine actions in the state machine */
        if (liBullets.Count > 0) {
            TryToAvoidBullets(liBullets);
        }

    }

}

