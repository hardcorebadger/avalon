using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using System.Linq;

namespace Assets.Gamelogic.Core {

	public class DistrictController : MonoBehaviour {
		
		[Require] private District.Writer districtWriter;
		[Require] private Building.Writer buildingWriter;
		[Require] private Inventory.Writer inventoryWriter;

		public GameObject spawn;

		Map<EntityId, Vector3d> positionMap;
		List<EntityId> constructionList;
		Map<EntityId, JobInfoOption> characters;

		int beds;
		float spawnTimer = -1f;
		public BuildingController building;
		public OwnedController owned;

		void OnEnable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.RegisterResponse (OnRegisterBuilding);
			districtWriter.CommandReceiver.OnDeregisterBuilding.RegisterResponse (OnDeregisterBuilding);
			districtWriter.CommandReceiver.OnRegisterCharacter.RegisterResponse (OnRegisterCharacter);
			districtWriter.CommandReceiver.OnDeregisterCharacter.RegisterResponse (OnDeregisterCharacter);
			districtWriter.CommandReceiver.OnSetJob.RegisterResponse (OnSetJob);

			districtWriter.CommandReceiver.OnFindItemGet.RegisterResponse (OnFindItemGet);
			districtWriter.CommandReceiver.OnFindItemPut.RegisterResponse (OnFindItemPut);

			positionMap = districtWriter.Data.positionMap;
			characters = districtWriter.Data.characterMap;
			beds = districtWriter.Data.beds;
			constructionList = districtWriter.Data.constructionList;
			building = GetComponent<BuildingController> ();
			owned = GetComponent<OwnedController> ();

			if (beds == 0) {
				BuildingController b = GetComponent<BuildingController> ();

				if (b != null) {
					beds = b.GetBeds();
					districtWriter.Send (new District.Update ()
						.SetBeds(beds)
					);
				}
			}

			StartCoroutine (ItemTrendUpdate());
		}

		void OnDisable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.DeregisterResponse ();
			districtWriter.CommandReceiver.OnDeregisterBuilding.DeregisterResponse ();

			districtWriter.CommandReceiver.OnRegisterCharacter.DeregisterResponse ();
			districtWriter.CommandReceiver.OnDeregisterCharacter.DeregisterResponse ();

			districtWriter.CommandReceiver.OnSetJob.DeregisterResponse ();

