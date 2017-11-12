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

		private ConstructionJobAssignment OnGetJob(ConstructionJobRequest r , ICommandCallerInfo __) {
			if (r.itemInHand != -1 
				&& requirements.ContainsKey (r.itemInHand) 
				&& requirements [r.itemInHand].amount + requirements [r.itemInHand].requested < requirements [r.itemInHand].required 
			) {
				ConstructionRequirement req = requirements [r.itemInHand];
				req.requested++;
				requirements [r.itemInHand] = req;
				return new ConstructionJobAssignment (new Option<int>(r.itemInHand));
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
			
		private TaskResponse OnCompleteJob(ConstructionJobResult result, ICommandCallerInfo _) {
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
				// tell them it's done if they hve nothing left to do
				if (CheckConstructionRequestedProgress())
					return new TaskResponse (200);
				else
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
			CompleteConstruction ();
			return true;
			
		}

		private bool CheckConstructionRequestedProgress() {
			foreach (int key in requirements.Keys) {
				ConstructionRequirement val = requirements[key];
				if (val.amount + val.requested < val.required)
					return false;
			}
			return true;
		}

		private void CompleteConstruction() {
			if (districtBuildingConstruction) {
				// custom replacement for making new settlements
				SpatialOS.Commands.ReserveEntityId (constructionWriter)
					.OnSuccess (result => OnReserveSettlementId (result.ReservedEntityId));
			} else {
				// else just use default building replace
				GetComponent<BuildingController> ().ReplaceBuilding (EntityTemplates.EntityTemplateFactory.CreateEntityTemplate ("building-" + buildingToSpawn, transform.position, owned.getOwner (), owned.getOwnerObject (), buildingWriter.Data.district),false);
			}
		}

		// custom replacement for making new settlements
		private void OnReserveSettlementId(EntityId id) {
			SpatialOS.Commands.CreateEntity (constructionWriter, id, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate ("building-" + buildingToSpawn, transform.position, owned.getOwner (), owned.getOwnerObject (), new Improbable.Collections.Option<EntityId> (id)))
				.OnSuccess (entityId => OnSettlementCreated (id));
		}

		private void OnSettlementCreated(EntityId id) {
			SpatialOS.Commands.SendCommand(constructionWriter, PlayerOnline.Commands.RegisterDistrict.Descriptor, new DistrictRegisterRequest(id), owned.getOwnerObject());
			OnSettlementRegistered (new Nothing ());
		}

		private void OnSettlementRegistered(Nothing n) {
			GetComponent<BuildingController> ().DestroyBuilding ();
		}

	}
		
}