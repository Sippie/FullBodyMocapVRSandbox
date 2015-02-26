using UnityEngine;
using System.Collections;

public class ReturnToOrigin : MonoBehaviour {
	public float ReturnAfterThisDistance = 10;

	private Vector3 origPos;

	// Use this for initialization
	void Start () {
		origPos = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float dist = Vector3.Distance(transform.position, origPos);

		if (dist > ReturnAfterThisDistance) {
			transform.position = origPos;
			transform.rigidbody.velocity = Vector3.zero;
			transform.rigidbody.angularVelocity = Vector3.zero;
		}
	}
}
