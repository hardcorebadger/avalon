using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class SteeringDemo : MonoBehaviour {

	public GameObject target;
	public float speed = 5f;
	public float range = 5f;
	public float maxRotation = 60f;
	public float interpolation = 1f;
	private Rigidbody2D rigidbody;

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody2D> ();
	}

	void Update() {
		Vector3 dir = (target.transform.position - transform.position);
		float dist = dir.magnitude;
		Steer (ref dir);

		if (dist > 5)
			rigidbody.velocity = transform.TransformDirection(new Vector2 (0, speed));
		else if (dist > 3)
			rigidbody.velocity = transform.TransformDirection(new Vector2 (0, speed/2));
		else
			rigidbody.velocity = transform.TransformDirection(new Vector2 (0, 0));
		
		transform.Rotate(new Vector3(0,0,GetScaledRotation(GetRotationTo (dir))));
	}
	
	// Update is called once per frame
	void Steer (ref Vector3 dir) {
		dir.Normalize ();
		Debug.DrawRay (transform.position, dir*range, Color.red);
		if (!CanWalk (dir, range))
			Avoid (ref dir, range);
		Debug.DrawRay (transform.position, dir*range, Color.blue);
	}

	protected bool CanWalk(Vector3 dir, float castDist) {
		RaycastHit2D[] hits = Physics2D.CircleCastAll (transform.position, 1f, dir, range);
		bool flag = true;
		foreach (RaycastHit2D hit in hits) {
			if (hit.collider.gameObject == gameObject || hit.collider.gameObject == target)
				continue;
			if (!hit.collider.isTrigger)
				flag = false;
		}
		return flag;
	}

	protected void Avoid (ref Vector3 dir, float dist) {
		
		Vector3 dirP = dir;
		Vector3 dirN = dir;
		float tests = 0;
		while (!CanWalk(dirP.normalized, dist)) {
			Rotate (ref dirP, interpolation);
			tests += interpolation;
			if (tests > 360f)
				break;
		}
		tests = 0;
		while (!CanWalk(dirN.normalized, dist)) {
			Rotate (ref dirN, -1*interpolation);
			tests += interpolation;
			if (tests > 360f)
				break;
		}
		if (Vector3.Distance (dirP, transform.up) < Vector3.Distance (dirN, transform.up))
			dir = dirP;
		else
			dir = dirN;
	}

	protected void Rotate(ref Vector3 v, float theta) {
		theta *= Mathf.Deg2Rad;
		float cos = Mathf.Cos (theta);
		float sin = Mathf.Sin (theta);
		v = transform.InverseTransformDirection (v);
		v = new Vector3(v.x*cos - v.y*sin,v.x*sin + v.y*cos, 0f);
		v = transform.TransformDirection (v);
	}

	protected float GetRotationTo(Vector3 dif) {
		float deg = (float)(Mathf.Acos (Vector3.Dot (transform.up, dif)) * 180 / 3.14);
		Vector3 cross = Vector3.Cross (transform.up, dif).normalized;

		if (Single.IsNaN (deg))
			return 0f;
		if (cross.z < 0)
			return (float)deg * -1f;
		else
			return (float)deg;
	}

	protected float GetScaledRotation(float rotation) {
		if (Mathf.Abs(rotation) < maxRotation*Time.deltaTime)
			return rotation;
		if (rotation < -maxRotation*Time.deltaTime)
			return -maxRotation*Time.deltaTime;
		return maxRotation*Time.deltaTime;
	}
}
