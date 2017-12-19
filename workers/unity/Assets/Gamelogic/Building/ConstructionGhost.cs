using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {

	public class ConstructionGhost : MonoBehaviour {

		public Material yesMat;
		public Material noMat;

		private int xWidth, zWidth;
		public bool requiresDistrict;

		private EntityId district;
		private bool multiPlacementEnabled;

		// Use this for initialization

		public void SetInfo(BuildingManager.ConstructionInfo info) {
			xWidth = info.xWidth;
			zWidth = info.zWidth;
			requiresDistrict = info.requiresDistrict;
			transform.GetChild(0).localScale = new Vector3(xWidth*8,1f,zWidth*8);
			transform.GetChild(0).localPosition = new Vector3((xWidth-1)*4,0f,(zWidth-1)*4);
			multiPlacementEnabled = info.multiPlacement;
		}

		void Update() {
			RaycastHit h = GetHit ();
			transform.position = GetNearestBlock(h.point);

			GameObject currentTile = null;
			bool canBuild = true;

			Collider[] overlap = Physics.OverlapBox (transform.position + new Vector3((xWidth-1)*4,0f,(zWidth-1)*4), new Vector3 (xWidth*4,0.125f,zWidth*4));

			if (requiresDistrict) {

				int tileCount = 0;

				foreach (Collider col in overlap) {
					if (col.gameObject.layer == 11 && TrueOverlap(col.gameObject)) {
						tileCount++;
						currentTile = col.gameObject;
					}
				}
				if (tileCount >= xWidth * zWidth) {
					canBuild = true;
				} else {
					canBuild = false;
				}
			}

			if (canBuild) {
				transform.GetChild (0).GetComponent<MeshRenderer> ().material = yesMat;
			} else {
				transform.GetChild (0).GetComponent<MeshRenderer> ().material = noMat;
			}

			if (Input.GetMouseButtonDown (0)) {
				// consider overlap

				if (canBuild) {
					if (requiresDistrict)
						BuildingManager.Construct (transform.position, overlap, currentTile.GetComponentInParent<ConstructionTile>().district);
					else
						BuildingManager.Construct (transform.position, overlap);
				}
				if (!multiPlacementEnabled) {
					Destroy (gameObject);
					BuildingManager.StopBuilding ();
				}
			}

			if (Input.GetMouseButtonDown (1)) {
				Destroy (gameObject);
				BuildingManager.StopBuilding ();
			}

			if (Input.GetKeyDown (KeyCode.Escape)) {
				Destroy (gameObject);
				BuildingManager.StopBuilding ();
			}
		}

		private RaycastHit GetHit() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast (ray, out hit, 1000f, LayerMask.GetMask(LayerMask.LayerToName(10)));
			return hit;
		}

		private Vector3 GetNearestBlock(Vector3 p) {

			// shift footprint to center building on mouse
			p.x -= (xWidth-1) * 4f;
			p.z -= (zWidth-1) * 4f;
				
			// get percent across the tile
			int xmod = (int)p.x % 8;
			int zmod = (int)p.z % 8;

			if (xmod >= 0 && xmod <= 4)
				p.x = (int)p.x - xmod;
			else if (xmod > 4)
				p.x = (int)p.x + (8-xmod);
			else if (xmod < 0 && xmod >= -4)
				p.x = (int)p.x - xmod;
			else if (xmod < -4)
				p.x = (int)p.x - (8+xmod);


			if (zmod >= 0 && zmod <= 4)
				p.z = (int)p.z - zmod;
			else if (zmod > 4)
				p.z = (int)p.z + (8-zmod);
			else if (zmod < 0 && zmod >= -4)
				p.z = (int)p.z - zmod;
			else if (zmod < -4)
				p.z = (int)p.z - (8+zmod);


			p.y = 0;

			return p;
		}

		private bool TrueOverlap(GameObject g) {
			Vector3 pos;
			for (int z = 0; z < zWidth; z++) {
				for (int x = 0; x < xWidth; x++) {
					pos = transform.position + new Vector3 (8 * x, 0f, 8 * z);
					// if the building actually covers this block
					if ((int)pos.x == (int)g.transform.position.x && (int)pos.z == (int)g.transform.position.z) {
						return true;
					}
				}
			}
			return false;
		}
	}

}