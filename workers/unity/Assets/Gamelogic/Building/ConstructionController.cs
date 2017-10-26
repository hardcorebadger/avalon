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
			Debug.LogWarning ("job request  ");
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
			Debug.LogWarning ("this is happening");
			return new ConstructionJobAssignment (new Option<int>());
		}
			
		private TaskResponse OnCompleteJob(ConstructionJobResult result, ICommandCallerInfo _) {
			Debug.LogWarning ("job complete");
			if (!result.assignment.toGet.HasValue)
				return new TaskResponse (100);
			
			ConstructionRequirement req = requirements [result.assignment.toGet.Value];
			req.requested -= 1;

			if (AIAction.OnSuccess (result.response))
				req.amount += 1;

			requirements [result.assignment.toGet.Value] = req;

			if (result.response == 403 /* district has no applicable items */ || 
				result.response == 402 || result.response == 401 /* no district or non-applicable item in hand, tell them to fuck off */)
				return new TaskResponse (400);
			
			if (!CheckConstructionProgress ()) {
				SendRequirementsUpdate ();
				return new TaskResponse (100);
			} else
				return new TaskResponse (200);
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