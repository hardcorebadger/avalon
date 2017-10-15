using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ConstructionGhost : MonoBehaviour {

		public Material yesMat;
		public Material noMat;
		public bool workaround = false;

//		private GameObject mesh;

		private int xWidth, zWidth;

		// Use this for initialization
		void Start () {
//			mesh = transform.GetChild
		}

		public void SetSize(int x, int z) {
			xWidth = x;
			zWidth = z;
			transform.GetChild(0).localScale = new Vector3(x*8,1f,z*8);
			transform.GetChild(0).localPosition = new Vector3((x-1)*4,0f,(z-1)*4);
		}

		void Update() {
			RaycastHit h = GetHit ();
			transform.position = GetNearestBlock(h.point);

			// overlaps box 

			Collider[] c = Physics.OverlapBox (transform.position + new Vector3((xWidth-1)*4,0f,(zWidth-1)*4), new Vector3 (xWidth*4,0.125f,zWidth*4));
			bool canBuild = false;
			int tileCount = 0;

			foreach (Collider col in c) {
				if (col.gameObject.layer == 11 && TrueOverlap(col.gameObject)) {
					tileCount++;
				}
			}

			if (tileCount >= xWidth * zWidth) {
				canBuild = true;
				transform.GetChild (0).GetComponent<MeshRenderer> ().material = yesMat;
			} else {
				canBuild = false;
				transform.GetChild (0).GetComponent<MeshRenderer> ().material = noMat;
			}

			if (Input.GetMouseButtonDown (0)) {
				// consider overlap

				if (canBuild || workaround) {
					BuildingManager.Construct (transform.position);
				}
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