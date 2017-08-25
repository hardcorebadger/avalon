using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedRotation : MonoBehaviour {

	private Vector3 pos = Vector3.zero;
	private Vector3 parentPos = Vector3.zero;

	// Use this for initialization
	void OnEnable () {
		pos = transform.position;
		parentPos = transform.parent.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.identity;
		transform.position = pos + (transform.parent.position - parentPos);
	}
}
