using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class Selectable : MonoBehaviour {

	public Color tagColor;
	public GameObject tagPrefab;

	private Outline outline;
	private GameObject tag;

	// Use this for initialization
	void Start () {
		outline = GetComponent<Outline>();
		outline.color = 2;
	}
	
	// Update is called once per frame
	void OnMouseEnter () {
		outline.color = 0;
		tag = Instantiate (tagPrefab,FindObjectOfType<Canvas>().transform);
		tag.GetComponent<HoverTag> ().SetText ("Pine Tree");
		tag.GetComponent<RectTransform> ().position = Camera.main.WorldToScreenPoint (transform.position);
	}

	void OnMouseExit () {
		outline.color = 2;
		Destroy (tag);
	}

}
