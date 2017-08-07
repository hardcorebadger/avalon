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

		private static  List<EntityId> agents;
		private static RaycastHit2D hit;
		private static Vector3 position;
		private static float radius;
		private static GameObject target;
		private static bool radial;

		// called from selection manager
		public static void InterpretRadialCommand(List<Selectable> s, Vector3 p, float r) {
			agents = ParseControllableEntities (s);
			if (agents.Count == 0)
				return;
			
			radius = r;
			position = p;
			radial = true;

			List<string> options = new List<string> ();
			Collider2D[] colliders = Physics2D.OverlapCircleAll (p, r);
			foreach (Collider2D c in colliders) {
				ParseOptions (ref options, c.gameObject);
			}
			UIManager.OpenCommandPicker(options);
		}

		// called from selection manager
		public static void InterpretClickCommand(List<Selectable> s, RaycastHit2D h, Vector3 p) {
			agents = ParseControllableEntities (s);
			if (agents.Count == 0)
				return;

			hit = h;
			position = p;
			radial = false;

			List<string> options = new List<string> ();
			if (hit.collider != null) {
				target = hit.collider.gameObject;
				ParseOptions (ref options, target);
			} else {
				// prematurely assume walk
				ExecutePositionTargetedCommand ("goto");
				return;
			}

			UIManager.OpenCommandPicker(options);
		}

		// called from ui picker
		public static void OnCommandSelected(string command) {
			if (radial) {
				ExecuteRadialTargetedCommand (command);
			} if (target != null) {
				ExecuteEntityTargetedCommand (command);
			} else {
				ExecutePositionTargetedCommand (command);
			}
		}

		// called from ui picker
		public static void OnCommandCancelled() {
			hit = new RaycastHit2D();
			position = Vector3.zero;
			agents = null;
			radial = false;
			target = null;
		}

		private static void ExecuteRadialTargetedCommand(string command) {
			int index = 0;
			foreach (EntityId id in agents) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.RadiusTarget.Descriptor, new RadiusTargetRequest (new Vector3d (position.x, 0, position.y), radius, command, new GroupInfo(index,agents.Count)), id);
				index++;
			}
		}

		private static void ExecuteEntityTargetedCommand(string command) {
			int index = 0;
			foreach (EntityId id in agents) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (target.EntityId(), command, new GroupInfo(index,agents.Count)), id);
				index++;
			}
		}

		private static void ExecutePositionTargetedCommand(string command) {
			int index = 0;
			foreach (EntityId id in agents) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.PositionTarget.Descriptor, new PositionTargetRequest (new Vector3d (position.x, 0, position.y), command, new GroupInfo(index,agents.Count)), id);
				index++;
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

		//TODO this is the function the parses options from targets
		private static void ParseOptions(ref List<string> options, GameObject g) {
			GatherableVisualizer gatherable = g.GetComponent<GatherableVisualizer> ();
			if (gatherable != null) {
				//TODO actually add the options here
				if (!options.Contains ("gather"))
					options.Add ("gather");
			}
			ConstructionVisualizer construction = g.GetComponent<ConstructionVisualizer> ();
			if (construction != null) {
				if (!options.Contains ("build"))
					options.Add ("build");
			}
		}
			
	}

}