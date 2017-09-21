using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ConstructionGhost : MonoBehaviour {

		public Material yesMat;
		public Material noMat;

		private SpriteRenderer sprite;

		private int xWidth, zWidth;

		// Use this for initialization
		void Start () {
			sprite = GetComponent<SpriteRenderer> ();
		}

		public void SetSize(int x, int z) {
			xWidth = x;
			zWidth = z;
			transform.localScale = new Vector3(x,z,1);
		}

		void Update() {
			RaycastHit h = GetHit ();
			transform.position = GetNearestBlock(h.point);

			// overlaps box 

			if (Input.GetMouseButtonDown (0)) {
				// consider overlap
				if (true) {
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

			if (xWidth % 2 == 0)
				p.x += 4;
			if (zWidth % 2 == 0)
				p.z += 4;
			
			p.y = 0;

			return p;
		}
	}

}