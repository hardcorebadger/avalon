using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
		transform.eulerAngles = new Vector3 (30, 45, 0);
	}

}
