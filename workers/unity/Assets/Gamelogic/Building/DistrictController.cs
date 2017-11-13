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

		public GameObject spawn;

		Map<EntityId, Vector3d> positionMap;
		Map<int, BuildingList> storageMap;
		Map<int, BuildingList> storageAvailabilityMap;
		List<EntityId> constructionList;
		List<EntityId> characters;

		int beds;
		float spawnTimer = -1f;
		public BuildingController building;
		public OwnedController owned;

		void OnEnable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.RegisterResponse (OnRegisterBuilding);
			districtWriter.CommandReceiver.OnDeregisterBuilding.RegisterResponse (OnDeregisterBuilding);
			districtWriter.CommandReceiver.OnStorageUpdateHas.RegisterResponse (OnStorageUpdateHas);
			districtWriter.CommandReceiver.OnStorageUpdateOut.RegisterResponse (OnStorageUpdateOut);
			districtWriter.CommandReceiver.OnStorageUpdateAccepting.RegisterResponse (OnStorageUpdateAccepting);
			districtWriter.CommandReceiver.OnStorageUpdateNotAccepting.RegisterResponse (OnStorageUpdateNotAccepting);
			districtWriter.CommandReceiver.OnFindAnyItem.RegisterResponse (OnFindAnyItem);
			districtWriter.CommandReceiver.OnFindConstructionSite.RegisterResponse (OnFindConstructionSite);
			districtWriter.CommandReceiver.OnRegisterCharacter.RegisterResponse (OnRegisterCharacter);
			districtWriter.CommandReceiver.OnDeregisterCharacter.RegisterResponse (OnDeregisterCharacter);

			positionMap = districtWriter.Data.positionMap;
			storageMap = districtWriter.Data.storageMap;
			storageAvailabilityMap = districtWriter.Data.storageAvailabilityMap;
			characters = districtWriter.Data.characterList;
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
		}

		void OnDisable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.DeregisterResponse ();
			districtWriter.CommandReceiver.OnDeregisterBuilding.DeregisterResponse ();
			districtWriter.CommandReceiver.OnStorageUpdateHas.DeregisterResponse ();
			districtWriter.CommandReceiver.OnStorageUpdateOut.DeregisterResponse ();
			districtWriter.CommandReceiver.OnStorageUpdateAccepting.DeregisterResponse ();
			districtWriter.CommandReceiver.OnStorageUpdateNotAccepting.DeregisterResponse ();
			districtWriter.CommandReceiver.OnFindAnyItem.DeregisterResponse ();
			districtWriter.CommandReceiver.OnFindConstructionSite.DeregisterResponse ();
			districtWriter.CommandReceiver.OnRegisterCharacter.DeregisterResponse ();
			districtWriter.CommandReceiver.OnDeregisterCharacter.DeregisterResponse ();


		}

		void Update() {

			if (districtWriter.HasAuthority) {

				spawnTimer += Time.deltaTime;

				if (spawnTimer >= 60f) {
					if (characters.Count < beds) {
						SpatialOS.Commands.ReserveEntityId (districtWriter)
							.OnSuccess (result => SpawnCharacterEntity (result.ReservedEntityId));
					}
					spawnTimer = 0F;
				}


			}

		}

		public void SpawnCharacterEntity(EntityId entityId) {

			SpatialOS.Commands
				.CreateEntity(districtWriter, entityId, Gamelogic.EntityTemplates.EntityTemplateFactory.CreateCharacterTemplate(building.door.position, owned.getOwner(), owned.getOwnerObject(), new Option<EntityId>(districtWriter.EntityId)))
					.OnSuccess(result => RegisterSpawnedCharacter(entityId));

		}

		public void RegisterSpawnedCharacter(EntityId characterId) {

			SpatialOS.Commands.SendCommand (districtWriter, PlayerOnline.Commands.RegisterCharacter.Descriptor, new CharacterPlayerRegisterRequest (characterId), owned.getOwnerObject ());

			characters.Add(characterId);
			districtWriter.Send (new District.Update ()
				.SetCharacterList(characters)
			);
		}

		private Nothing OnRegisterBuilding(BuildingRegistrationRequest r, ICommandCallerInfo _) {
			positionMap.Add (r.buildingId, r.position);
			AddBuildingToStorageMaps (r.buildingId, r.acceptingItems);
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
			RemoveBuildingFromStorageMaps (r.buildingId);
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

		private Nothing OnStorageUpdateHas(StorageUpdateRequest r, ICommandCallerInfo _) {
			BuildingList l;
			if (!storageMap.TryGetValue (r.item, out l))
				l = new BuildingList(new List<EntityId>());
			l.list.Add (r.building);
			storageMap [r.item] = l;
			districtWriter.Send (new District.Update ()
				.SetStorageMap (storageMap)
			);
			return new Nothing ();
		}

		private Nothing OnStorageUpdateOut(StorageUpdateRequest r, ICommandCallerInfo _) {
			BuildingList l;
			// shouldnt happen
			if (!storageMap.TryGetValue (r.item, out l))
				return new Nothing ();
			l.list.Remove (r.building);
			if (l.list.Count < 1)
				storageMap.Remove (r.item);
			else
				storageMap [r.item] = l;
			districtWriter.Send (new District.Update ()
				.SetStorageMap (storageMap)
			);
			return new Nothing ();
		}

		private Nothing OnStorageUpdateAccepting(StorageUpdateRequest r, ICommandCallerInfo _) {
			BuildingList l;
			if (!storageAvailabilityMap.TryGetValue (r.item, out l))
				l = new BuildingList(new List<EntityId>());
			l.list.Add (r.building);
			storageAvailabilityMap [r.item] = l;
			districtWriter.Send (new District.Update ()
				.SetStorageAvailabilityMap (storageAvailabilityMap)
			);
			return new Nothing ();
		}

		private Nothing OnStorageUpdateNotAccepting(StorageUpdateRequest r, ICommandCallerInfo _) {
			BuildingList l;
			// shouldnt happen
			if (!storageAvailabilityMap.TryGetValue (r.item, out l))
				return new Nothing ();
			l.list.Remove (r.building);
			if (l.list.Count < 1)
				storageAvailabilityMap.Remove (r.item);
			else
				storageAvailabilityMap [r.item] = l;
			districtWriter.Send (new District.Update ()
				.SetStorageAvailabilityMap (storageAvailabilityMap)
			);
			return new Nothing ();
		}

		private ItemFindResponse OnFindAnyItem(ItemFindRequest r, ICommandCallerInfo _) {
			foreach (int id in r.ids) {
				if (!storageMap.ContainsKey (id) || storageMap[id].list.Count < 1)
					continue;
				
				foreach (EntityId entity in storageMap [id].list)  {
					if (!r.asker.HasValue || entity.Id != r.asker.Value.Id) {
						Vector3d v = positionMap [entity];
						return new ItemFindResponse (id, new Option<EntityId> (entity), v);
					}
				}
			}
			// nope.
			return new ItemFindResponse (-1, new Option<EntityId> (), Vector3d.ZERO);
		}

		private Nothing OnRegisterCharacter(CharacterRegistrationRequest r, ICommandCallerInfo _) {
			foreach (var e in r.characters) {
				characters.Add(e);
			}
			districtWriter.Send (new District.Update ()
				.SetCharacterList(characters)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterCharacter(CharacterDeregistrationRequest r, ICommandCallerInfo _) {
			foreach (var e in r.characters) {
				characters.Remove(e);
			}
			districtWriter.Send (new District.Update ()
				.SetCharacterList(characters)
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

		private void AddBuildingToStorageMaps(EntityId id, List<int> accepting) {
			foreach (int i in accepting) {
				BuildingList l;
				if (!storageAvailabilityMap.TryGetValue (i, out l))
					l = new BuildingList(new List<EntityId>());
				l.list.Add (id);
				storageAvailabilityMap [i] = l;
			}
			districtWriter.Send (new District.Update ()
				.SetStorageAvailabilityMap (storageAvailabilityMap)
			);
		}

		private void RemoveBuildingFromStorageMaps(EntityId id) {
			foreach (int item in storageMap.Keys) {
				BuildingList l;
				// shouldnt happen
				if (!storageMap.TryGetValue (item, out l))
					continue;
				l.list.Remove (id);
				if (l.list.Count < 1)
					storageMap.Remove (item);
				else
					storageMap [item] = l;
			}
			foreach (int item in storageAvailabilityMap.Keys) {
				BuildingList l;
				// shouldnt happen
				if (!storageAvailabilityMap.TryGetValue (item, out l))
					continue;
				l.list.Remove (id);
				if (l.list.Count < 1)
					storageAvailabilityMap.Remove (item);
				else
					storageAvailabilityMap [item] = l;
			}
			districtWriter.Send (new District.Update ()
				.SetStorageMap (storageMap)
				.SetStorageAvailabilityMap (storageAvailabilityMap)
			);
		}

		public Option<EntityId> GetRandomConstructionSite() {
			if (constructionList.Count > 0)
				return constructionList[Random.Range(0,constructionList.Count)];
			else
				return new Option<EntityId> ();
		}

		public void Cede(EntityId playerEntity, int playerId) {

			// set the characters to no district
			foreach (EntityId id in characters) {
				SpatialOS.Commands.SendCommand (districtWriter, Character.Commands.SetDistrict.Descriptor, new SetCharacterDistrictRequest(new Option<EntityId>()), id);
			}
			characters.Clear ();
			districtWriter.Send (new District.Update ()
				.SetCharacterList (characters)
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