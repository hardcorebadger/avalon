using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class DistrictController : MonoBehaviour {
		
		[Require] private District.Writer districtWriter;
		[Require] private Building.Writer buildingWriter;

		public GameObject spawn;

		Map<EntityId, Vector3d> positionMap;
		Map<int, BuildingList> storageMap;
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
			districtWriter.CommandReceiver.OnFindAnyItem.RegisterResponse (OnFindAnyItem);
			districtWriter.CommandReceiver.OnRegisterCharacter.RegisterResponse (OnRegisterCharacter);
			districtWriter.CommandReceiver.OnDeregisterCharacter.RegisterResponse (OnDeregisterCharacter);

			positionMap = districtWriter.Data.positionMap;
			storageMap = districtWriter.Data.storageMap;
			characters = districtWriter.Data.characterList;
			beds = districtWriter.Data.beds;
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
			districtWriter.CommandReceiver.OnFindAnyItem.DeregisterResponse ();

			districtWriter.CommandReceiver.OnRegisterCharacter.DeregisterResponse ();
			districtWriter.CommandReceiver.OnDeregisterCharacter.DeregisterResponse ();


		}

		void Update() {

			if (districtWriter.HasAuthority) {

				spawnTimer += Time.deltaTime;

				if (spawnTimer < 2F) {
					spawnTimer = 2f;
					//spawn (so weird so that it spawns to begin with isntead of waiting for debug)

					if (characters.Count < beds) {
						SpatialOS.Commands.CreateEntity(districtWriter, Gamelogic.EntityTemplates.EntityTemplateFactory.CreateCharacterTemplate(new Vector3d(0,0,0), owned.getOwner(), owned.getOwnerObject()));
					}

				}

				if (spawnTimer >= 30f)
					spawnTimer = -1f;


			}

		}

		private Nothing OnRegisterBuilding(BuildingRegistrationRequest r, ICommandCallerInfo _) {
			positionMap.Add (r.buildingId, r.position);
			beds += r.beds;
			districtWriter.Send (new District.Update ()
				.SetPositionMap(positionMap)
				.SetBeds(beds)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterBuilding(BuildingDeregistrationRequest r, ICommandCallerInfo _) {
			positionMap.Remove (r.buildingId);
			beds -= r.beds;
			districtWriter.Send (new District.Update ()
				.SetPositionMap(positionMap)
				.SetBeds(beds)
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
			Debug.LogWarning ("A");
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


	}

}