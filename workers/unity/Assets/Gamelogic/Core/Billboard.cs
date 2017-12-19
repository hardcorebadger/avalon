using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log ("yo");
		transform.eulerAngles = new Vector3 (40, 45, 0);
	}

}
