using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public GameObject playerCamera;
	private GameObject disc;

	public float discDistance = 2f;
	public float discThrowingForce = 5f;

	private bool holdingDisc = false;

	// Use this for initialization
	void Start () {
		disc = GameObject.Find("Disc");
	}

	void OnTriggerEnter (Collider col)
	{
		Debug.Log ("COLLISSIOSNSIONSION");
		if (col.gameObject.CompareTag ("Disc"))
		{
			disc.GetComponent<Rigidbody>().useGravity = false;
			holdingDisc = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (holdingDisc)
		{
			disc.transform.position = playerCamera.transform.position + playerCamera.transform.forward * discDistance;
		}

		if (Input.GetMouseButtonDown(0))
		{
			holdingDisc = false;
			disc.GetComponent<Rigidbody> ().useGravity = true;
			disc.GetComponent<Rigidbody> ().AddForce (playerCamera.transform.forward * discThrowingForce);
		}
	}
}
