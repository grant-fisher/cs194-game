using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Rigidbody2D rb;
	public Camera mainCamera;

	/* For debugging help */
	private int fixedUpdateCount = 0;

	#region prefabs
	public GameObject BulletPrefab;
	public GameObject TetherPointPrefab;
	public GameObject DashCloudPrefab;
	#endregion

    private GameObject DashCloud;


	private const float BulletSpeed = 10.0f;



	private const float maxSpeed = 5.0f; // Max running speed
	private const float stopTime = 0.3f; 

	private float JumpSpeed = 12f;

	private const float DashVelocity = 15f;
	private bool Dashing;
	private float DashCloudTimer;
	private const float DashCloudLinger = 0.2f; // How long the cloud sprite lingers

	private GameObject bullet;

	private float minRopeLength = 0.5f; // Don't allow the rope to become shorter than this
	private float maxSwingSpeed = 10f;


    private float MaxClimbTime = 1f;

	// Maintain face = 1 if facing right, and face = 0 if facing left
	private string facing = "right";


	public Input_ Input_;

	public StateMachine StateMachine;
	public const int StNormal = 0;
	public const int StClimb = 1;
	public const int StDash = 2;
	public const int StSlide = 3;
	public const int StSwing = 4;


    private const float MaxRunSpeed = 8f;


    private const bool PRINT_METHOD_CALL = true;

	void PrimaryAttack() {

        if (PRINT_METHOD_CALL) Debug.Log("PrimaryAttack");

		// Create a unit normalized vector pointing in the direction from the center of the player sprite to the current
		// world position of the mouse
		var vel = Utils.Vec3ToVec2(mainCamera.ScreenToWorldPoint(Input.mousePosition) - gameObject.GetComponent<Transform>().position).normalized;

		var sz = Utils.GetActualBC2DSize(gameObject) / 2;

		// Delta is an offset from the player center, where we choose to spawn the sprite
		// todo: unsure if necessary
		var delta = new Vector2(sz.x, 0.0f);
		delta = delta * ((vel[0] > 0.0f) ? 1 : -1);
		
		// Determine the point at which to spawn the bullet
		// Instantiate(...) creates a new bullet game object using the bulletPrefab 
		var bulletSpawnPosition = Utils.Vec3ToVec2(GetComponent<Transform>().position) + vel + delta;
		bullet = Instantiate(BulletPrefab, bulletSpawnPosition, Quaternion.identity);

		// Scale the unity velocity by bullet speed and set the velocity of the newly instantiated bullet
		vel = vel * BulletSpeed;
		bullet.GetComponent<Rigidbody2D>().velocity = vel;
		
	}


	private bool CanJump()
	{
        if (PRINT_METHOD_CALL) Debug.Log("CanJump");

		return GroundCheck();
	}

	void FixedUpdate()
	{
		fixedUpdateCount++;
		StateMachine.Update();

		if (StateMachine.State == StSwing)
		{
			if (positionChanged)
			{
				// Set the transform to the new position
				GetComponent<Transform>().position = newPosition;
			}
			// Update the line render endpoints at the end of each step
			lineRender.GetComponent<LineRenderer>().SetPositions(lineRenderEndpoints);
			rb.AddForce(SwingForce);
		}

		rb.AddForce(Vector2.right * MoveForce);
        var clampedVelocity = new Vector2(Mathf.Clamp(rb.velocity[0], -MaxRunSpeed, MaxRunSpeed), rb.velocity[1]);
        rb.velocity = clampedVelocity;

	}


	// called by unity
	void Update() 
	{
        if (PRINT_METHOD_CALL) Debug.Log("Update");

		Input_.Capture();


		if (StateMachine.State == StClimb)
		{
			ClimbTimer += Time.deltaTime;
			HasStamina = ClimbTimer < MaxClimbTime;
		}

	}


	private bool CanDash()
	{
		return StateMachine.State == StNormal;
	}


	#region normal
	private float FallMultiplier = 4f;
	private float LowJumpMultiplier = 3f;
	private float PlayerBonusGravity = 9.8f; // add additional gravity to get faster jumps
	private int Facing = 0; // Todo: Should be 1 or -1
    private float MoveForce = 0f;
    private float deltaMoveForce = 1f; // How much force is added in the horizontal direction at each step

	private void NormalBegin()
	{

	}
	private IEnumerator NormalCoroutine()
	{
		yield return null;
	}
	private void NormalEnd()
	{
		
	}

	private void NormalUpdate()	
	{	       
        if (PRINT_METHOD_CALL) Debug.Log("NormalUpdate");
 
        if (Input_.MoveX.Value != 0)
        {
            MoveForce += deltaMoveForce * Input_.MoveX.Value;
        }
        else
        {
            MoveForce = 0f;
        }

		if (Input_.Attack.Down)
		{
			PrimaryAttack();
		} 

		// Todo: if we want to add the option for an instant wall jump, then these branches
		// will not be mutually exclusive, but we'll need to make sure that we coordinate the switch
		// to the climb state with the application of the jumps
		if (Input_.Grab.Pressed && WallCheck())
		{
			StateMachine.State = StClimb;
		}
		else if (Input_.Jump.Down && CanJump())
		{
			rb.velocity += Vector2.up * JumpSpeed; 
		}

		if (Input_.Dash.Down && CanDash())
		{
			StateMachine.State = StDash;
		}	



		// Begin apply Gravity multipliers
		// https://www.youtube.com/watch?v=7KiK0Aqtmzc 
        if (rb.velocity.y < 0)
		{			
			rb.velocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1) * Time.deltaTime;
		}
		else if (rb.velocity.y > 0 && !Input_.Jump.Pressed) 
		{
			rb.velocity += Vector2.up * Physics2D.gravity.y * (LowJumpMultiplier - 1) * Time.deltaTime;
		} 
		// End gravity multipliers
	}
	#endregion

	#region jumping


	private const float WallJumpSpeed = 5f;
	private void WallJump(int dir) 
	{
        if (PRINT_METHOD_CALL) Debug.Log("WallJump");

		// Jump in the direction of dir at 45 degree angle
		var wallJumpTrajectory = new Vector2(dir, 1f); 
		rb.velocity = wallJumpTrajectory * WallJumpSpeed;
		StateMachine.State = StNormal;
	}

	#endregion
	
	#region climbing
	private int wallDir; // -1 to left, +1 to right
	private const float ClimbSpeed = 3f;
	private bool HasStamina = true;
    private float ClimbTimer;

	private void ClimbBegin()
	{
        if (PRINT_METHOD_CALL) Debug.Log("ClimbBegin");

		ClimbTimer = 0f;
		rb.velocity = Vector2.zero;

		// Nullify gravity on the player while climing for simplicity
		rb.gravityScale = 0f;
	}
	private IEnumerator ClimbCoroutine()
	{
		yield return null;
	}
	private void ClimbUpdate()	
	{
        if (PRINT_METHOD_CALL) Debug.Log("ClimbUpdate");

		if (Input_.Jump.Pressed)
		{
			WallJump(-wallDir);
			StateMachine.State = StNormal;
		}
		else if (!Input_.Grab.Pressed)
		{
			// Free falling down the wall
			StateMachine.State = StNormal;
		}

		else // Continue climbing
		{
			if (HasStamina)
			{
				// Move up or down the wall as we wish
				rb.velocity = new Vector2(0f, Input_.MoveY.Value);
				rb.velocity *= ClimbSpeed;
			} 
			else 
			{
				// Start sliding down the wall
				StateMachine.State = StSlide;
			}
		}
	}

	private void ClimbEnd()
	{
        if (PRINT_METHOD_CALL) Debug.Log("ClimbEnd");

		ClimbTimer = 0f;
		rb.gravityScale = 1f;
	}
	#endregion



	// StSlide handles sliding down the wall
	#region wallslide
	private float wallFrictionForce = 6f; // applied in upwards direction
	private void SlideBegin() 
	{
		// Reapply gravity at the start of the slide
		rb.gravityScale = 1f;
	}
	private IEnumerator SlideCoroutine()
	{
		yield return null;
	}
	private void SlideUpdate()
	{
		if (Input_.Jump.Pressed)
		{
			// Execute wall jump
			WallJump(-wallDir);
			StateMachine.State = StNormal;
		}
		else if (!Input_.Grab.Pressed || GroundCheck())
		{
			// We either let go of the wall, or are touching the ground
			StateMachine.State = StNormal;
		}
		else 
		{
			// We are still sliding, so apply the frictional force
			Debug.Log("---- " + Vector2.up * wallFrictionForce + " " + rb.gravityScale + " " + Physics2D.gravity);
			rb.AddForce(Vector2.up * wallFrictionForce);
		}
	}
	private void SlideEnd()
	{
		// No special behaviour for ending the slide
	}
	#endregion







	#region dashing
	// Check Dash timer and apply updates to velocity

	private float DashTime;
	private const float kDashTime = 0.8f;
	
	private Vector2 DashDir;
    private const float dashSpeed = 10f;
	private Vector2 preDashSpeed;


	// coroutines are called after update

	private IEnumerator DashCoroutine()
	{
        if (PRINT_METHOD_CALL) Debug.Log("DashCoroutine");

		// Maintain dash velocity for kDashTime
		while (DashTime < kDashTime)
		{
			Debug.Log("DT: " + DashTime + " " + kDashTime);
			if (Input_.Grab.Pressed && WallCheck())
			{
				rb.velocity = Vector2.zero;
				StateMachine.State = StClimb;
				yield break; 
			}
			else if (GroundCheck())
			{
				Debug.Log("yeet");
				rb.velocity = Vector2.zero;
				StateMachine.State = StNormal;
				yield break;
			}
			else
			{
				DashTime += Time.deltaTime;
				yield return null;
			}
			
		}

		// Dash is completed while we are in the air, 
		// so set state to normal and return the x component of velocity to its pre-dash setting
		rb.velocity = new Vector2(preDashSpeed.x, rb.velocity.y);
		StateMachine.State = StNormal;
		yield return null;
	}


	

	private void DashBegin() 
	{
        if (PRINT_METHOD_CALL) Debug.Log("DashBegin");

		DashTime = 0f;
		preDashSpeed = rb.velocity;
		DashDir = new Vector2(Input_.MoveX.Value, Input_.MoveY.Value);
		rb.velocity = DashDir * dashSpeed;

		var dcpos = Utils.Vec3ToVec2(GetComponent<Transform>().position);
		DashCloud = Instantiate(DashCloudPrefab, dcpos, Quaternion.identity);

		int degrees; // Measured counterclockwise from Vector2.right, since the cloud looks like an "0" shape
		switch (Input_.MoveX.Value.ToString() + "|" + Input_.MoveY.Value.ToString())
		{
			case "1|0": 	degrees = 0; 	break;
			case "1|1": 	degrees = 45; 	break;
			case "0|1": 	degrees = 90; 	break;
			case "-1|1": 	degrees = 135; 	break;
			case "-1|0": 	degrees = 180; 	break;
			case "-1|-1": 	degrees = 225; 	break;
			case "0|-1": 	degrees = 270; 	break;
			case "1|-1": 	degrees = 315; 	break;
			default: 		degrees = 0; 	break;
		}
		
		var rotation = new Vector3(0f, 0f, degrees);
		DashCloud.GetComponent<Transform>().Rotate(rotation);

	}

	private void DashEnd() 
	{
        if (PRINT_METHOD_CALL) Debug.Log("DashEnd");
	}

    private void DashUpdate()
    {
        if (PRINT_METHOD_CALL) Debug.Log("DashUpdate " + fixedUpdateCount);
    }
	#endregion

	#region swinging
	private LineRenderer lineRender;
	private float kSwingForce;
	private const float kDeltaRopeLength = 0.1f;
	private Vector3[] lineRenderEndpoints;
	private Vector3 newPosition;
	private HingeJoint2D hinge_A;
	private HingeJoint2D hinge_B;
	private bool positionChanged = false;
	private GameObject tetherPoint;
	private Vector2 SwingForce;

	void SetHingeJoints() {
        if (PRINT_METHOD_CALL) Debug.Log("SetHingeJoints");

		// https://forum.unity.com/threads/swinging-ninja-rope.227628/ 	
		// connect hinges between the player and the selected TetherPoint
		hinge_A = gameObject.AddComponent<HingeJoint2D>(); // on the player
		hinge_B = tetherPoint.AddComponent<HingeJoint2D>(); // on the tether
		hinge_A.connectedBody = tetherPoint.GetComponent<Rigidbody2D>();

	}

	private void SetRopeEndpoints()
	{
        if (PRINT_METHOD_CALL) Debug.Log("SetRopeEndpoints");

		lineRenderEndpoints = new Vector3[] { Utils.Vec3ToVec2(GetComponent<Transform>().position), tetherPoint.GetComponent<Transform>().position };
	}

	void SwingBegin() 
	{
        if (PRINT_METHOD_CALL) Debug.Log("SwingBegin");

		SetRopeEndpoints();
		SetHingeJoints();

	}

	private IEnumerator SwingCoroutine() 
	{
        if (PRINT_METHOD_CALL) Debug.Log("SwingCoroutine");

        while (StateMachine.State == StSwing)
        {
            // Let MoveX still denote swinging motion from side to side
            // When swinging, handle with forces rather than by setting velocities
            SwingForce = Vector2.right * Input_.MoveX.Value * kSwingForce;

            if (Input_.MoveY.Value != 0) 
            {
                // Move the player closer to or further from the hinge
                // Delay execution until next call to Update()
                var diff = Utils.Vec3ToVec2(tetherPoint.GetComponent<Transform>().position - GetComponent<Transform>().position).normalized;
                newPosition = Utils.Vec3ToVec2(GetComponent<Transform>().position) + diff * kDeltaRopeLength;
                positionChanged = true;

                // Destroy old hinge joints
                Destroy(hinge_A);
                Destroy(hinge_B);

                // Add new hinge joints
                SetHingeJoints(); 
            }
            else
            {
                positionChanged = false;
            }
            yield return null;
        }
		yield return null;

	}
	private void SwingUpdate()	
	{
        if (PRINT_METHOD_CALL) Debug.Log("SwingUpdate");

	}
	private void SwingEnd()
	{
        if (PRINT_METHOD_CALL) Debug.Log("SwingEnd");

		positionChanged = false;
	}
	#endregion



    //https://gist.github.com/jbroadway/b94b971d224332f9158988a66f35f22d
    //https://answers.unity.com/questions/546668/how-to-create-coroutine-delegates.html
	void Start() 
	{
        if (PRINT_METHOD_CALL) Debug.Log("Start");

		StateMachine = new StateMachine(this);

        // Since our update methods are all of type void, pass them as delegates
        // Pass coroutines as illustrated in the second link

        UDelegateFn dNormalUpdate = NormalUpdate, dNormalBegin = NormalBegin, dNormalEnd = NormalEnd;
		System.Action l_normalCoroutine = () => StartCoroutine(NormalCoroutine());
		StateMachine.SetCallbacks(StNormal, dNormalUpdate, l_normalCoroutine, dNormalBegin, dNormalEnd);

        UDelegateFn dClimbUpdate = ClimbUpdate, dClimbBegin = ClimbBegin, dClimbEnd = ClimbEnd;
		System.Action l_climbCoroutine = () => StartCoroutine(ClimbCoroutine());
		StateMachine.SetCallbacks(StClimb, dClimbUpdate, l_climbCoroutine, dClimbBegin, dClimbEnd);

        UDelegateFn dDashUpdate = DashUpdate, dDashBegin = DashBegin, dDashEnd = DashEnd;
		System.Action l_dashCoroutine = () => StartCoroutine(DashCoroutine());
		StateMachine.SetCallbacks(StDash, DashUpdate, l_dashCoroutine, DashBegin, DashEnd);
		
        UDelegateFn dSwingUpdate = SwingUpdate, dSwingBegin = SwingBegin, dSwingEnd = SwingEnd;
		System.Action l_swingCoroutine = () => StartCoroutine(SwingCoroutine());
        StateMachine.SetCallbacks(StSwing, SwingUpdate, l_swingCoroutine, SwingBegin, SwingEnd);

		UDelegateFn dSlideUpdate = SlideUpdate, dSlideBegin = SlideBegin, dSlideEnd = SlideEnd;
		System.Action l_slideCoroutine = () => StartCoroutine(SlideCoroutine());
		StateMachine.SetCallbacks(StSlide, dSlideUpdate, l_slideCoroutine, dSlideBegin, dSlideEnd);

        // Set up our input class instance
		Input_ = new Input_();

        // Freeze rotation on the player
		rb.freezeRotation = true;

		StateMachine.State = StNormal;

	}


	private const int MASK_WALL_LAYER = (1 << 10);
	private const int MASK_GROUND_LAYER = (1 << 11);

	private bool WallCheck()
	{ 
        if (PRINT_METHOD_CALL) Debug.Log("WallCheck");

		// Call this method after the physics update step
		return Physics2D.IsTouchingLayers(GetComponent<Collider2D>(), MASK_WALL_LAYER);
	}

	private bool GroundCheck()
	{

        if (PRINT_METHOD_CALL) Debug.Log("GroundCheck");

        //Debug.Log("Ground check: " + Physics2D.IsTouchingLayers(GetComponent<Collider2D>(), MASK_GROUND_LAYER));
		return Physics2D.IsTouchingLayers(GetComponent<Collider2D>(), MASK_GROUND_LAYER);
	}

	// Define global variables for all physics-related information
	// Within update step, modify these variables
	// Within fixed update step, apply these variables

	

}
