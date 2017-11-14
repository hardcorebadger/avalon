using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChatText : MonoBehaviour {

	public Text text;

	public float timer = 0f;
	public float alphaTimer = 0f;
	public float alpha = 1f;

	// Use this for initialization
	void Start () {
		text = GetComponent<Text> ();

	}

	// Update is called once per frame
	void Update () {

		if (timer > 10f) { 
			DestroyImmediate (this.gameObject);
		} else {
			
			alphaTimer += Time.deltaTime;
			timer += Time.deltaTime;

			if (alphaTimer > 0.13f) {
				alpha -= 0.01f;
				text.color = new Color (0, 0, 0, alpha);
				alphaTimer = 0f;
			}
		}
	}
}
