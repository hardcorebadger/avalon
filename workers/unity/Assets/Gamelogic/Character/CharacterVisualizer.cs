using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class CharacterVisualizer : MonoBehaviour {

		private Vector3 target;
		public float movementSpeed = 0.5f;

		void Update () {
			if (Vector3.Distance (target, transform.position) < 0.1)
				transform.position = target;
			else {
				Vector3 vel = (target - transform.position).normalized * movementSpeed;
				transform.position += vel;
			}
		}

		// Use this for initialization
		void Start () {
			StartCoroutine("Move");
		}
		
		// Update is called once per frame
		IEnumerator Move () {
			while (this.isActiveAndEnabled) {
				yield return new WaitForSeconds (2);

				SetPosition (transform.position.x, transform.position.y+1);
			}
		}

		void SetPosition(float x, float y) {
			target = new Vector3 (x, y, 0);
		}
	}

}