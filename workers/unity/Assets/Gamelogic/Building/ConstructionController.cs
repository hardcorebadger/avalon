using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;


namespace Assets.Gamelogic.Core {
	
	public class ConstructionController : MonoBehaviour {

		[Require] private Construction.Writer constructionWriter;
		[Require] private Building.Writer buildingWriter;

		public string buildingToSpawn;

		private Map<int,ConstructionRequirement> requirements;
		private OwnedController owned;

		public bool districtBuildingConstruction;
		private bool shouldStall = false;
		private int stallCounter = 0;

		// Use this for initialization
		void OnEnable () {

			constructionWriter.CommandReceiver.OnGetJob.RegisterResponse(OnGetJob);
			constructionWriter.CommandReceiver.OnCompleteJob.RegisterResponse(OnCompleteJob);

			requirements = constructionWriter.Data.requirements;

			owned = GetComponent<OwnedController> ();

		}

		void OnDisable() {
			constructionWriter.CommandReceiver.OnGetJob.DeregisterResponse();
			constructionWriter.CommandReceiver.OnCompleteJob.DeregisterResponse();
		}

		private ConstructionJobAssignment OnGetJob(Nothing _ , ICommandCallerInfo __) {
			// basically, if workers are sending back user errors, tell them to wait a bit
			if (shouldStall) {
				stallCounter++;
				if (stallCounter <= GetComponent<WorkSiteController> ().workers.Count)
					return new ConstructionJobAssignment (new Option<int> ());
				else {
					shouldStall = false;
					stallCounter = 0;
				}
			}
			foreach (int item in requirements.Keys) {
				ConstructionRequirement req = requirements [item];
				// if we need more still
				if (req.amount < req.required) {
					// if we haven't assigned it all yet
					if (req.requested < req.required - req.amount) {
						req.requested++;
						requirements [item] = req;
						return new ConstructionJobAssignment (new Option<int>(item));
					}
				}
			}
			SendRequirementsUpdate ();
			return new ConstructionJobAssignment (new Option<int>());
		}

		private Nothing OnCompleteJob(ConstructionJobResult result, ICommandCallerInfo _) {
			if (!result.assignment.toGet.HasValue)
				return new Nothing ();
			ConstructionRequirement req = requirements [result.assignment.toGet.Value];
			req.requested -= 1;
			if (AIAction.OnSuccess (result.response))
				req.amount += 1;
			else if (AIAction.OnUserError(result.response)) {
				shouldStall = true;
			}
			requirements [result.assignment.toGet.Value] = req;

			if (!CheckConstructionProgress ())
				SendRequirementsUpdate ();
			
			return new Nothing ();
		}

		private void SendRequirementsUpdate() {
			constructionWriter.Send (new Construction.Update ()
				.SetRequirements (requirements)
			);
		}

		public void Log() {
			foreach (int key in requirements.Keys) {
				ConstructionRequirement val = requirements[key];
				Debug.Log(Item.GetName (key) + ": " + val.amount + " / " + val.required);
			}
		}

		private bool CheckConstructionProgress() {
			foreach (int key in requirements.Keys) {
				ConstructionRequirement val = requirements[key];
				if (val.amount < val.required)
					return false;
			}
			SpatialOS.Commands.ReserveEntityId (constructionWriter)
				.OnSuccess (result => OnReserveEntityId (result.ReservedEntityId));
			return true;
			
		}

		private void OnReserveEntityId(EntityId id) {
			if (districtBuildingConstruction) {
				SpatialOS.Commands.CreateEntity (constructionWriter, id, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate ("building-" + buildingToSpawn, transform.position, owned.getOwner (), owned.getOwnerObject(), new Improbable.Collections.Option<EntityId> (id)))
					.OnSuccess (entityId => OnBuildingCreated (id));
			} else {
				SpatialOS.Commands.CreateEntity (constructionWriter, id, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate ("building-" + buildingToSpawn, transform.position, owned.getOwner (), owned.getOwnerObject(), buildingWriter.Data.district))
					.OnSuccess (entityId => OnBuildingCreated (id));
			}
		}

		private void OnBuildingCreated(EntityId id) {
			if (!districtBuildingConstruction) {
				int beds = 0;
				List<int> acceptingItems = new List<int> ();

				if (gameObject.name.Contains ("house-3d")) {
					beds = 4;
				}
				SpatialOS.Commands.SendCommand (
					constructionWriter, 
					District.Commands.RegisterBuilding.Descriptor, 
					new BuildingRegistrationRequest (id, new Vector3d(transform.position.x, transform.position.y, transform.position.z), beds, acceptingItems), 
					buildingWriter.Data.district.Value
				).OnSuccess (OnBuildingRegistered);
			} else {
				// pre registered if its a settlement

				SpatialOS.Commands.SendCommand(constructionWriter, PlayerOnline.Commands.RegisterDistrict.Descriptor, new DistrictRegisterRequest(id), owned.getOwnerObject());

				OnBuildingRegistered (new Nothing ());
			}
		}

		private void OnBuildingRegistered(Nothing n) {
			GetComponent<BuildingController> ().DestroyBuilding ();
		}

	}
		
}