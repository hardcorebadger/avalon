using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;

public class PlayerController : MonoBehaviour {

	public float speed = 0.1f;

	[Require] private Player.Writer playerWriter;

	// Use this for initialization
	void Start () {
		Camera.main.transform.SetParent (transform);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += new Vector3 (Input.GetAxis ("Horizontal")*speed, Input.GetAxis ("Vertical")*speed, 0);
	}
}
