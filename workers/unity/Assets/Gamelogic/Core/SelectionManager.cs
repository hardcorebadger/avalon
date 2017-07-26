using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour {

	public RectTransform dragSelector;
	public Texture rectTexture;
	private Vector3 startPos;
	private bool mouseDown;
	private List<Selectable> selected;
	private List<Selectable> selecting;
	private static SelectionManager instance;

	// Use this for initialization
	void Start () {
		selected = new List<Selectable> ();
		selecting = new List<Selectable> ();
		instance = this;
	}
	
	// Update is called once per frame
	void OnGUI () {
		if (Input.GetMouseButtonDown (0)) {
			if (!Input.GetKey (KeyCode.LeftShift)) {
				foreach (Selectable s in selected) {
					s.SetHighlighted (false);
				}
				selected.Clear ();
			}
			startPos = Input.mousePosition;
			dragSelector.position = startPos;
			dragSelector.sizeDelta = Vector2.zero;
			dragSelector.gameObject.SetActive (true);
			mouseDown = true;
		}
		if (mouseDown) {
			//draw rect between start and cur pos
			Vector3 cur = Input.mousePosition;
			Vector3 pos = dragSelector.position;

			float width = cur.x - startPos.x;
			if (cur.x < startPos.x) {
				pos.x = cur.x;
				width *= -1;
			} else {
				pos.x = startPos.x;
			}
			float height = cur.y - startPos.y;
			if (cur.y > startPos.y) {
				pos.y = cur.y;
			} else {
				pos.y = startPos.y;
				height *= -1;
			}
			dragSelector.position = pos;
			dragSelector.sizeDelta = new Vector2 (width, height);

			foreach (Selectable s in selecting) {
				s.SetHighlighted (false);
			}
			selecting.Clear ();

			Vector3 pt1 = Camera.main.ScreenToWorldPoint (dragSelector.position + new Vector3 (0, 0, 30));
			Vector3 pt2 = Camera.main.ScreenToWorldPoint (dragSelector.position + new Vector3 (width, -1 * height, 30));

			Collider2D[] colliders = Physics2D.OverlapAreaAll (pt1, pt2);
			foreach (Collider2D c in colliders) {
				Selectable s = c.GetComponent<Selectable> ();
				if (s) {
					s.SetHighlighted (true);
					selecting.Add (s);
				}
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			mouseDown = false;
			dragSelector.sizeDelta = Vector2.zero;
			dragSelector.gameObject.SetActive (false);

			foreach (Selectable s in selecting) {
				if (!selected.Contains (s))
					selected.Add (s);
			}
			selecting.Clear ();
		}
	}

	public static void SetSelected(Selectable s) {
		instance.selected.Clear ();
		s.SetHighlighted (true);
		instance.selected.Add (s);
	}

	public static void AddSelected(Selectable s) {
		s.SetHighlighted (true);
		instance.selected.Add (s);
	}

	public static bool IsSelected(Selectable s) {
		return instance.selected.Contains (s);
	}
}
