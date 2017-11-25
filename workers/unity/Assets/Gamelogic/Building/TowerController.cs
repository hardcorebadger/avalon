using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Worker;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class TowerController : MonoBehaviour {

		public float range = 24f;
		[Require] private Tower.Writer towerWriter;

		private void Start() {
			StartCoroutine (ArcheryUpdate ());
		}

		IEnumerator ArcheryUpdate () {
			while (enabled) {
				
				List<EntityId> enemies = new List<EntityId> ();
				Collider[] cols = Physics.OverlapSphere (transform.position, range);
				foreach (Collider c in cols) {
					CharacterController character = c.GetComponent<CharacterController> ();
					if (character == null)
						continue;
					if (character.owned.getOwner () != GetComponent<OwnedController> ().getOwner ()) {
						enemies.Add (character.gameObject.EntityId ());
					}
				}

				// tell enemies they got hit
				// turn on animation
				if (enemies.Count > 0) {
					towerWriter.Send (new Tower.Update ()
						.SetArcheryState (1)
					);
				} else {
					towerWriter.Send (new Tower.Update ()
						.SetArcheryState (0)
					);
				}

				yield return new WaitForSeconds (1f);
			}
		}

	}

}
