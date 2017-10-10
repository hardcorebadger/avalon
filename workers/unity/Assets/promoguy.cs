using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class promoguy : MonoBehaviour {

	public Vector3 dir;
	public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += Time.deltaTime * speed * dir;
	}
}
