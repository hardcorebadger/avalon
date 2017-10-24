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

			constructionWriter.CommandReceiver.OnGive.RegisterResponse(OnGive);
			constructionWriter.CommandReceiver.OnGiveMultiple.RegisterResponse(OnGiveMultiple);

			requirements = constructionWriter.Data.requirements;

			owned = GetComponent<OwnedController> ();

		}

		void OnDisable() {
			constructionWriter.CommandReceiver.OnGive.DeregisterResponse();
			constructionWriter.CommandReceiver.OnGiveMultiple.DeregisterResponse();
		}

		private ConstructionGiveResponse OnGive(ItemStack itemStack, ICommandCallerInfo callerinfo) {
			bool f = Insert (itemStack.id, itemStack.amount);
			bool c = CheckConstructionProgress ();
			return new ConstructionGiveResponse (f, c);
		}

		private ConstructionGiveResponse OnGiveMultiple(ItemStackList itemStackList, ICommandCallerInfo callerinfo) {
			foreach (int id in itemStackList.inventory.Keys) {
				if (!requirements.ContainsKey (id))
					return new ConstructionGiveResponse (false, false);
			}
			foreach (int id in itemStackList.inventory.Keys) {
				Insert (id, itemStackList.inventory [id]);
			}
			bool c = CheckConstructionProgress ();
			return new ConstructionGiveResponse (true, c);
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

		public bool Insert(int id, int amount) {
			if (!requirements.ContainsKey (id))
				return false;

			ConstructionRequirement r = requirements [id];
			r.amount += amount;
			requirements [id] = r;
			if (r.amount > r.required)
				Debug.LogWarning ("construction overfilling - item loss will occur");

			Log ();
			SendRequirementsUpdate ();
			return true;
		}

		private void Clear() {
			requirements.Clear ();
			SendRequirementsUpdate ();
		}

		public int Count(int i) {
			ConstructionRequirement req;
			requirements.TryGetValue (i, out req);
			return req.amount;
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