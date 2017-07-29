using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class CommandCenter : MonoBehaviour {

		public static void PerformAction(List<Selectable> selected, RaycastHit2D hit, Vector3 position) {
			List<EntityId> ids = ParseControllableEntities (selected);
			if (hit.collider != null && hit.collider.GetComponent<Selectable> () != null) {
				EntityTargetedAction (ids, hit.collider.gameObject);
			} else {
				PositionTargetedAction (ids, position);
			}
		}

		private static void PositionTargetedAction(List<EntityId> ids, Vector3 position) {
			Debug.Log (position);
			foreach (EntityId id in ids) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.Goto.Descriptor, new GotoRequest (new Vector3d (position.x, 0, position.y)), id);
			}
		}

		private static void EntityTargetedAction(List<EntityId> ids, GameObject target) {
			Debug.Log ("no entity targeted actions yet");
		}

		private static List<EntityId> ParseControllableEntities(List<Selectable> selected) {
			List<EntityId> ids = new List<EntityId>();
			foreach (Selectable s in selected) {
				CharacterVisualizer cv = s.GetComponent<CharacterVisualizer> ();
				if (cv != null && cv.CanControl ()) {
					ids.Add (cv.gameObject.EntityId());
				}
			}
			return ids;
		}
			
	}

}