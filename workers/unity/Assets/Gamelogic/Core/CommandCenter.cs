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

		public static void PerformRadialAction(List<Selectable> selected, Vector3 position, float radius) {
			List<EntityId> ids = ParseControllableEntities (selected);
			foreach (EntityId id in ids) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.RadiusTarget.Descriptor, new RadiusTargetRequest (new Vector3d (position.x, 0, position.y), radius, "gather"), id);
			}
		}

		public static void PerformAction(List<Selectable> selected, RaycastHit2D hit, Vector3 position) {
			List<EntityId> ids = ParseControllableEntities (selected);
			if (hit.collider != null && hit.collider.GetComponent<Selectable> () != null) {
				EntityTargetedAction (ids, hit.collider.gameObject);
			} else {
				PositionTargetedAction (ids, position);
			}
		}

		private static void PositionTargetedAction(List<EntityId> ids, Vector3 position) {
			foreach (EntityId id in ids) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.PositionTarget.Descriptor, new PositionTargetRequest (new Vector3d (position.x, 0, position.y), "goto"), id);
			}
		}

		private static void EntityTargetedAction(List<EntityId> ids, GameObject target) {
			foreach (EntityId id in ids) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (target.EntityId(), "gather"), id);
			}
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