using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class BuildingController : MonoBehaviour {

		[Require] private Building.Writer buildingWriter;
	
		private OwnedController owned;

		public float strength;

		private void OnEnable() {
	
			owned = GetComponent<OwnedController> ();
			buildingWriter.CommandReceiver.OnReceiveDamage.RegisterResponse(OnReceiveDamage);
			strength = buildingWriter.Data.strength;
		
		}

		private void Update() {
			// if the controlling action completes, stop doing it

			if (strength <= 0F) {
				DestroyBuilding ();
			}

		}

		private Nothing OnReceiveDamage(ReceiveDamageRequest request, ICommandCallerInfo callerinfo) {

			strength -= Random.Range(3.0f, 6.0f);
			buildingWriter.Send (new Building.Update ()
				.SetStrength (strength)
//				.AddShowDamage(new Nothing())
			);

			Collider[] cols = Physics.OverlapSphere (transform.position, 50);
			System.Collections.Generic.List<CharacterController> enemies = new System.Collections.Generic.List<CharacterController>();
			System.Collections.Generic.List<CharacterController> friends = new System.Collections.Generic.List<CharacterController>();

			for (int x = 0; x < cols.Length; x++) {
				GameObject g = cols [x].gameObject;
				CharacterController c = g.GetComponent<CharacterController> ();
				if (c != null) {
					if (c.characterWriter.Data.playerId == owned.getOwner()) {
						//my character found 
						friends.Add(c);
					} else if (c.characterWriter.Data.playerId == request.playerId) {
						//other HOSTILE character found
						enemies.Add(c);
					} else {
						//other NEUTRAL/HOSTILE character found
					}
				}

			}
			int i = -1; 
			for (int y = 0; y < friends.Count; y++) {
				i++;
				SpatialOS.Commands.SendCommand (buildingWriter, Character.Commands.HostileAlert.Descriptor, new HostileAlertRequest(enemies[i].characterWriter.EntityId), friends[y].characterWriter.EntityId);
				if (i >= (enemies.Count - 1)) {
					i = -1;
				}

			}

			return new Nothing ();

		}

		public void DestroyBuilding() {
			if (buildingWriter.Data.district.HasValue) {
				// deregiste the construction site
				SpatialOS.Commands.SendCommand (
					buildingWriter, 
					District.Commands.DeregisterBuilding.Descriptor, 
					new BuildingDeregistrationRequest (gameObject.EntityId (), 4), 
					buildingWriter.Data.district.Value
				).OnSuccess (OnDeregisteredSelf);
			} else {
				// settlement construction is not registered, so no deregistration
				OnDeregisteredSelf (new Nothing ());
			}
		}

		private void OnDeregisteredSelf(Nothing n) {
			// finally delete yourself
			SpatialOS.WorkerCommands.DeleteEntity (gameObject.EntityId ());
		}

	}

}