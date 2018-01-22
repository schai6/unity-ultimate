using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour {

	private Rigidbody discRigidBody;

	public GameObject playerCamera;
	private GameObject disc;

	public float discDistance = 2f;
	public float discThrowingForce = 5f;

	private bool holdingDisc = false;

	//==============================================================
	// Mousebuttons
	//==============================================================
	const int LEFTCLICK = 0;
	const int RIGHTCLICK = 1;

	//==============================================================
	// Game Transforms
	//==============================================================
	private Transform Disc; // The Frisbee

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

	// Use this for initialization
	void Start () {
		disc = GameObject.Find("Disc");
		rotateSpeed = 4000f;
		zTiltMin = 0;
		zTiltMax = 40;
		discInitialPosition = disc.transform.localPosition; // Get disc localposition related to Player
		discInitialRotation = disc.transform.localRotation; // Get disc localrotation
		Debug.Log(disc);
		discRigidBody = disc.GetComponent<Rigidbody>(); // Get Disc Rigidbody
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject.CompareTag ("Disc"))
		{
			disc.GetComponent<Rigidbody>().isKinematic = true;
			holdingDisc = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (holdingDisc)
		{
			disc.transform.position = playerCamera.transform.position + playerCamera.transform.forward * discDistance;
			if (Input.GetMouseButtonDown(LEFTCLICK))
				ThrowDisc();
		}

		//==============================================================
		// The disc is Thrown
		//==============================================================
//		if (isThrown)
//		{ 
//			CheckLanding ();
//		}
		if (discRigidBody.velocity.magnitude > maxSpeed)
			discRigidBody.velocity = discRigidBody.velocity.normalized * maxSpeed;
	}

	//==============================================================
	// FixedUpdate. Add curve and rotate
	//==============================================================
	void FixedUpdate ()
	{
		if (isThrown && !isGrounded) 
		{
			// Curve force added each frame
			Vector3 sideDir = Vector3.Cross (disc.transform.up, discRigidBody.velocity).normalized;
			discRigidBody.AddForce (sideDir * curveAmount);
		}

		if (isRotate && !isGrounded)
		{
			Rotate (); // Rotate the disc
		}
	}
//	void LateUpdate ()
//	{
//		if (!isThrown && Input.GetMouseButton (RIGHTCLICK)) 
//		{ // Allow tilting the disc before released
//			tiltUpDown ();
//			tiltLeftRight ();
//		}
//	}

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
//		force.x = Randomize (50, 60) * playerThrust;

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
		discRigidBody.isKinematic = false; // Add gravity to the disc

		// Add force
		discRigidBody.AddForce(disc.transform.right * force.x); // Add force on X to the disc
		discRigidBody.AddForce(disc.transform.up * force.y); // Add force on Y to the disc
		discRigidBody.AddForce(disc.transform.forward * force.z); // Add force on Z to the disc

		//==============================================================
		// Set bools
		//==============================================================
		isThrown = true; // Disc is thrown by player
		isRotate = true; // The disc will rotate
		holdingDisc = false;
	}

	//==============================================================
	// Rotate the disc
	//==============================================================
	void Rotate()
	{
		discRigidBody.transform.Rotate (Vector3.up, (rotateSpeed * playerThrust)* Time.deltaTime);
	}

//	void ThrowDisc()
//	{
//		holdingDisc = false;
//		disc.GetComponent<Rigidbody> ().useGravity = true;
//		disc.GetComponent<Rigidbody> ().AddForce (playerCamera.transform.forward * discThrowingForce);
//	}

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
