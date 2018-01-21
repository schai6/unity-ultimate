using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LaunchFrisbee : MonoBehaviour 
{
	Rigidbody rigidBody;

	//==============================================================
	// Mousebuttons
	//==============================================================
	const int LEFTCLICK = 0;
	const int RIGHTCLICK = 1;

	//==============================================================
	// Game Transforms
	//==============================================================
	[SerializeField] private Transform Player; // Holder of the Disc
	[SerializeField] private Transform Basket; // The DiscGolfBasket
	[SerializeField] private Transform Disc; // The Frisbee

	//==============================================================
	// Disc Initial Values
	//==============================================================
	private Vector3 discInitialPosition; // Disc initial position related to Player
	private Quaternion discInitialRotation; // Disc initial rotation related to Player

	//==============================================================
	// Camera Initial Values
	//==============================================================
	private Vector3 camInitialPosition; // Camera initial position
	private Quaternion camInitialRotation; // Camera initial rotation

	//==============================================================
	// Physics
	//==============================================================
	public Vector3 force; // The force applayed on XYZ
	private float playerThrust; // Get thrust in percent from ThrustBar script 0-100 %
	public float curveAmount = 0.0f; // The disc turn
	public float maxSpeed = 30f; // Max speed the disc can have
	public float Gravity = -2.0f; // For lower speed and longer throws (Orignial -9.81)
	private float rotateSpeed; // Speed of the disc rotation

	//==============================================================
	// Booleans
	//==============================================================
	private bool isThrown = false; // Is the disc in the air
	private bool isGrounded = false; // Is the disc on the ground
	private bool isRotate = false; // If the disc in the air it rotates

	//==============================================================
	// Tilt and pan the disc with right mouse button pressed
	//==============================================================
	private float xTilt = 0.0f; // Up , down
	private float zTilt = 0.0f; // Left, right

	private Quaternion tiltCurrentRotation;
	private Quaternion tiltDesiredRotation;

	public int xTiltMin = -40;
	public int xTiltMax = 40;
	public int zTiltMin = -40;
	public int zTiltMax = 40;
	private float tiltSpeed = 200.0f;

	private float yPan = 0.0f;

	private Quaternion panCurrentRotation;
	private Quaternion panDesiredRotation;

	public int maxPanLeft = -30;
	public int maxPanRight = 30;
	private float panSpeed = 100.0f;

	private float zoomDampening = 10.0f;

	//==============================================================
	// Start
	//==============================================================
	void Start ()
	{
		rotateSpeed = 4000f;
		zTiltMin = 0;
		zTiltMax = 40;

		// Get game components
		Player = GameObject.Find ("Player").GetComponent<Transform> ();

		//Disc = transform; // This transform. Used in Tilt disc
		Disc = GameObject.Find ("Disc").GetComponent<Transform> (); // This transform. Used in Tilt disc
		discInitialPosition = Disc.transform.localPosition; // Get disc localposition related to Player
		discInitialRotation = Disc.transform.localRotation; // Get disc localrotation

		Basket = GameObject.Find ("Frisbee-Basket").GetComponent<Transform> ();

		rigidBody = GetComponent<Rigidbody> (); // Get Disc Rigidbody

		ResetDisc ();
	}

	//==========================================================================================================================================================================================
	//==========================================================================================================================================================================================
	// Update
	//==========================================================================================================================================================================================
	//==========================================================================================================================================================================================

	void Update ()
	{
			//==============================================================
			// Get physics. Add force to the rigidbody and throw the disc
			//==============================================================
			if (!isThrown && Input.GetMouseButtonUp (LEFTCLICK))
				ThrowDisc ();

			//==============================================================
			// The disc is Thrown
			//==============================================================
			if (isThrown)
			{ 
				CheckLanding ();
			}

			//==============================================================
			// Hold speed at maximum maxSpeed
			//==============================================================
			if (rigidBody.velocity.magnitude > maxSpeed)
				rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
	}

	//==============================================================
	// FixedUpdate. Add curve and rotate
	//==============================================================
	void FixedUpdate ()
	{
			if (isThrown && !isGrounded) 
			{
				// Curve force added each frame
				Vector3 sideDir = Vector3.Cross (Disc.up, rigidBody.velocity).normalized;
				rigidBody.AddForce (sideDir * curveAmount);
			}

			if (isRotate && !isGrounded)
			{
				Rotate (); // Rotate the disc
			}
	}

	//==============================================================
	// LateUpdate Tilt and pan disc
	//==============================================================
	void LateUpdate ()
	{
			if (!isThrown && Input.GetMouseButton (RIGHTCLICK)) 
			{ // Allow tilting the disc before released
				tiltUpDown ();
				tiltLeftRight ();
			}
			if (!isThrown && !Input.GetMouseButton (RIGHTCLICK))
				PanLeftRight ();
				//Pan ();
	}
	//==========================================================================================================================================================================================
	//==========================================================================================================================================================================================

	//==============================================================
	// Throw the disc
	//==============================================================
	void ThrowDisc()
	{
		//==============================================================
		// Thrust in percent
		//==============================================================
		playerThrust = 0.3f; // 30%

		//==============================================================
		// Physics
		//==============================================================

		// Force side (Wind)
		force.x = Randomize (50, 60) * playerThrust;

		// Curve Amount
		curveAmount = Randomize (-1.5f, -1.2f) * playerThrust - (zTilt/24); // Randomize and Increase curveAmount

		// Force up (xTilt)
		if (xTilt < 0) force.y = Randomize (10, 20) + (playerThrust * 100); // Force up (100 - 110)
		// Force up (playerThrust)
		else if (playerThrust > 0 && playerThrust < 0.40f) force.y = Randomize (0, 10) + (playerThrust * 100); 
		else if (playerThrust >= 0.40f && playerThrust < 0.50f) force.y = Randomize (10, 20) + (playerThrust * 100);
		else if (playerThrust >= 0.50f && playerThrust < 0.60f) force.y = Randomize (20, 30) + (playerThrust * 100);
		else if (playerThrust >= 0.60f && playerThrust < 0.70f) force.y = Randomize (30, 40) + (playerThrust * 100);
		else if (playerThrust >= 0.70f && playerThrust < 0.80f) force.y = Randomize (40, 50) + (playerThrust * 100);
		else if (playerThrust >= 0.80f && playerThrust < 0.90f) force.y = Randomize (50, 60) + (playerThrust * 100);
		// Force up (160 - 170)
		else force.y = Randomize (70, 90) + (playerThrust * 100);

		// Force forward (xTilt)
		if (xTilt < 0)
			force.z = Randomize ((980 + (xTilt*5)), (1000 + (xTilt*5))) * playerThrust; // (xTilt 0 to -40) Randomize and decrease Force forward when tilt up
		// Force forward (playerThrust)
		else
			force.z = Randomize (980, 1000) * playerThrust;
		
		// Speed
		maxSpeed = Randomize (maxSpeed-1.0f, maxSpeed+1.0f); // Randomize Max Speed

		Physics.gravity = new Vector3 (0.0f, Gravity, 0.0f); // Let the disc be lighter..
		rigidBody.isKinematic = false; // Add gravity to the disc

		// Add force
		rigidBody.AddForce(Disc.right * force.x); // Add force on X to the disc
		rigidBody.AddForce(Disc.up * force.y); // Add force on Y to the disc
		rigidBody.AddForce(Disc.forward * force.z); // Add force on Z to the disc

		//==============================================================
		// Set bools
		//==============================================================
		isThrown = true; // Disc is thrown by player
		isRotate = true; // The disc will rotate
	}

	//==============================================================
	// Check Landing
	//==============================================================
	void CheckLanding()
	{
		if(rigidBody.IsSleeping() && isGrounded && isThrown)	
		{
			ResetDisc (); // Default values
			NextThrow ();
		}
	}

	//==============================================================
	// Coroutine Next throw
	//==============================================================
	void NextThrow ()
	{
		rigidBody.isKinematic = true; // Remove gravity to the disc

		Player.transform.position = new Vector3 (Disc.position.x, Disc.position.y + 1.2f, Disc.position.z); // Player position same as Disc landing position

		// Make Player "LookAt" Basket only on y axis
		Player.transform.LookAt(Basket.transform.position);
		Vector3 eulerAngles = Player.transform.rotation.eulerAngles;
		eulerAngles.x = 0;
		eulerAngles.z = 0;

		// Set the altered rotation back
		Player.transform.rotation = Quaternion.Euler(eulerAngles);

		Disc.transform.localPosition = discInitialPosition; // Reset disc position related to player
		Disc.transform.localRotation = discInitialRotation; // Reset disc rotation related to player
	}

	//==============================================================
	// Rotate the disc
	//==============================================================
	void Rotate()
	{
		rigidBody.transform.Rotate (Vector3.up, (rotateSpeed * playerThrust)* Time.deltaTime);
	}

	//==============================================================
	// Reset the disc
	//==============================================================
	void ResetDisc()
	{
		isThrown = false; // Next throw
		isGrounded = false; // Not on ground
		isRotate = false; // The disc will not rotate

		// Stop "AddForce". let the disc fall down
		rigidBody.velocity = Vector3.zero; // Stop velocity on the disc
		rigidBody.angularVelocity = Vector3.zero; // Stop rotation of the disc

		// Reset some Physics. 
		curveAmount = 0.0f;
		force = Vector3.zero;
		zTilt = 0.0f;
		xTilt = 0.0f;
	}

	//==============================================================
	// Hit something
	//==============================================================
	void OnCollisionEnter (Collision collision)
	{
		//==============================================================
		// Hit Ground
		//==============================================================
		if (collision.gameObject.name.Contains ("Terrain"))
		{
			rigidBody.velocity = rigidBody.velocity / 1.4f; // Slow down the disc
			rigidBody.angularVelocity = Vector3.zero; // Stop rotation of the disc
			isGrounded = true;
		}

		Physics.gravity = new Vector3 (0.0f, -9.81f, 0.0f);	// Reset Gravity. Nicer look when disc hits things and fall down
	}

	//==============================================================
	// Tilt Frisbee Z and Y
	//==============================================================
	private void tiltUpDown ()
	{
		xTilt += Input.GetAxis ("Mouse Y") * tiltSpeed * 0.02f;
		xTilt = ClampAngle (xTilt, xTiltMin, xTiltMax);
		tiltDesiredRotation = Quaternion.Euler (xTilt, 0, 0);
		tiltCurrentRotation = Disc.localRotation;
		Disc.localRotation = Quaternion.Lerp (tiltCurrentRotation, tiltDesiredRotation, Time.deltaTime * zoomDampening);
	}

	private void tiltLeftRight ()
	{
		zTilt -= Input.GetAxis ("Mouse X") * tiltSpeed * 0.02f;
		zTilt = ClampAngle (zTilt, zTiltMin, zTiltMax);
		tiltDesiredRotation = Quaternion.Euler (0, 0, zTilt);
		tiltCurrentRotation = Disc.localRotation;
		Disc.localRotation = Quaternion.Lerp (tiltCurrentRotation, tiltDesiredRotation, Time.deltaTime * zoomDampening);
	}

	//==============================================================
	// Pan Camera (Player)
	//==============================================================
	private void PanLeftRight ()
	{
		//yPan += Input.GetAxis ("Mouse X") * panSpeed * 0.02f;
		//yPan = ClampAngle (yPan, maxPanLeft, maxPanRight);
		//panDesiredRotation = Quaternion.Euler (0, yPan, 0);
		//panCurrentRotation = Player.localRotation;
		//Player.localRotation = Quaternion.Lerp (panCurrentRotation, panDesiredRotation, Time.deltaTime * zoomDampening);

		yPan += Input.GetAxis ("Mouse X") * panSpeed * 0.02f;
		yPan = ClampAngle (yPan, maxPanLeft, maxPanRight);
		panDesiredRotation = Quaternion.Euler (0, yPan, 0);
		panCurrentRotation = Player.rotation;
		Player.rotation = Quaternion.Lerp (panCurrentRotation, panDesiredRotation, Time.deltaTime * zoomDampening);
	}

	private void Pan ()
	{
		yPan = Input.GetAxis ("Mouse X") * panSpeed * 0.02f;
		Player.transform.Rotate(0, yPan, 0, Space.Self);
	}

	private float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360f)
			angle += 360f;
		if (angle > 360f)
			angle -= 360f;
		return Mathf.Clamp (angle, min, max);
	}

	//==============================================================
	// Get random float number between min - max
	//==============================================================
	private float Randomize (float min, float max)
	{
		float random;
		random = Random.Range(min, max);
		return random;
	}
}