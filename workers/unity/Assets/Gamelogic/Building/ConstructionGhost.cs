using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ConstructionGhost : MonoBehaviour {

		public Color yesColor;
		public Color noColor;

		private SpriteRenderer sprite;
		private Collider2D col;

		// Use this for initialization
		void Start () {
			sprite = GetComponent<SpriteRenderer> ();
			col = GetComponent<Collider2D> ();
		}
		
		// Update is called once per frame
		void Update () {
			transform.position = Camera.main.ScreenToWorldPoint (Input.mousePosition + new Vector3 (0, 0, Camera.main.transform.position.z * -1));

			Collider2D[] c = new Collider2D[20];
			ContactFilter2D filter = new ContactFilter2D ();
			filter.useTriggers = false;
			int collisionCount = col.OverlapCollider (filter, c);

			if (Input.GetMouseButtonDown (0)) {
				if (collisionCount == 0) {
					BuildingManager.Construct (transform.position);
				}
				Destroy (gameObject);
			}

			if (collisionCount == 0) {
				sprite.color = yesColor;
			} else {
				sprite.color = noColor;
			}
		}

	}

}