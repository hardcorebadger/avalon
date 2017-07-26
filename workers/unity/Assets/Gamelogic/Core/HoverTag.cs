using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverTag : MonoBehaviour {

	private Text text;

	// Use this for initialization
	void OnEnable () {
		text = GetComponentInChildren<Text> ();
	}
	
	public void SetText(string s) {
		text.text = s;
	}
}
