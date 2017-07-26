using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Selectable : MonoBehaviour {

	public string typeName;
	public Color tagColor;
	public GameObject tagPrefab;

	private cakeslice.Outline outline;
	private GameObject tagObject;

	// Use this for initialization
	void Start () {
		outline = GetComponent<cakeslice.Outline>();
		outline.color = 2;
	}
	
	// Update is called once per frame
	void OnMouseEnter () {
		SetHighlighted (true);
		tagObject = Instantiate (tagPrefab,FindObjectOfType<Canvas>().transform);
		tagObject.GetComponent<Image> ().color = tagColor;
		tagObject.GetComponent<HoverTag> ().SetText (typeName);
		tagObject.GetComponent<RectTransform> ().position = Camera.main.WorldToScreenPoint (transform.position);
	}

	void OnMouseExit () {
		if (!SelectionManager.IsSelected (this)) {
			SetHighlighted (false);
		}
		Destroy (tagObject);
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
