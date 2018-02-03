using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Improbable;

namespace Assets.Gamelogic.Core {
	
	public class SelectionManager : MonoBehaviour {

		public RectTransform dragSelector;
		public RectTransform circleSelector;
		public GameObject tagPrefab;
		public GameObject canvas;
		public double downDelay = 0.2;
		public double upDelay = 0.1;
		public float dragTriggerDistance = 1f;

		[HideInInspector]
		public static SelectionManager instance;


		private bool isChatting = false;
		private bool hasTriggered = false;
		private Vector3 startPos;
		private Vector3 downPos;
		private bool boxSelecting = false;
		private bool radiusSelecting = false;
		private bool wasControllingMouseDown = false;

		public List<CharacterVisualizer> selected;
		private List<EntityId> remoteSelected;
		private List<CharacterVisualizer> currentDragSelection;

		private Hoverable currentHover = null;

		void OnEnable() {
			instance = this;
			selected = new List<CharacterVisualizer> ();
			remoteSelected = new List<EntityId> ();
			currentDragSelection = new List<CharacterVisualizer> ();
		}

		void Update() {

			if (!isChatting) {
				if (EventSystem.current.IsPointerOverGameObject () && !boxSelecting) {
					wasControllingMouseDown = false;
					if (currentHover != null) {
						currentHover.SetHovered (Hoverable.HoverState.None);
						currentHover = null;
					}
					return;
				}

				if (BuildingManager.isBuilding) {
					wasControllingMouseDown = false;
					return;
				}

				RaycastHit hit = GetHit ();
				if (hit.collider != null && hit.collider.GetComponentInParent<Hoverable> () != null) {
					if (hit.collider.GetComponentInParent<Hoverable> () != currentHover) {
						if (currentHover != null && !selected.Contains (currentHover.GetComponent<CharacterVisualizer> ()))
							currentHover.SetHovered (Hoverable.HoverState.None);
						else if (currentHover != null && selected.Contains (currentHover.GetComponent<CharacterVisualizer> ()))
							currentHover.SetHovered (Hoverable.HoverState.Selected);
						CharacterVisualizer c = hit.collider.GetComponent<CharacterVisualizer> ();
						ConstructionVisualizer cons = hit.collider.GetComponent<ConstructionVisualizer> ();

						if (c != null) {
							if (c.CanControl ()) {
								hit.collider.GetComponentInParent<Hoverable> ().SetHovered (Hoverable.HoverState.Hovered);
								currentHover = hit.collider.GetComponent<Hoverable> ();
							}
						} else if (cons != null) {
							if (cons.CanControl ()) {
								hit.collider.GetComponentInParent<Hoverable> ().SetHovered (Hoverable.HoverState.Hovered);
								currentHover = hit.collider.GetComponent<Hoverable> ();
							}

						} else {

							hit.collider.GetComponentInParent<Hoverable> ().SetHovered (Hoverable.HoverState.Hovered);
							currentHover = hit.collider.GetComponentInParent<Hoverable> ();

						}
					}
				} else if (currentHover != null && !selected.Contains (currentHover.GetComponent<CharacterVisualizer> ())) {
					currentHover.SetHovered (Hoverable.HoverState.None);
					currentHover = null;
				} else if (currentHover != null && selected.Contains (currentHover.GetComponent<CharacterVisualizer> ())) {
					currentHover.SetHovered (Hoverable.HoverState.Selected);
					currentHover = null;
				}

				if (boxSelecting)
					UpdateBoxSelect ();
				if (radiusSelecting)
					UpdateRadiusSelect ();

				if (Input.GetKeyDown (KeyCode.Escape)) {
					ClearSelected ();
				}

				if (Input.GetKeyDown (KeyCode.I)) {
					SelectIdle ();
				}

				if (Input.GetMouseButtonDown (0)) {
					wasControllingMouseDown = true;
					downPos = Input.mousePosition;
				}

				if (Input.GetMouseButton (0)) {
					// If left button down for certain time /// Drag
					if (!hasTriggered && Vector3.Distance (downPos, Input.mousePosition) > dragTriggerDistance && wasControllingMouseDown) {
						StartBoxSelect ();
						hasTriggered = true;
					}
				}

				if (Input.GetMouseButtonUp (0)) {
					LeftClick();

					hasTriggered = false;
					if (boxSelecting)
						StopBoxSelect ();
				}

				// If right button down
				if (Input.GetMouseButtonDown (1)) {
					downPos = Input.mousePosition;
				}

				if (Input.GetMouseButtonUp (1))
					RightClick();

				if (Input.GetKeyUp (KeyCode.Return) && !UIManager.instance.chat.isActive) {
					isChatting = true;
					UIManager.instance.chat.setInputEnabled (true);
				}
			} else {
				if (Input.GetKeyUp (KeyCode.Return) && UIManager.instance.chat.isActive) {
					isChatting = false;
					UIManager.instance.chat.onInputSubmitted ();
				}
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

		void RightClickOld() {
			RaycastHit hit = GetHit();
			if (hit.collider != null)
				UIManager.OpenPreview (hit.collider.gameObject);
		}

		void LeftClickOld() {
			RaycastHit hit = GetHit ();
			if (
				Input.GetKey (KeyCode.LeftShift) || 
				NothingSelected() || 
				(hit.collider != null && hit.collider.GetComponent<CharacterVisualizer>() != null  && hit.collider.GetComponent<CharacterVisualizer>().CanControl())
			) {
				// If pos hits a character, select them

				if (hit.collider != null) {
					CharacterVisualizer s = hit.transform.GetComponent<CharacterVisualizer> ();
					if (s != null) {
						if (Input.GetKey (KeyCode.LeftShift)) {
							AddSelected (s);
						} else {
							SetSelected (s);
						}
					} else if (NothingSelected()) {
						WorkSiteVisualizer ws = hit.transform.GetComponent<WorkSiteVisualizer> ();
						if (ws != null)
							SelectWorkers (ws);
					}

				} else {
					// deselect
					ClearSelected ();
				}
			} else {
				CommandCenter.InterpretClickCommand (selected, remoteSelected, hit);
			}
		}

		void RightClick() {
			
			RaycastHit hit = GetHit ();
			if (hit.collider == null)
				return;
			
			// if anyone is selected, interpret the command
			// else try to open a preview
			if (!NothingSelected())
				CommandCenter.InterpretClickCommand (selected, remoteSelected, hit);
			else if (hit.collider.GetComponentInParent<WorkSiteVisualizer>() != null || hit.collider.GetComponentInParent<ConstructionVisualizer>() != null)
				UIManager.OpenPreview (hit.collider.gameObject);

		}

		void LeftClick() {
			
			RaycastHit hit = GetHit ();
			if (hit.collider == null) {
				ClearSelected ();
				return;
			}
			
			// if it's a character you can control - select (either set or add based on lshift
			CharacterVisualizer c = hit.collider.GetComponentInParent<CharacterVisualizer> ();
			if (c != null && c.CanControl ()) {
				if (Input.GetKey (KeyCode.LeftShift)) {
					AddSelected (c);
				} else {
					SetSelected (c);
				}
				return;
			}

			// if it's a ws select the workers
			WorkSiteVisualizer ws = hit.transform.GetComponentInParent<WorkSiteVisualizer> ();
			if (ws != null) {
				SelectWorkers (ws);
				return;
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

			CommandCenter.InterpretRadialCommand (selected, remoteSelected, pt1, Vector3.Distance (pt1, pt2));
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
			float scaleFactor = GetComponent<Canvas> ().scaleFactor;


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
			dragSelector.sizeDelta = new Vector2 (width/scaleFactor, height/scaleFactor);

			foreach (CharacterVisualizer s in currentDragSelection) {
				s.GetComponentInParent<Hoverable> ().SetHovered (Hoverable.HoverState.None);
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
				CharacterVisualizer s = c.collider.GetComponentInParent<CharacterVisualizer> ();
				if (s && s.CanControl()) {
					s.GetComponentInParent<Hoverable> ().SetHovered (Hoverable.HoverState.None);
					currentDragSelection.Add (s);
				}

				ConstructionVisualizer cons = c.collider.GetComponentInParent<ConstructionVisualizer> ();
				if (cons && cons.CanControl()) {
					cons.GetComponentInParent<Hoverable> ().SetHovered (Hoverable.HoverState.None);
				}

				//ifinitely adding stuff here for hoverables???

			}
		}

		private void StopBoxSelect() {
			boxSelecting = false;
			dragSelector.sizeDelta = Vector2.zero;
			dragSelector.gameObject.SetActive (false);

			foreach (CharacterVisualizer s in currentDragSelection) {
				if (!selected.Contains (s))
					AddSelected (s);
			}
			currentDragSelection.Clear ();
		}


		public void SetSelected(List<EntityId> selection) {
			ClearSelected ();
			CharacterVisualizer[] characters = FindObjectsOfType<CharacterVisualizer> ();
			foreach (CharacterVisualizer c in characters) {
				if (selection.Contains (c.gameObject.EntityId ())) {
					AddSelected (c.GetComponent<CharacterVisualizer> ());
					selection.Remove (c.gameObject.EntityId ());
				}
			}
			foreach (EntityId i in selection) {
				remoteSelected.Add (i);
			}
		}

		public void SetSelected(CharacterVisualizer s) {
			ClearSelected ();
			if (s.CanControl ()) {
				s.GetComponentInParent<Hoverable>().SetHovered (Hoverable.HoverState.Selected);
				selected.Add (s);
			}
		}

		public void AddSelected(CharacterVisualizer s) {
			if (s.CanControl ()) {
				s.GetComponentInParent<Hoverable>().SetHovered (Hoverable.HoverState.Selected);
				selected.Add (s);
			}
		}

		public void RemoveSelected(CharacterVisualizer s) {
			s.GetComponentInParent<Hoverable>().SetHovered (Hoverable.HoverState.None);
			selected.Remove (s);
		}

		public void ClearSelected() {
			foreach (CharacterVisualizer s in selected) {
				if (s != null)
					s.GetComponentInParent<Hoverable>().SetHovered (Hoverable.HoverState.None);
				else
					selected.Remove (s);
			}
			selected.Clear ();
			remoteSelected.Clear ();
		}

		public bool IsSelected(CharacterVisualizer s) {
			return selected.Contains (s);
		}

		public bool NothingSelected() {
			return remoteSelected.Count == 0 && selected.Count == 0;
		}

		public Vector3 GetMedianSelectionPosition() {
			if (NothingSelected())
				return Vector3.zero;

			float x = 0;
			float y = 0;

			foreach (CharacterVisualizer s in selected) {
				if (s != null) {
					x += s.transform.position.x;
					y += s.transform.position.y;
				} 
			}

			x /= selected.Count;
			y /= selected.Count;

			return new Vector3 (x, y);
		}

		private void SelectIdle() {
			DistrictVisualizer v = FindObjectOfType<DistrictVisualizer>();
			if (v != null)
				SetSelected (v.GetIdleCharacters ());
		}

		private void SelectWorkers(WorkSiteVisualizer workSite) {
			SetSelected (workSite.GetWorkers());
		}
			
		public void OnCharacterEnabled(GameObject o) {
			if (remoteSelected.Contains (o.EntityId ())) {
				remoteSelected.Remove (o.EntityId ());
				AddSelected (o.GetComponent<CharacterVisualizer>());
			}
		}

		public void OnCharacterDisabled(GameObject o) {
			if (selected.Contains (o.GetComponent<CharacterVisualizer>())) {
				RemoveSelected (o.GetComponent<CharacterVisualizer> ());
				remoteSelected.Add (o.EntityId ());
			}
		}

		public bool IsChatting() {
			return isChatting;
		}

	}

}