			districtWriter.CommandReceiver.OnFindItemGet.DeregisterResponse ();
			districtWriter.CommandReceiver.OnFindItemPut.DeregisterResponse ();

		}

		void Update() {
			spawnTimer += Time.deltaTime;
			if (spawnTimer >= 60f) {
				if (characters.Count < beds) {
					SpatialOS.Commands.ReserveEntityId (districtWriter)
						.OnSuccess (result => SpawnCharacterEntity (result.ReservedEntityId));
				}
				spawnTimer = 0F;
			}
		}

		private System.Collections.IEnumerator ItemTrendUpdate() {
			while (enabled) {
				Map<int, int> inventory = inventoryWriter.Data.inventory;
				Map<int, ItemTrend> trends = districtWriter.Data.itemTrends;
				foreach (int id in inventory.Keys) {
					if (!trends.ContainsKey (id))
						trends.Add (id, new ItemTrend (inventory[id], 0));
					else {
						ItemTrend t = trends [id];
						if (t.currentTrend == 0)
							t.currentTrend = inventory [id] - t.previousAmount;
						else {
							float f = ((inventory [id] - t.previousAmount) * 80f + t.currentTrend * 20f);
							t.currentTrend = Mathf.RoundToInt (f/100f);
						}
						t.previousAmount = inventory [id];
						trends [id] = t;
					}
				}
				districtWriter.Send (new District.Update ()
					.SetItemTrends(trends)
				);
				yield return new WaitForSeconds (60f);
			}
		}

		public void SpawnCharacterEntity(EntityId entityId) {
			SpatialOS.Commands.CreateEntity(districtWriter, entityId, Gamelogic.EntityTemplates.EntityTemplateFactory.CreateCharacterTemplate(building.door.position, owned.getOwner(), owned.getOwnerObject(), new Option<EntityId>(districtWriter.EntityId)))
					.OnSuccess(result => RegisterSpawnedCharacter(entityId));
		}

		public void RegisterSpawnedCharacter(EntityId characterId) {

			SpatialOS.Commands.SendCommand (districtWriter, Player.Commands.ReceiveNotification.Descriptor, new ReceiveNotificationRequest ("A new worker has spawned!", new Option<EntityId>(characterId), new Option<Vector3d>()), owned.getOwnerObject ());

			SpatialOS.Commands.SendCommand (districtWriter, PlayerOnline.Commands.RegisterCharacter.Descriptor, new CharacterPlayerRegisterRequest (characterId), owned.getOwnerObject ());

			characters.Add (characterId, new JobInfoOption (new Option<JobInfo> ()));
			districtWriter.Send (new District.Update ()
				.SetCharacterMap(characters)
			);
		}

		private Nothing OnRegisterBuilding(BuildingRegistrationRequest r, ICommandCallerInfo _) {
			positionMap.Add (r.buildingId, r.position);
			if (r.construction)
				constructionList.Add (r.buildingId);
			beds += r.beds;
			districtWriter.Send (new District.Update ()
				.SetPositionMap(positionMap)
				.SetBeds(beds)
				.SetConstructionList(constructionList)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterBuilding(BuildingDeregistrationRequest r, ICommandCallerInfo _) {
			positionMap.Remove (r.buildingId);
			if (constructionList.Contains (r.buildingId))
				constructionList.Remove (r.buildingId);
			beds -= r.beds;
			districtWriter.Send (new District.Update ()
				.SetPositionMap(positionMap)
				.SetBeds(beds)
				.SetConstructionList(constructionList)
			);
			return new Nothing ();
		}

		private ItemFindResponse OnFindItemGet(ItemFindRequest r, ICommandCallerInfo _) {
			if (GetComponent<InventoryController>().HasItem(r.id))
				return new ItemFindResponse (gameObject.EntityId(), positionMap[gameObject.EntityId()]);

			return new ItemFindResponse (new Option<EntityId> (), new Option<Vector3d> ());
		}

		// plug
		private ItemFindResponse OnFindItemPut(ItemFindRequest r, ICommandCallerInfo _) {
			if (GetComponent<InventoryController>().HasRoom())
				return new ItemFindResponse (gameObject.EntityId(), positionMap[gameObject.EntityId()]);
			return new ItemFindResponse (new Option<EntityId> (), new Option<Vector3d> ());
		}

		private Nothing OnRegisterCharacter(CharacterRegistrationRequest r, ICommandCallerInfo _) {
			foreach (var characterId in r.characters) {
				characters.Add (characterId, new JobInfoOption (new Option<JobInfo> ()));
			}
			districtWriter.Send (new District.Update ()
				.SetCharacterMap(characters)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterCharacter(CharacterDeregistrationRequest r, ICommandCallerInfo _) {
			foreach (var e in r.characters) {
				characters.Remove(e);
			}
			districtWriter.Send (new District.Update ()
				.SetCharacterMap(characters)
			);
			return new Nothing ();
		}

		private BuildingQueryResponse OnFindConstructionSite(FindConstructionRequest r, ICommandCallerInfo __) {
			foreach (EntityId id in constructionList) {
				if (id.Id != r.prev.Id)
					return new BuildingQueryResponse (new Option<EntityId>(id));
			}
			return new BuildingQueryResponse (new Option<EntityId>());
		}

		public Nothing OnSetJob(SetJobRequest r, ICommandCallerInfo _) {
			if (!characters.ContainsKey(r.character))
				return new Nothing ();
			JobInfoOption job = characters [r.character];

			// this only happens for de-registering jobs
			// basically, if we get a command to deregister from worksite A, 
			// but the map already has worksite B registered, we don't need to do anything
			// if thats not the case, then in the meantime we can set the job to empty
			if (job.jobInfo.HasValue && r.currentWorksite.HasValue) {
				if (job.jobInfo.Value.id.Id != r.currentWorksite.Value.Id)
					return new Nothing ();
			}

			// set the job to the new one
			characters [r.character] = new JobInfoOption (r.job);
			districtWriter.Send (new District.Update ()
				.SetCharacterMap(characters)
			);
			return new Nothing ();
		}

		public Option<EntityId> GetRandomConstructionSite() {
			if (constructionList.Count > 0)
				return constructionList[Random.Range(0,constructionList.Count)];
			else
				return new Option<EntityId> ();
		}

		public void Cede(EntityId playerEntity, int playerId) {

			// set the characters to no district
			foreach (EntityId id in characters.Keys) {
				SpatialOS.Commands.SendCommand (districtWriter, Character.Commands.SetDistrict.Descriptor, new SetCharacterDistrictRequest(new Option<EntityId>()), id);
			}
			characters.Clear ();
			districtWriter.Send (new District.Update ()
				.SetCharacterMap (characters)
				.AddShowCede(new Nothing())
			);


			// tell our owner to unregister the district
			SpatialOS.Commands.SendCommand (districtWriter, PlayerOnline.Commands.DeregisterDistrict.Descriptor, new DistrictDeregisterRequest(gameObject.EntityId()), owned.getOwnerObject());

			// tell the buildings to flip their owned components over to new player
			foreach (EntityId id in positionMap.Keys) {
				SpatialOS.Commands.SendCommand (districtWriter, Owned.Commands.SetOwner.Descriptor, new OwnRequest(playerId, playerEntity), id);
			}

			// tell our new owner to register the district
			SpatialOS.Commands.SendCommand (districtWriter, PlayerOnline.Commands.RegisterDistrict.Descriptor, new DistrictRegisterRequest(gameObject.EntityId()), playerEntity);

		}

	}

}