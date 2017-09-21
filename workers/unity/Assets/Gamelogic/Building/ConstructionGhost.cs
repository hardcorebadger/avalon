using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ConstructionGhost : MonoBehaviour {

		public Material yesMat;
		public Material noMat;

		private SpriteRenderer sprite;

		// Use this for initialization
		void Start () {
			sprite = GetComponent<SpriteRenderer> ();
		}

		public void SetSize(int x, int z) {
			transform.localScale = new Vector3(x,z,1);
		}

		void Update() {
			RaycastHit h = GetHit ();
			transform.position = new Vector3(h.point.x, 0f, h.point.z);

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

	}

}