using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ConstructionGhost : MonoBehaviour {

		public Color yesColor;
		public Color noColor;
		public bool townOnly = true;
		public bool isTownCenter = false;

		private SpriteRenderer sprite;
		private Collider2D col;
		private GameObject currentTownRadius;

		// Use this for initialization
		void Start () {
			sprite = GetComponent<SpriteRenderer> ();
			col = GetComponent<Collider2D> ();
		}

		void Update() {
			transform.position = Camera.main.ScreenToWorldPoint (Input.mousePosition + new Vector3 (0, 0, Camera.main.transform.position.z * -1));

			int state = 0;

			// Get towns it is in radius of
			Collider2D[] c = new Collider2D[20];
			ContactFilter2D filter = new ContactFilter2D ();
			filter.useLayerMask = true;
			filter.layerMask = LayerMask.GetMask (new string[]{ "TownRadius" });
			filter.useTriggers = true;
			int townRadiusCollisionCount = col.OverlapCollider (filter, c);

			if (townRadiusCollisionCount == 0) {
				ResetCurrentTownRadius ();
			}

			foreach (Collider2D c2d in c) {
				if (c2d == null)
					continue;
				// If any town is not yours
				if (c2d.GetComponent<TownRadiusMarker> ().townCenter.GetComponent<OwnedVisualizer> ().GetOwnerId () != Bootstrap.playerId) {
					// select that town
					ResetCurrentTownRadius ();
					currentTownRadius = c2d.gameObject;
					// set it to red
					c2d.GetComponent<TownRadiusMarker> ().SetMarker (-1);
					// set the building to red
					state = -1;
					// exit
					break;
				}
			}

			if (state == 0) {
				int i = 0;
				// If it requires a town
				if (townOnly) {
					foreach (Collider2D c2d in c) {
						if (c2d == null)
							continue;
						// If any town is yours
						if (c2d.GetComponent<TownRadiusMarker> ().townCenter.GetComponent<OwnedVisualizer> ().GetOwnerId () == Bootstrap.playerId) {
							i++;
							// select that town
							ResetCurrentTownRadius ();
							currentTownRadius = c2d.gameObject;
							// set it to green
							c2d.GetComponent<TownRadiusMarker> ().SetMarker (1);
							break;
						}
					}
					if (i == 0)
						state = -1;
					// Else if it is a town center
				} else if (isTownCenter) {
					foreach (Collider2D c2d in c) {
						if (c2d == null)
							continue;
						// If any town is yours
						if (c2d.GetComponent<TownRadiusMarker> ().townCenter.GetComponent<OwnedVisualizer> ().GetOwnerId () == Bootstrap.playerId) {
							// select that town
							ResetCurrentTownRadius ();
							currentTownRadius = c2d.gameObject;
							// set it to red
							c2d.GetComponent<TownRadiusMarker> ().SetMarker (-1);
							// set the building to red
							state = -1;
							//exit
							break;
						}
					}
				}
				// else, continue

			}

			if (state == 0) {

				c = new Collider2D[20];
				filter = new ContactFilter2D ();
				filter.useLayerMask = true;
				filter.layerMask = LayerMask.GetMask (new string[]{ "Default" });
				filter.useTriggers = false;
				int collisionCount = col.OverlapCollider (filter, c);

				if (collisionCount == 0)
					state = 1;
				else
					state = -1;
			}

			if (state == 1) {
				sprite.color = yesColor;
			} else if (state == -1) {
				sprite.color = noColor;
			}


			if (Input.GetMouseButtonDown (0)) {
				if (state == 1) {
					BuildingManager.Construct (transform.position, currentTownRadius);
				}
				Destroy (gameObject);
				BuildingManager.StopBuilding ();
			}
		}

		private void ResetCurrentTownRadius() {
			if (currentTownRadius != null)
				currentTownRadius.GetComponent<TownRadiusMarker> ().SetMarker (0);
			currentTownRadius = null;
		}
		
		
		// Update is called once per frame
		void UpdateOld () {
			transform.position = Camera.main.ScreenToWorldPoint (Input.mousePosition + new Vector3 (0, 0, Camera.main.transform.position.z * -1));

			// Get towns it is in radius of
			// If any town is not yours
				// select that town
				// set it to red
			    // set the building to red
			    // exit
			// If it requires a town
				// If any town is yours
					// select that town
					// set it to green
			// Else if it is a town center
				// If any town is yours
					// select that town
					// set it to red
					// set the building to red
					// exit
			// else, continue

			// If it is colliding with anything
				// set it to red
			// else
				// set it to green


			Collider2D[] c = new Collider2D[20];
			ContactFilter2D filter = new ContactFilter2D ();
			filter.useLayerMask = true;
			filter.layerMask = LayerMask.GetMask(new string[]{"Default"});
			filter.useTriggers = false;
			int collisionCount = col.OverlapCollider (filter, c);

			int townRadiusCollisionCount = 1;

			if (townOnly) {
				townRadiusCollisionCount = 0;
				c = new Collider2D[20];
				filter = new ContactFilter2D ();
				filter.useLayerMask = true;
				filter.layerMask = LayerMask.GetMask(new string[]{"TownRadius"});
				filter.useTriggers = true;
				townRadiusCollisionCount = col.OverlapCollider (filter, c);
				if (townRadiusCollisionCount > 0) {						
					if (currentTownRadius != null && currentTownRadius != c [0].gameObject)
						currentTownRadius.GetComponent<TownRadiusMarker> ().SetMarker (0);
					currentTownRadius = c [0].gameObject;
					currentTownRadius.GetComponent<TownRadiusMarker> ().SetMarker (1);
				} else {
					if (currentTownRadius != null)
						currentTownRadius.GetComponent<TownRadiusMarker>().SetMarker (0);
					currentTownRadius = null;
				}
			}
				
			if (Input.GetMouseButtonDown (0)) {
				if (collisionCount == 0 && townRadiusCollisionCount > 0) {
					BuildingManager.Construct (transform.position, currentTownRadius);
				}
				Destroy (gameObject);
				BuildingManager.StopBuilding ();
			}

			if (collisionCount == 0 && townRadiusCollisionCount > 0) {
				sprite.color = yesColor;
			} else {
				sprite.color = noColor;
			}
		}

	}

}