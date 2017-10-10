using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class promocam : MonoBehaviour {

	public float speed = 5f;
	public float xmult = 1f;
	public float zmult = 1f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float f = Time.deltaTime * speed;
		transform.position += new Vector3 (f*xmult, 0, f*zmult);
	}
}
