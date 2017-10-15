using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class ConstructionTile : MonoBehaviour {

		public bool willDelete = false;

		public EntityId district;

		public void SetDistrict(EntityId i) {
			district = i;
		}

		// Use this for initialization
		void Start () {
			Collider[] c = Physics.OverlapBox (transform.position, new Vector3 (4,0.125f,4));
			bool touchingGround = false;
			int i = 0;
			foreach (Collider col in c) {
				if (col.gameObject.layer == 9)
					OverlapBuidling (col.gameObject);
				if (col.gameObject.layer == 10)
					touchingGround = true;
				if (col.gameObject.layer == 11) {
					if (OverlapTile (col.gameObject))
						i++;
				}
			}
			if (i > 1)
				willDelete = true;
			if (!touchingGround)
				Destroy (gameObject);
		}
		
		// Update is called once per frame
		void Update () {
			if (willDelete)
				Destroy (gameObject);
		}

		bool OverlapTile(GameObject tile) {
			// if they are deleting, its fine
			if (tile.GetComponentInParent<ConstructionTile> ().willDelete) {
				return false;
			}
			
			if ((int)tile.transform.position.x == (int)transform.position.x && (int)tile.transform.position.z == (int)transform.position.z) {
				
				return true;
			}
			return false;
		}

		void OverlapBuidling(GameObject building) {
			// figures out if its just a fringe collision
			Vector3 pos;
			BuildingVisualizer viz = building.GetComponent<BuildingVisualizer> ();
			for (int z = 0; z < viz.zWidth; z++) {
				for (int x = 0; x < viz.xWidth; x++) {
					pos = building.transform.position + new Vector3 (8 * x, 0f, 8 * z);
					// if the building actually covers this block
					if ((int)pos.x == (int)transform.position.x && (int)pos.z == (int)transform.position.z)
						Destroy (gameObject);
				}
			}
		}
	}

}