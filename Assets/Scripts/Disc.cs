using UnityEngine;
using System.Collections;

public class Disc : MonoBehaviour {

	// The fly speed (used by the weapon later)
	public float speed = 2000.0f;


	void OnCollisionEnter(Collision collision)
	{
		Destroy(gameObject);
	}
}