using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;

public class PlayerController : MonoBehaviour {

	public float speed = 0.1f;

	[HideInInspector]
	[Require] public Player.Writer playerWriter;

	public static PlayerController instance;

	// Use this for initialization
	void Start () {
		Camera.main.transform.SetParent (transform);
		instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += new Vector3 (Input.GetAxis ("Horizontal")*speed, Input.GetAxis ("Vertical")*speed, 0);
	}
}
