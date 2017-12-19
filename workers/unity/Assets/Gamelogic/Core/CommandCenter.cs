using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Entity.Component;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;

namespace Assets.Gamelogic.Core {

	public class CommandCenter : MonoBehaviour {

		private static  List<EntityId> agents;
		private static RaycastHit hit;
		private static Vector3 position;
		private static float radius;
		private static GameObject target;
		private static bool radial;

		// called from selection manager
		public static void InterpretRadialCommand(List<CharacterVisualizer> s, List<EntityId> rs, Vector3 p, float r) {
			agents = ParseControllableEntities (s, rs);
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

			if (options.Count == 0)
				return;
			else if (options.Count == 1)
				OnCommandSelected (options.ToArray () [0]);
			else
				UIManager.OpenCommandPicker (options);
			
		}

		// called from selection manager
		public static void InterpretClickCommand(List<CharacterVisualizer> s, List<EntityId> rs, RaycastHit h) {
			agents = ParseControllableEntities (s, rs);
			if (agents.Count == 0)
				return;

			hit = h;
			position = hit.point;
			radial = false;

			List<string> options = new List<string> ();
			if (hit.collider != null && hit.collider.gameObject.layer != 10) {
				target = hit.collider.gameObject;
				ParseOptions (ref options, target);
			} else {
				// prematurely assume walk
				ExecutePositionTargetedCommand ("goto");
				return;
			}
			if (options.Count == 1) {
				OnCommandSelected (options.ToArray () [0]);
			} else {
				UIManager.OpenCommandPicker (options);
			}
		}

		// called from ui picker
		public static void OnCommandSelected(string command) {
			foreach (CharacterVisualizer s in SelectionManager.instance.selected) {
				s.OnAcceptCommand ();
			}
			if (radial) {
				ExecuteRadialTargetedCommand (command);
			} if (target != null) {
				ExecuteEntityTargetedCommand (command);
			} else {
				ExecutePositionTargetedCommand (command);
			}
			SelectionManager.instance.ClearSelected ();
		}

		// called from ui picker
		public static void OnCommandCancelled() {
			hit = new RaycastHit();
			position = Vector3.zero;
			agents = null;
			radial = false;
			target = null;
		}

		public static void SendChat(string message) {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerCreator.Commands.SendChat.Descriptor, new SendChatRequest(message, Bootstrap.playerId), Bootstrap.playerCreator);
		}

		// depricated
		private static void ExecuteRadialTargetedCommand(string command) {
			foreach (EntityId id in agents) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.RadiusTarget.Descriptor, new RadiusTargetRequest (new Vector3d (position.x, 0, position.y), radius, command), id);
			}
		}

		private static void ExecuteEntityTargetedCommand(string command) {
			if (command == "gather") 
				ExecuteGatherableTargetedCommand ();
			else if (command == "attack")
				ExecuteAttackTargetedCommand ();
			else {
				foreach (EntityId id in agents) {
					SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (target.EntityId(), command), id);
				}
			}
		}

		private static void ExecuteAttackTargetedCommand() {
			string command = "attack";
			if (agents.Count < 1)
				return;

			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (target.EntityId(), command), agents[0]);
			agents.RemoveAt (0);

			if (agents.Count < 1)
				return;

			Collider[] cols = Physics.OverlapSphere (position, UIManager.instance.coOpRadius);
			List<Collider> clist = new List<Collider> (cols);
			clist.Sort(delegate(Collider c1, Collider c2){
				return Vector3.Distance(target.transform.position, c1.transform.position).CompareTo
					((Vector3.Distance(target.transform.position, c2.transform.position)));   
			});
			List<GameObject> used = new List<GameObject> ();
			used.Add (target);

			foreach (Collider c in clist) {
				if (used.Contains (c.gameObject))
					continue;
				CharacterVisualizer character = c.gameObject.GetComponent<CharacterVisualizer> ();
				if (character == null)
					continue;
				if (!character.CanControl()) {
					used.Add (c.gameObject);
					SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (c.gameObject.EntityId(), command), agents [0]);
					agents.RemoveAt (0);
					if (agents.Count < 1)
						return;
				}
			}

		}

		private static void ExecuteGatherableTargetedCommand() {
			string command = "gather";
			if (agents.Count < 1)
				return;

			EntityId id = agents [0];
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (target.EntityId(), command), agents[0]);
			agents.RemoveAll (x => x.Id == agents [0].Id);

			if (agents.Count < 1)
				return;

			Collider[] cols = Physics.OverlapSphere (position, UIManager.instance.coOpRadius);
			List<Collider> clist = new List<Collider> (cols);
			clist.Sort(delegate(Collider c1, Collider c2){
				return Vector3.Distance(target.transform.position, c1.transform.position).CompareTo
					((Vector3.Distance(target.transform.position, c2.transform.position)));   
			});
			WorkType t = target.GetComponentInParent<GatherableVisualizer> ().gatherableReader.Data.workType;
			List<GameObject> used = new List<GameObject> ();
			used.Add (target);

			foreach (Collider c in clist) {
				if (used.Contains (c.gameObject))
					continue;
				GatherableVisualizer gatherable = c.gameObject.GetComponentInParent<GatherableVisualizer> ();
				if (gatherable == null)
					continue;
				if (gatherable.gatherableReader.Data.workType == t) {
					used.Add (c.gameObject);
					SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.EntityTarget.Descriptor, new EntityTargetRequest (c.gameObject.EntityId(), command), agents [0]);
					agents.RemoveAt (0);
					if (agents.Count < 1)
						return;
				}
			}

		}

		private static void ExecutePositionTargetedCommand(string command) {
			foreach (EntityId id in agents) {
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Character.Commands.PositionTarget.Descriptor, new PositionTargetRequest (new Vector3d (position.x, position.y, position.z), command), id);
			}
		}

		private static List<EntityId> ParseControllableEntities(List<CharacterVisualizer> localSelected, List<EntityId> selected) {
			foreach (CharacterVisualizer cv in localSelected) {
				if (cv != null && cv.CanControl ()) {
					selected.Add (cv.gameObject.EntityId());
				}
			}
			return selected;
		}

		//TODO this is the function the parses options from targets
		private static void ParseOptions(ref List<string> options, GameObject g) {
			GatherableVisualizer gatherable = g.GetComponentInParent<GatherableVisualizer> ();
			if (gatherable != null) {
				//TODO actually add the options here
				if (!options.Contains ("gather"))
					options.Add ("gather");
			}
			WorkSiteVisualizer worksite = g.GetComponentInParent<WorkSiteVisualizer> ();
			if (worksite != null) {
				if (!options.Contains ("work"))
					options.Add ("work");
			}
			CharacterVisualizer character = g.GetComponentInParent<CharacterVisualizer> ();
			if (character != null && !character.CanControl()) {
				if (!options.Contains ("attack"))
					options.Add ("attack");
			}
			BuildingVisualizer building = g.GetComponentInParent<BuildingVisualizer> ();
			if (building != null && !building.CanControl()) {
				if (!options.Contains ("damage"))
					options.Add ("damage");
			}
			ConstructionVisualizer construction = g.GetComponentInParent<ConstructionVisualizer> ();
			if (construction != null && construction.CanControl()) {
				if (!options.Contains ("construction"))
					options.Add ("construction");
			}
		}
			
	}

}