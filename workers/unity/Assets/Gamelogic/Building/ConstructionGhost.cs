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

			if (requiresDistrict) {
				
				Collider[] c = Physics.OverlapBox (transform.position + new Vector3((xWidth-1)*4,0f,(zWidth-1)*4), new Vector3 (xWidth*4,0.125f,zWidth*4));

				int tileCount = 0;

				foreach (Collider col in c) {
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
						BuildingManager.Construct (transform.position, currentTile.GetComponentInParent<ConstructionTile>().district);
					else
						BuildingManager.Construct (transform.position);
				}
				if (!multiPlacementEnabled) {
					Destroy (gameObject);
					BuildingManager.StopBuilding ();
				}
			}

			if (Input.GetKeyDown (KeyCode.Escape)) {
				Destroy (gameObject);
				BuildingManager.StopBuilding ();
			}
		}

		private RaycastHit GetHit() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast (ray, out hit);
			return hit;
		}

		private Vector3 GetNearestBlock(Vector3 p) {

			p.x -= (xWidth-1) * 4f;
			p.x -= (zWidth-1) * 4f;
				
			int xmod = (int)p.x % 8;
			int zmod = (int)p.z % 8;

			if (xmod <= 4)
				p.x = (int)p.x - xmod;
			else
				p.x = (int)p.x + (8-xmod);


			if (zmod <= 4)
				p.z = (int)p.z - zmod;
			else
				p.z = (int)p.z + (8-zmod);


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