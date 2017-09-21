using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Gamelogic.Core {
	
	public class SelectionManager : MonoBehaviour {

		public RectTransform dragSelector;
		public RectTransform circleSelector;
		public GameObject tagPrefab;
		public GameObject canvas;
		public double downDelay = 0.2;
		public double upDelay = 0.1;
		public float dragTriggerDistance = 1f;
		public bool leftMouseInputMode = true;

		[HideInInspector]
		public static SelectionManager instance;


		private bool hasTriggered = false;
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

			if (EventSystem.current.IsPointerOverGameObject () && !boxSelecting)
				return;

			if (BuildingManager.isBuilding)
				return;

			if (boxSelecting)
				UpdateBoxSelect ();
			if (radiusSelecting)
				UpdateRadiusSelect ();

			if (leftMouseInputMode) {
				if (Input.GetKeyDown (KeyCode.Escape)) {
					ClearSelected ();
				}
			}

			if (Input.GetMouseButtonDown (0)) {
				downPos = Input.mousePosition;
			}

			if (Input.GetMouseButton (0)) {
				// If left button down for certain time /// Drag
				if (!hasTriggered && Vector3.Distance(downPos,Input.mousePosition) > dragTriggerDistance) {
					StartBoxSelect ();
					hasTriggered = true;
				}
			}

			if (Input.GetMouseButtonUp (0)) {
				if (leftMouseInputMode)
					LeftClickLMBMode();
				else
					LeftClickRMBMode();

				hasTriggered = false;
				if (boxSelecting)
					StopBoxSelect ();
			}

			// If left button down
			if (Input.GetMouseButtonDown (1)) {
				downPos = Input.mousePosition;
			}

			if (Input.GetMouseButtonUp (1)) {
				if (leftMouseInputMode)
					RightClickLMBMode();
				else
					RightClickRMBMode();
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

		private RaycastHit GetHit() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast (ray, out hit);
			return hit;
		}

		void RightClickRMBMode() {
			RaycastHit hit = GetHit();
			if (selected.Count == 0) {
				// info pop ups
				if (hit.collider != null)
					UIManager.OpenPreview (hit.collider.gameObject);
			} else {
				CommandCenter.InterpretClickCommand (selected, hit);
			}
		}

		void LeftClickRMBMode() {
			// If Not Shift, clear selection
			if (!Input.GetKey (KeyCode.LeftShift)) {
				ClearSelected ();
			}
			// If pos hits a character, select them
			RaycastHit hit = GetHit();
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

		void RightClickLMBMode() {
			RaycastHit hit = GetHit();
			if (hit.collider != null)
				UIManager.OpenPreview (hit.collider.gameObject);
		}

		void LeftClickLMBMode() {
			RaycastHit hit = GetHit ();
			if (
				Input.GetKey (KeyCode.LeftShift) || 
				selected.Count == 0 || 
				(hit.collider != null && hit.collider.GetComponent<CharacterVisualizer>() != null  && hit.collider.GetComponent<CharacterVisualizer>().CanControl())
			) {
				// If pos hits a character, select them

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
					ClearSelected ();
				}
			} else {
				CommandCenter.InterpretClickCommand (selected, hit);
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

			Vector3 pt1 = Camera.main.ScreenToWorldPoint (dragSelector.position);
			Vector3 pt2 = Camera.main.ScreenToWorldPoint (dragSelector.position + new Vector3 (width, height));
			float worldDist = Vector3.Distance (pt1, pt2);
			float screenDist = Vector3.Distance (dragSelector.position, dragSelector.position + new Vector3 (width, height));
			float ratio = worldDist / screenDist;

			Vector3 center = Camera.main.ScreenToWorldPoint (dragSelector.position + new Vector3 (width/2, height/-2));
			Debug.DrawRay (center, Camera.main.transform.forward, Color.red);

			RaycastHit[] hits = Physics.BoxCastAll ( center, new Vector3 ((width/2f)*ratio, (height/2f)*ratio, 1f), Camera.main.transform.forward, Quaternion.LookRotation(Camera.main.transform.forward));

			foreach (RaycastHit c in hits) {
				Selectable s = c.collider.GetComponent<Selectable> ();
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

		public Vector3 GetMedianSelectionPosition() {
			if (selected.Count == 0)
				return Vector3.zero;

			float x = 0;
			float y = 0;

			foreach (Selectable s in selected) {
				x += s.transform.position.x;
				y += s.transform.position.y;
			}

			x /= selected.Count;
			y /= selected.Count;

			return new Vector3 (x, y);
		}

	}

}