using UnityEngine;
using System.Collections;

public class Disc : MonoBehaviour {
	private GameObject disc;
	private Rigidbody discRigidBody;

	void Start () {
		disc = GameObject.Find("Disc");
		discRigidBody = disc.GetComponent<Rigidbody>();
	}

	void onTriggerEnter (Collision collision)
	{
		//==============================================================
		// Hit Ground
		//==============================================================
		Debug.Log("LALALAL");
		if (collision.gameObject.name == "Playing Field")
		{
			Debug.Log ("COLLISSSIOSN");
			discRigidBody.velocity = discRigidBody.velocity / 1.4f; // Slow down the disc
			discRigidBody.angularVelocity = Vector3.zero; // Stop rotation of the disc
		}

		Physics.gravity = new Vector3 (0.0f, -9.81f, 0.0f);	// Reset Gravity. Nicer look when disc hits things and fall down
	}
}