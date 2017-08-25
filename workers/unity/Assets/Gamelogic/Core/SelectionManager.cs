using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {
	
	public class SelectionManager : MonoBehaviour {

		public RectTransform dragSelector;
		public RectTransform circleSelector;
		public GameObject tagPrefab;
		public GameObject canvas;
		public double downDelay = 0.2;
		public double upDelay = 0.1;

		[HideInInspector]
		public static SelectionManager instance;

		private double downTime = 0;
		private double upTime = 0;
		private bool potentialDouble = false;
		private bool hasTriggered = true;
		private Vector3 startPos;
		private Vector3 downPos;
		private bool boxSelecting = false;
		private bool radiusSelecting = false;

		public List<Selectable> selected;
		private List<Selectable> currentDragSelection;

		void OnEnable() {
			instance = this;
			selected = new List<Selectable> ();
			currentDragSelection = new List<Selectable> ();
		}

		void Update() {

			if (boxSelecting)
				UpdateBoxSelect ();
			if (radiusSelecting)
				UpdateRadiusSelect ();



			// If left button down
			if (Input.GetMouseButtonDown (0)) {
				downPos = Input.mousePosition;
				downTime = Time.time;
				SingleClick();
			}

			if (Input.GetMouseButton (0)) {
				// If left button down for certain time /// Drag
				if (!hasTriggered && Time.time > downTime + downDelay) {
					StartBoxSelect ();
					hasTriggered = true;
				}
			}

			if (Input.GetMouseButtonUp (0)) {
				hasTriggered = false;
				if (boxSelecting)
					StopBoxSelect ();
			}

			// If left button down
			if (Input.GetMouseButtonDown (1)) {
				downPos = Input.mousePosition;
				downTime = Time.time;
				RightClick();
			}

			if (Input.GetMouseButton (1)) {
				// If left button down for certain time /// Drag
				if (!hasTriggered && Time.time > downTime + downDelay) {
					StartRadiusSelect ();
					hasTriggered = true;
				}
			}

			if (Input.GetMouseButtonUp (1)) {
				hasTriggered = false;
				if (radiusSelecting)
					StopRadiusSelect ();
			}


//			
//			if (Input.GetMouseButtonDown (0)) {
//				downPos = Input.mousePosition;
//				hasTriggered = false;
//				downTime = Time.time;
//				if (Time.time - upTime < upDelay) {
//					potentialDouble = true;
//				}
//			}
//
//			if (Input.GetMouseButtonUp (0)) {
//				if (boxSelecting)
//					StopBoxSelect ();
//				if (radiusSelecting)
//					StopRadiusSelect ();
//				
//				if (Time.time - downTime < downDelay) {
//					if (potentialDouble) {
//						TriggerWipe ();
//						DoubleClick ();
//					} else 
//						upTime = Time.time;
//				}
//			}
//
//			if (Input.GetMouseButton (0) && !hasTriggered) {
//				if (Time.time - downTime > downDelay) {
//					TriggerWipe ();
//					Drag ();
//				}
//			} else if (!hasTriggered) {
//				if (Time.time - upTime > upDelay) {
//					TriggerWipe ();
//					SingleClick ();
//				}
//			}
		}

		private void TriggerWipe() {
			hasTriggered = true;
			upTime = 0;
			downTime = 0;
			potentialDouble = false;
		}

		private RaycastHit2D GetHit() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			return Physics2D.GetRayIntersection(ray,Mathf.Infinity);
		}

		void RightClick() {
			if (selected.Count == 0) {
				// info pop ups
			} else {
				CommandCenter.InterpretClickCommand (selected, GetHit (), Camera.main.ScreenToWorldPoint (Input.mousePosition + new Vector3 (0, 0, Camera.main.transform.position.z * -1)));
			}
		}

		void SingleClick() {
			// If Not Shift, clear selection
			if (!Input.GetKey (KeyCode.LeftShift)) {
				ClearSelected ();
			}
			// If pos hits a character, select them
			RaycastHit2D hit = GetHit();
			if (hit.collider != null) {
				Selectable s = hit.transform.GetComponent<Selectable> ();
				if (s != null) {
					if (Input.GetKey (KeyCode.LeftShift)) {
						AddSelected (s);
					} else {
						SetSelected (s);
					}
				}
			} else {
				// deselect
				ClearSelected();
			}
		}

		private void StartRadiusSelect () {
			startPos = downPos;
			circleSelector.position = startPos;
			circleSelector.sizeDelta = Vector2.zero;
			circleSelector.gameObject.SetActive (true);
			radiusSelecting = true;
		}

		private void UpdateRadiusSelect () {
			float dist = Vector3.Distance (Input.mousePosition, startPos);
			circleSelector.sizeDelta = new Vector2(dist*2,dist*2);
		}

		private void StopRadiusSelect () {
			radiusSelecting = false;
			circleSelector.sizeDelta = Vector2.zero;
			circleSelector.gameObject.SetActive (false);

			Vector3 pt1 = Camera.main.ScreenToWorldPoint (startPos + new Vector3 (0, 0, Camera.main.transform.position.z*-1));
			Vector3 pt2 = Camera.main.ScreenToWorldPoint (Input.mousePosition + new Vector3 (0, 0, Camera.main.transform.position.z*-1));

			CommandCenter.InterpretRadialCommand (selected, pt1, Vector3.Distance (pt1, pt2));
		}

		private void StartBoxSelect() {
			if (!Input.GetKey (KeyCode.LeftShift)) {
				ClearSelected ();
			}
			startPos = downPos;
			dragSelector.position = startPos;
			dragSelector.sizeDelta = Vector2.zero;
			dragSelector.gameObject.SetActive (true);
			boxSelecting = true;
		}

		private void UpdateBoxSelect() {
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

			foreach (Selectable s in currentDragSelection) {
				s.SetHighlighted (false);
			}
			currentDragSelection.Clear ();

			Vector3 pt1 = Camera.main.ScreenToWorldPoint (dragSelector.position + new Vector3 (0, 0, Camera.main.transform.position.z*-1));
			Vector3 pt2 = Camera.main.ScreenToWorldPoint (dragSelector.position + new Vector3 (width, -1 * height, Camera.main.transform.position.z*-1));

			Collider2D[] colliders = Physics2D.OverlapAreaAll (pt1, pt2);
			foreach (Collider2D c in colliders) {
				Selectable s = c.GetComponent<Selectable> ();
				if (s && s.IsSelectable()) {
					s.SetHighlighted (true);
					currentDragSelection.Add (s);
				}
			}
		}

		private void StopBoxSelect() {
			boxSelecting = false;
			dragSelector.sizeDelta = Vector2.zero;
			dragSelector.gameObject.SetActive (false);

			foreach (Selectable s in currentDragSelection) {
				if (!selected.Contains (s))
					AddSelected (s);
			}
			currentDragSelection.Clear ();
		}

		public void SetSelected(Selectable s) {
			ClearSelected ();
			if (s.IsSelectable ()) {
				s.SetHighlighted (true);
				selected.Add (s);
			}
		}

		public void AddSelected(Selectable s) {
			if (s.IsSelectable ()) {
				s.SetHighlighted (true);
				selected.Add (s);
			}
		}

		public void RemoveSelected(Selectable s) {
			s.SetHighlighted (false);
			selected.Remove (s);
		}

		public void ClearSelected() {
			foreach (Selectable s in selected) {
				s.SetHighlighted (false);
			}
			selected.Clear ();
		}

		public bool IsSelected(Selectable s) {
			return selected.Contains (s);
		}

	}

}