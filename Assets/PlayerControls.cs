/*     Bugs that gotta be fixed  
 * --- ( a very small subset ) --- */

// Sometimes when lookForWall is pressed when the player is on the wall, he shoots off 
// in the opposite direction. I think it's an issue with the colliders on the player and the wall.
// Maybe try putting in some buffer for snapping to the wall

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour {

	public Rigidbody2D rb;
	public Camera mainCamera;

	/* Prefabs */
	public GameObject laserPrefab;
	public GameObject ropePrefab;
	public GameObject tetherPointPrefab;
	public GameObject lineRenderRopePrefab;
	public GameObject syntheticTreePrefab;
	public GameObject boostCloudPrefab;


	private const float laserSpeed = 10.0f;

	/* For debugging help */
	private int fixedUpdateCount = 0;

	private const float maxSpeed = 5.0f; // Max running speed
	private const float stopTime = 0.3f; 


	/* Climbing */
	private static KeyCode lookForWall = KeyCode.I;
	private float curClimbTime = 0f;
	private float maxClimbTime = 5f;
	private float climbSpeed = 3f;
	private bool haveStamina = true; 
	private bool onWall = false; // Whether we are currently hanging on to a wall
	private float frictionForce = 6f; // Applied in the upwards direction while slipping down a wall
	private const float climbForce = 15f; // fights against gravity, so set relatively high
	private const float maxClimbSpeed = 4f;
	private Vector2 wallDirection;



	/* Jumping */
	private bool touchingGround = false;
	private static KeyCode jumpCode = KeyCode.Space;
	private float jumpSpeed = 12f;
	private float fallMultiplier = 4f;
	private float lowJumpMultiplier = 3f;
	private float playerBonusGravity = 9.8f; // add additional gravity to get faster jumps

	/* Boosting */
	private GameObject boostCloud;
	private static KeyCode airBoost = KeyCode.A;
	private const float boostVelocity = 15f;
	private bool boosting;
	private float boostCloudTimer;
	private const float boostCloudLinger = 0.2f; // How long the cloud sprite lingers
	private bool canBoost = true;


	/* Key bindings */
	private const KeyCode primaryAttackCode = KeyCode.H;
	private const KeyCode stageSpecialCode1 = KeyCode.R;
	private const KeyCode stageSpecialCode2 = KeyCode.T;
	
	/* Tree spawning */
	private GameObject spawnedTree;
	private bool spawningTree = false; // Whether we are in the process of spawning a tree
	private float heightAdded; // Total movement from the initial spawn position
	private float spawnTreeDeltaPos = 0.03f; // Amount to move tree by at each update step
	private float updateTick = 0.01f; // Update position each updateTick seconds
	private float thisTick; // like a counter. Compare with updateTick

	private GameObject laser;

	/* 0 = city, 1 = jungle, 2 = mountain, 3 = ice, 4 = temple */
	private int stage = 1;

	/* Swinging logic */
	private bool tethered = false; // If the player is currently hanging on to a rope
	private static KeyCode toggleTether = KeyCode.Y;
	private static KeyCode lengthenRope = KeyCode.DownArrow; // When swinging, can move up/down the rope
	private static KeyCode shortenRope = KeyCode.UpArrow;
	private const float deltaRopeLength = 0.5f; // How much to climb or descend the rope
	private float ropeLength  = 5f;
	private const float tetherPointClickRadius = 2f; // Search this radius surrounding the mouse position at time of click
	
	private GameObject tetherPoint; // The point we are latching onto
	private GameObject rope; // The rope attaching the player to the vine

	private HingeJoint2D hinge_A, hinge_B; // Updated each time we change the location of the player manually by setting ropelength
	private bool tetherQueued = false; // Set to true when toggleTether clicked. Call SetHingeJoints() when player is no longer touching ground
	private GameObject lineRender; // For drawing ropes

	private float minRopeLength = 0.5f; // Don't allow the rope to become shorter than this
	private float maxSwingSpeed = 10f;



	/* Since Update() and FixedUpdate() work at different rates, capture all input in these lists, or'ing values together
	 * then resetting them at the next fixed update step */
	private static readonly List<KeyCode> getKeyList = new List<KeyCode>(new KeyCode[] 
		{ KeyCode.RightArrow, KeyCode.LeftArrow, jumpCode, lengthenRope, shortenRope });
	private static readonly List<KeyCode> getKeyDownList = new List<KeyCode>(new KeyCode[]
		{ jumpCode, primaryAttackCode, stageSpecialCode1, stageSpecialCode2, toggleTether, airBoost });
	
	private Dictionary<KeyCode, bool> getKey = new Dictionary<KeyCode, bool>(); // init in Start
	private Dictionary<KeyCode, bool> getKeyDown = new Dictionary<KeyCode, bool>();



	/* Maintain face = 1 if facing right, and face = 0 if facing left */
	private string facing = "right";



	void PrimaryAttack() {

		/* Spawn an attack and run it. May want to move this input capture to update as well */
		Vector3 vel = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - gameObject.GetComponent<Transform>().position);
		vel = new Vector3(vel[0], vel[1], 0.0f); // Disregard z-direction
		vel = vel.normalized;
		Vector3 sz = Utils.GetActualBC2DSize(gameObject) / 2;
		Vector3 delta = new Vector3(sz[0], 0.0f, 0.0f);
		delta = delta * ((vel[0] > 0.0f) ? 1 : -1);
		Vector3 laserSpawnPosition = GetComponent<Transform>().position + vel + delta;
		laser = Instantiate(laserPrefab, laserSpawnPosition, Quaternion.identity);
		vel = vel * laserSpeed;
		laser.GetComponent<Rigidbody2D>().velocity = vel;
		
	}

	private void BeginSpawnTree(Vector3 point) {

		/* Initialize the tree sprite in the ground */
		heightAdded = 0.0f;
		thisTick = 0.0f;
		spawningTree = true;
		Vector2 sz = Utils.GetActualBC2DSize(syntheticTreePrefab);
		Vector3 startPoint = new Vector3(point[0], point[1] - sz[1] / 2, 0.0f);
		spawnedTree = Instantiate(syntheticTreePrefab, startPoint, Quaternion.identity);

		
	}

	private void UpdateSpawnTree() {

		Debug.Log("update spawn tree");

		/* Perform update every 'updateTick' seconds */
		if (thisTick < updateTick) {
			thisTick += Time.deltaTime;
			return;
		} else {
			thisTick = 0.0f; // reset the tick counter
		}

		/* Increment tree y-position */
		if (heightAdded >= Utils.GetActualBC2DSize(syntheticTreePrefab)[1] / 2) {
			spawningTree = false;
			Debug.Log("finished");
		}
		
		else {
			spawnedTree.GetComponent<Transform>().position += (Vector3.up * spawnTreeDeltaPos);
			heightAdded += spawnTreeDeltaPos;
		}
	}


	/* Method: StageSpecial1
	 * ---------------------
	 * I'm thinking each stage will have one or two unique mechanics tailored to the setting, which may be triggered
	 * by the methods StageSpecial(1/2) */
	void StageSpecial1() {

		Debug.Log("StageSpecial1");

		if (stage == 1) {
			/* Jungle Stage - causes a tree to grow at the mouse coordinates */
			Vector3 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
			Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPoint);
			if (viewportPoint[0] >= 0.0 && viewportPoint[0] <= 1.0 && viewportPoint[1] >= 0.0 && viewportPoint[1] <= 1.0) {
				BeginSpawnTree(worldPoint);
			}
		}
	}

	void StageSpecial2() { return; }

	/* Method: HandleActions
	 * --------------------- */
	void HandleActions() {

		if (getKeyDown[primaryAttackCode])
			PrimaryAttack();
		if (getKeyDown[stageSpecialCode1])
			StageSpecial1();

	}


	void HorizontalMotionUpdate() {

		if (!touchingGround && !tethered) return;

		/* Get direction and clamp magnitude at maxSpeed */
		if (getKey[KeyCode.RightArrow])
			rb.AddForce(45 * Vector2.right);
		if (getKey[KeyCode.LeftArrow]) 
			rb.AddForce(45 * Vector2.left);
		

		/* Set max speed dependent on whether we are swinging or not */
		float clampSpeed = tethered ? maxSwingSpeed : maxSpeed;

		/* Clamp the x velocity */
		float vx = Mathf.Clamp(rb.velocity[0], -clampSpeed, clampSpeed);
		rb.velocity = new Vector2(vx, rb.velocity[1]);

		/* Update facing. Do nothing if velocity == 0 */
		if (rb.velocity[0] > 0.01f) facing = "right";
		else if (rb.velocity[0] < -0.01f) facing = "left";

	}


	void ApplyGravityMultipliers() {

		if (rb.velocity[1] < 0) {
			/* Multiply the force of gravity by fallMultiplier when falling */
			rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
		} else if (rb.velocity[1] > 0 && !getKey[jumpCode]) {
			/* If we are going up and have let go of the jump key, jump lower (apply higher gravity) */
			rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;

		} 
	}


	void ApplyWallJump() {

		/* Get the wall that we're climbing, and add velocity directed away from that wall */
		float wallJumpSpeed = 5f;
		if (wallDirection == Vector2.left) {
			Debug.Log("a");
			rb.velocity = (Vector2.right + Vector2.up) * wallJumpSpeed;
		} else if (wallDirection == Vector2.right) {
			Debug.Log("b");
			rb.velocity = (Vector2.left + Vector2.up) * wallJumpSpeed;
		}
		
		/* No longer on wall, so apply gravity */
		onWall = false;
		rb.gravityScale = 1f;

	}	




	void ApplyAirBoost() {

		Debug.Log("boost " + fixedUpdateCount);
		Vector2 airDirection = Vector2.zero;
		if (getKey[KeyCode.LeftArrow]) 	airDirection += Vector2.left;
		if (getKey[KeyCode.RightArrow]) airDirection += Vector2.right;
		if (getKey[KeyCode.UpArrow]) 	airDirection += Vector2.up;
		if (getKey[KeyCode.DownArrow]) 	airDirection += Vector2.down;

		rb.velocity += boostVelocity * airDirection.normalized;

		float boostOffset = 0.5f;
		Vector3 pos = GetComponent<Transform>().position; 
		Vector2 pos2d = new Vector2(pos[0], pos[1]);
		boostCloud = Instantiate(boostCloudPrefab, pos2d + airDirection * boostOffset, Quaternion.identity);
		boosting = true;
	}

	void VerticalMotionUpdate() {

		if (getKeyDown[airBoost] && canBoost) { 
			canBoost = false;
			ApplyAirBoost();
		}

		if (getKeyDown[jumpCode] && touchingGround)
			rb.velocity += Vector2.up * jumpSpeed;
		
		ApplyGravityMultipliers();
		
		if (!touchingGround) 
			rb.velocity -= Vector2.up * playerBonusGravity * Time.deltaTime; 


	}

	void CheckTouchingGround() {

		/* Since we use a box collider, check both bottom corners for collisions with the ground */

		float delta = Utils.GetActualBC2DSize(gameObject)[1] / 2 + 0.01f;
		float delta2 = Utils.GetActualBC2DSize(gameObject)[0] / 2 + 0.05f; // 0.1f to give a bit of buffer
		
		/* Check the bottom left corner and the bottom right corner for raycast hits with the ground */
		Vector3 posLeft = GetComponent<Transform>().position + delta * Vector3.down + delta2 * Vector3.left;
		Vector3 posRight = GetComponent<Transform>().position + delta * Vector3.down + delta2 * Vector3.right;
		float rayLength = 0.05f;
		RaycastHit2D hitLeft = Physics2D.Raycast(posLeft, Vector2.down, rayLength);
		RaycastHit2D hitRight = Physics2D.Raycast(posRight, Vector2.down, rayLength);
		if (hitLeft.collider != null || hitRight.collider != null) {
			touchingGround = true;
			canBoost = true; // reset canBoost each time we touch the ground
		} else {
			touchingGround = false;
		}

	}

	void ClearInputLists() {
		
		/* Set all entries in our keypress dictionaries to false */
		foreach (KeyCode kc in getKeyList)
			getKey[kc] = false;
		foreach (KeyCode kc in getKeyDownList)
			getKeyDown[kc] = false;

	}

	void CaptureInput() {

		/* Store all pressed keys that we care about in the relevant dictionaries for processing at the
		 * next FixedUpdate step */
		foreach (KeyCode kc in getKeyList)
			getKey[kc] = getKey[kc] | Input.GetKey(kc);
		foreach (KeyCode kc in getKeyDownList)
			getKeyDown[kc] = getKeyDown[kc] | Input.GetKeyDown(kc);

	}


	void SnapToWall(Collider2D c) {

		Debug.Log("snap to wall " + fixedUpdateCount);

		/* We found a climable wall, so the player should move as near as possible to it */
		float wallColumn = c.GetComponent<Transform>().position[1];
		float playerColumn = GetComponent<Transform>().position[1];

		float diff = wallColumn - playerColumn; // Move left if < 0 else move right
		wallDirection = (diff < 0) ? Vector2.left : Vector2.right;

		float wallSnapImpulse = 100f;
		rb.AddForce(wallSnapImpulse * wallDirection, ForceMode2D.Impulse);
		
	}


	bool LookForWall() {

		/* Like SelectSwingTetherPoint, call from Update */
		Vector3 worldPoint = GetComponent<Transform>().position;
		Vector2 worldPoint2 = new Vector2(worldPoint[0], worldPoint[1]);

		float wallGrabRadius = Utils.GetActualBC2DSize(gameObject)[1] / 2 + 0.1f; // half the player width plus a bit
		Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(worldPoint2, wallGrabRadius);

		foreach (Collider2D c in nearbyColliders) {
			
			if (c.gameObject.tag == "ClimbableWall") {

				/* We found a climable wall, so the player should move as near as possible to it */
				float wallColumn = c.GetComponent<Transform>().position[1];
				float playerColumn = GetComponent<Transform>().position[1];

				float diff = wallColumn - playerColumn; // Move left if < 0 else move right
				wallDirection = (diff > 0) ? Vector2.left : Vector2.right;
				float wallSnapImpulse = 100f;
				rb.AddForce(wallSnapImpulse * wallDirection, ForceMode2D.Impulse);
				
				return true;
			}
		}
		return false;

	}


	bool SelectSwingTetherPoint() {

		/* Doesn't deal with physics, so call from Update */
		Vector3 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		Vector2 worldMousePos = new Vector2(worldPoint[0], worldPoint[1]);
		Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(worldMousePos, tetherPointClickRadius);

		foreach (Collider2D c in nearbyColliders) {
			if (c.gameObject.tag == "TetherPoint") {
				Vector3 pos = c.GetComponent<Transform>().position;
				tetherPoint = c.gameObject;
				return true;
			}
		}
		return false;

	}


	void UpdateClimbingPosition() {

		/* Handle application of forces for climbing */

		curClimbTime += Time.fixedDeltaTime; // Use fixedDeltaTime rather than deltaTime inside of FixedUpdate

		haveStamina = curClimbTime < maxClimbTime;

		if (haveStamina) {

			/* Still climbing */
			//Vector2 climbForceVec = Vector2.zero;
			if (getKeyDown[jumpCode]) {
				ApplyWallJump();
			} else if (getKey[KeyCode.UpArrow]) { 
				rb.velocity = climbSpeed * Vector2.up;
			} else if (getKey[KeyCode.DownArrow]) {
				rb.velocity = climbSpeed * Vector2.down;
			} else {
				rb.velocity = Vector2.zero; // holding in place
			}
		
		} else {
			
			/* Out of stamina, sliding down wall */
			rb.gravityScale = 1f;
			rb.AddForce(Vector2.up * frictionForce);

		}

	}




	void UpdateSwingingPosition() { 

		if (!tethered) return;

		bool ropeLengthChanged = false;

		/* Adjust for climbing/descending the rope */
		if (getKey[lengthenRope]) {
			Debug.Log("lengthen rope ");
			ropeLength += deltaRopeLength;
			ropeLengthChanged = true;
		} 
		if (getKey[shortenRope]) {
			Debug.Log("shorten rope ");
			ropeLength -= deltaRopeLength;
			if (ropeLength < minRopeLength) 
				ropeLength = minRopeLength;
			ropeLengthChanged = true;
		}

		if (ropeLengthChanged) {

			/* If the rope length is changed, manually update the position of the transform */

			Vector3 diff = GetComponent<Transform>().position - tetherPoint.GetComponent<Transform>().position;
			Vector2 diff2 = new Vector2(diff[0], diff[1]);
			Vector2 delta = diff2.normalized * ropeLength; // gets direction
			Vector3 delta3 = new Vector3(delta[0], delta[1], 0f);
			GetComponent<Transform>().position = tetherPoint.GetComponent<Transform>().position + delta3;

			/* Destroy old hinge joints */
			Destroy(hinge_A);
			Destroy(hinge_B);

			/* Add new hinge joints */
			SetHingeJoints();
		
		}

		UpdateRope();



	}

	void SetHingeJoints() {

		/*  https://forum.unity.com/threads/swinging-ninja-rope.227628/  */		
		hinge_A = gameObject.AddComponent<HingeJoint2D>(); // on the player
		hinge_B = tetherPoint.AddComponent<HingeJoint2D>(); // on the tether
		hinge_A.connectedBody = tetherPoint.GetComponent<Rigidbody2D>();

	}


	void SwingError() { /* Show miss in trying to attach swing rope */ }

	void UpdateRope() {
		/* Update the line renderer so that its endpoints join the player and the tether */
		Vector2 zeroedPosition = new Vector2(GetComponent<Transform>().position[0], GetComponent<Transform>().position[1]);
		Vector3[] positions = new Vector3[] { zeroedPosition, tetherPoint.GetComponent<Transform>().position };
		lineRender.GetComponent<LineRenderer>().SetPositions(positions);
	}

	void InitRope() {
		/* Zero out the z-dimension and spawn a new instance of the line render rope prefab */
		Vector3 zeroedPosition = new Vector3(GetComponent<Transform>().position[0], GetComponent<Transform>().position[1], 0f);
		Vector3 tetherPosition = tetherPoint.GetComponent<Transform>().position;
		Vector3 ropeMidpoint = zeroedPosition + (tetherPosition - zeroedPosition) / 2;
		lineRender = Instantiate(lineRenderRopePrefab, ropeMidpoint, Quaternion.identity);
		UpdateRope();
	}

	

	void SetRope() {

		Vector3 playerPosition = GetComponent<Transform>().position;
		Vector3 tetherPosition = tetherPoint.GetComponent<Transform>().position;
		Vector3 ropeMidpoint = playerPosition + (tetherPosition - playerPosition) / 2;

		Vector3 diff = tetherPosition - playerPosition;
		Vector2 diff2 = new Vector2(diff[0], diff[1]);
		Debug.Log("diff = " + diff2);
		rope = Instantiate(ropePrefab, ropeMidpoint, Quaternion.identity);

		/* Now compute x and y scalings */
		float desiredHeight = diff2.magnitude;
		float desiredWidth = 1f;
		float actualHeight = Utils.GetSizeFromSpriteRenderer(rope)[1];
		float actualWidth = Utils.GetSizeFromSpriteRenderer(rope)[0];
		float heightScale = desiredHeight / actualHeight;
		float widthScale = desiredWidth / actualWidth;

		rope.GetComponent<Transform>().localScale = new Vector3(widthScale, heightScale, 1f);		

		ropeLength = desiredHeight; 

		/* Rotate about the z-axis. From the documentation, Transform.rotate 
		 * applies a rotation of eulerAngles.z degrees around the z axis, eulerAngles.x degrees around the x axis, 
		 * and eulerAngles.y degrees around the y axis (in that order). 
		 */
		float zRotation =  (Mathf.Atan(diff2[0] / diff2[1])) * Mathf.Rad2Deg;
		rope.GetComponent<Transform>().Rotate(new Vector3(0f, 0f, -zRotation));

	}


	/* 
		Climb methodology: 
		
		If KeyCode.lookForWall is pressed, then call LookForWall, which attempts to lock onto a wall
		If we find a wall in LookForWall, then set onWall to true. All movement behaviour is then determined by UpdateClimbinb position		
	
	 */


	void FixedUpdate() {

		/* Occurs at a measured timestep that does not coincide with Update(). Use for movement*/
		fixedUpdateCount++;

		CheckTouchingGround();

		
		if (onWall) {
			
			/* Restrict movement to wall climbing logic */
			UpdateClimbingPosition();

		} 
		else if (tetherQueued && !touchingGround) {
			
			/* Start swinging and take first swing movement step */
			tetherQueued = !tetherQueued;
			tethered = true;
			InitRope();
			SetHingeJoints();
			UpdateSwingingPosition();
			HorizontalMotionUpdate();
		}
		else if (tethered) {
			
			/* Handle vine-swinging */
			UpdateSwingingPosition();
			HorizontalMotionUpdate();
			
		} else {
			
			/* Non-swinging - covers all jumping and L/R movement */
			VerticalMotionUpdate();
			HorizontalMotionUpdate();
		}
		
		ClearInputLists();

	}


	void Update() {

		CaptureInput();
		HandleActions();
		if (spawningTree) {
			/* Continue spawning the tree */
			UpdateSpawnTree();
		}
		
		/* Attempt to snap to wall when the corresponding keycode is pressed */
		if (Input.GetKeyDown(lookForWall) && !onWall) {
			onWall = LookForWall();
			if (onWall) {
				/* Start physics settings and variables */
				curClimbTime = 0f;
				rb.gravityScale = 0f;
			}
		} else if (!Input.GetKey(lookForWall)) {
			onWall = false;
			rb.gravityScale = 1f; // reapply gravity
		}

		
		
		if (Input.GetKeyDown(toggleTether) && !tethered) {
			
			/* Start swinging */
			bool foundSwingPoint = SelectSwingTetherPoint();
			Debug.Log("Found swing point: " + foundSwingPoint);
			if (!foundSwingPoint) {
				SwingError();
			} else {
				tetherQueued = true; // set tether on next fixed update
 			}
		} else if (Input.GetKeyDown(toggleTether) && tethered) {
			
			/* Stop swinging */
			tethered = false;
			Destroy(lineRender);
			Destroy(hinge_A);
			Destroy(hinge_B);
		}


	}

	void Start() {
		rb.freezeRotation = true;

		/* Initialize key capture lists */
		foreach (KeyCode c in getKeyList) 
			getKey[c] = false;
		foreach (KeyCode c in getKeyDownList) 
			getKeyDown[c] = false;

	}
}
