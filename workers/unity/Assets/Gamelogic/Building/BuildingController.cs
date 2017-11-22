using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Worker;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class BuildingController : MonoBehaviour {

		[Require] private Building.Writer buildingWriter;
	
		private OwnedController owned;
		public Transform door;

		public float strength;
		public Option<EntityId> district;
		public string constructionName;
		private bool hasDied = false;
		private Option<EntityId> mostRecentAttacker;
		private Option<int> mostRecentAttackerId;

		private void OnEnable() {
	
			owned = GetComponent<OwnedController> ();
			buildingWriter.CommandReceiver.OnReceiveDamage.RegisterResponse(OnReceiveDamage);
			strength = buildingWriter.Data.strength;
			door = transform.FindChild ("door");
			district = buildingWriter.Data.district;
		}

		private void Update() {
			// if the controlling action completes, stop doing it

			if (strength <= 0F && !hasDied) {
				DistrictController district = GetComponent<DistrictController> ();
				if (district != null) {
					if (mostRecentAttacker.HasValue) {
						Debug.LogWarning ("cedeing town to: " + mostRecentAttacker.Value.Id + " " + mostRecentAttackerId.Value);
						district.Cede (mostRecentAttacker.Value, mostRecentAttackerId.Value);
						strength = 1000f;
						buildingWriter.Send (new Building.Update ()
							.SetStrength (strength)
						);
					}
				} else {
					hasDied = true;
					ReplaceBuilding (EntityTemplates.EntityTemplateFactory.CreateDestroyedBuildingTemplate (constructionName, transform.position, owned.getOwner (), owned.getOwnerObject (), buildingWriter.Data.district), true);
				}

			}

		}

		private Nothing OnReceiveDamage(ReceiveDamageRequest request, ICommandCallerInfo callerinfo) {
			
			mostRecentAttacker = request.playerEntityId;
			mostRecentAttackerId = request.playerId;

			strength -= Random.Range(3.0f, 6.0f);
			buildingWriter.Send (new Building.Update ()
				.SetStrength (strength)
//				.AddShowDamage(new Nothing())
			);

			//TODO add to hostile

//			Collider[] cols = Physics.OverlapSphere (transform.position, 50);
//			System.Collections.Generic.List<CharacterController> enemies = new System.Collections.Generic.List<CharacterController>();
//			System.Collections.Generic.List<CharacterController> friends = new System.Collections.Generic.List<CharacterController>();
//
//			for (int x = 0; x < cols.Length; x++) {
//				GameObject g = cols [x].gameObject;
//				CharacterController c = g.GetComponent<CharacterController> ();
//				if (c != null) {
//					if (c.owned.getOwnerObject().Id == owned.getOwnerObject().Id) {
//						//my character found 
//						friends.Add(c);
//					} else if (c.owned.getOwnerObject().Id == request.playerEntityId.Id) {
//						//other HOSTILE character found
//						enemies.Add(c);
//					} else {
//						//other NEUTRAL/HOSTILE character found
//					}
//				}
//
//			}
//			if (enemies.Count > 0) {
//				int i = -1; 
//				for (int y = 0; y < friends.Count; y++) {
//					i++;
//					SpatialOS.Commands.SendCommand (buildingWriter, Character.Commands.HostileAlert.Descriptor, new HostileAlertRequest (enemies [i].characterWriter.EntityId), friends [y].characterWriter.EntityId);
//					if (i >= (enemies.Count - 1)) {
//						i = -1;
//					}
//
//				}
//			}

			return new Nothing ();
		}

		public int GetBeds() {
			return buildingWriter.Data.beds;
		}

		public void DestroyBuilding() {
			WorkSiteController workSite = GetComponent<WorkSiteController> ();
			if (workSite != null) {
				workSite.FireAll ();
			}
			if (buildingWriter.Data.district.HasValue) {
				// deregister the construction site
				int beds = 0;

				if (gameObject.name.Contains ("building-house-3d")) {
					beds = 4;
				}
				SpatialOS.Commands.SendCommand (
					buildingWriter, 
					District.Commands.DeregisterBuilding.Descriptor, 
					new BuildingDeregistrationRequest (gameObject.EntityId (), beds), 
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


		public void ReplaceBuilding(Entity e, bool isConstruction) {
			SpatialOS.Commands.ReserveEntityId (buildingWriter)
				.OnSuccess (result => OnReserveReplacementEntityId (result.ReservedEntityId,e,isConstruction));
		}

		private void OnReserveReplacementEntityId(EntityId id, Entity e, bool b) {
			SpatialOS.Commands.CreateEntity (buildingWriter, id, e)
				.OnSuccess (entityId => OnReplacementBuildingCreated (id,b));
		}

		private void OnReplacementBuildingCreated(EntityId id, bool b) {
			if (!buildingWriter.Data.district.HasValue) {
				OnReplacementBuildingRegistered (new Nothing());
				return;
			}
			int beds = 0;
			List<int> acceptingItems = new List<int> ();

			if (gameObject.name.Contains ("house-3d")) {
				beds = 4;
			}
			SpatialOS.Commands.SendCommand (
				buildingWriter, 
				District.Commands.RegisterBuilding.Descriptor, 
				new BuildingRegistrationRequest (id, new Vector3d(transform.position.x, transform.position.y, transform.position.z), beds, acceptingItems, b), 
				buildingWriter.Data.district.Value
			).OnSuccess (OnReplacementBuildingRegistered);
		}

		private void OnReplacementBuildingRegistered(Nothing n) {
			DestroyBuilding ();
		}

	}

}