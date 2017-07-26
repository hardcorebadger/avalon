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
		SetHighlighted (true);
		tag = Instantiate (tagPrefab,FindObjectOfType<Canvas>().transform);
		tag.GetComponent<HoverTag> ().SetText ("Pine Tree");
		tag.GetComponent<RectTransform> ().position = Camera.main.WorldToScreenPoint (transform.position);
	}

	void OnMouseExit () {
		if (!SelectionManager.IsSelected (this)) {
			SetHighlighted (false);
		}
		Destroy (tag);
	}

	void OnMouseDown() {
		if (Input.GetKey(KeyCode.LeftShift))
			SelectionManager.AddSelected (this);
		else
			SelectionManager.SetSelected (this);
	}

	public void SetHighlighted(bool h) {
		if (h)
			outline.color = 0;
		else
			outline.color = 2;
	}

}
