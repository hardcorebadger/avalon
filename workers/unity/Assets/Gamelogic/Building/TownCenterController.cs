using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {

	public class TownCenterController : MonoBehaviour {

		[Require] private TownCenter.Writer townCenterWriter;

		private List<EntityId> tentativeCancellations;

		private void OnEnable() {
			townCenterWriter.CommandReceiver.OnAddCitizen.RegisterResponse (OnAddCitizen);
			townCenterWriter.CommandReceiver.OnAddBuilding.RegisterResponse (OnAddBuilding);
			townCenterWriter.CommandReceiver.OnRemoveCitizen.RegisterResponse (OnRemoveCitizen);
			townCenterWriter.CommandReceiver.OnRemoveBuilding.RegisterResponse (OnRemoveBuilding);

			townCenterWriter.CommandReceiver.OnTentativeAddCitizen.RegisterResponse (OnTentativeAddCitizen);
			townCenterWriter.CommandReceiver.OnTentativeRemoveCitizen.RegisterResponse (OnTentativeRemoveCitizen);

			tentativeCancellations = new List<EntityId> ();

			StartCoroutine (TownTick());
		}

		private void OnDisable() {
			townCenterWriter.CommandReceiver.OnAddCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnAddBuilding.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnRemoveCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnRemoveBuilding.DeregisterResponse ();

			townCenterWriter.CommandReceiver.OnTentativeAddCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnTentativeRemoveCitizen.DeregisterResponse ();

			foreach (EntityId id in townCenterWriter.Data.citizens) {
				SpatialOS.Commands.SendCommand (townCenterWriter, Character.Commands.LeaveTown.Descriptor, new Nothing (), id);
			}

			foreach (EntityId id in townCenterWriter.Data.buildings) {
				SpatialOS.Commands.SendCommand (townCenterWriter, Building.Commands.LeaveTown.Descriptor, new Nothing (), id);
			}
		}

		private System.Collections.IEnumerator TownTick() {
			while (enabled) {

				QueryBuildings ();


				// tick ever minute
				yield return new WaitForSeconds (60f);
			}
		}

		// definitive commands

		private TownAddResponse OnAddCitizen(TownAddRequest request, ICommandCallerInfo callerinfo) {
			AddCitizen (request.entity);
			return new TownAddResponse ();
		}

		private TownAddResponse OnAddBuilding(TownAddRequest request, ICommandCallerInfo callerinfo) {
			AddBuilding (request.entity);
			return new TownAddResponse ();
		}

		private TownRemoveResponse OnRemoveCitizen(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			RemoveCitizen(request.entity);
			return new TownRemoveResponse ();
		}

		private TownRemoveResponse OnRemoveBuilding(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			RemoveBuilding (request.entity);
			return new TownRemoveResponse ();
		}

		// tentative commands are used when characters are moving over worker borders
		// when a character is disabled on worker 1 a tentative removal is called
		// a timer is set, as long as a tentative add is recieved before the timer, the citizen stays in the list
		// if an add is not recieved, the character is removed

		// this system is here so that however a chracter is destroyed, the world doesn't fall apart from bad links
		// usually characters should be destoring themselves, in which case they can definitively remove themselves before destruction
		// Also there's no way to know when the character actually spawned, so this is used to add them tentatively in case they are new
	

		private TownAddResponse OnTentativeAddCitizen(TownAddRequest request, ICommandCallerInfo callerinfo) {
			if (!townCenterWriter.Data.citizens.Contains(request.entity)) 
				AddCitizen(request.entity);
			else
				tentativeCancellations.Add(request.entity);
			
			return new TownAddResponse ();
		}

		private TownRemoveResponse OnTentativeRemoveCitizen(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			StartCoroutine(TentativeRemoval(request.entity));
			return new TownRemoveResponse ();
		}

		private System.Collections.IEnumerator TentativeRemoval(EntityId id) {
			yield return new WaitForSeconds(5f);
			if (!tentativeCancellations.Contains (id)) {
				RemoveCitizen (id);
			} else {
				tentativeCancellations.Remove (id);
			}
		}

		// actual update functions

		public void RemoveCitizen(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.citizens;
			newList.Remove (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetCitizens (newList)
			);
		}

		public void RemoveBuilding(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.buildings;
			newList.Remove (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetBuildings (newList)
			);
		}

		public void AddCitizen(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.citizens;
			newList.Add (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetCitizens (newList)
			);
		}

		public void AddBuilding(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.buildings;
			newList.Add (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetBuildings (newList)
			);
		}

		private void QueryBuildings() {
			IConstraint[] queryConstraints = new IConstraint[townCenterWriter.Data.buildings.Count];
			int i = 0;
			foreach (EntityId id in townCenterWriter.Data.buildings) {
				queryConstraints [i] = Query.HasEntityId (id);
				i++;
			}

			if (townCenterWriter.Data.buildings.Count == 0) {
				return;
			} else if (townCenterWriter.Data.buildings.Count == 1) {
				var entityQuery = queryConstraints[0].ReturnComponents(Building.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulBuildingQuery);
			} else if (townCenterWriter.Data.buildings.Count >= 2) {
				var entityQuery = Query.Or(queryConstraints[0], queryConstraints[1], queryConstraints).ReturnComponents(Building.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulBuildingQuery);
			}
		}

		private void OnSuccessfulBuildingQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			if (resultMap.Count == 0) {
				return;
			}
			int livingSpaces = 0;
			foreach (EntityId id in resultMap.Keys) {
				Entity e = resultMap[id];
				Improbable.Collections.Option<IComponentData<Building>> g = e.Get<Building>();
				BuildingData building = g.Value.Get().Value;
				livingSpaces += building.livingSpaces;
			}
			int chances = livingSpaces - townCenterWriter.Data.citizens.Count;
			for (int i = 0; i < chances; i++) {
				// put randomizer in here...
				SpatialOS.Commands.CreateEntity (townCenterWriter, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate (
					"character", 
					transform.position, 
					GetComponent<OwnedController> ().getOwner (), 
					gameObject.EntityId ()
				));
			}
		}


	}

}
