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

		Map<EntityId, Vector3d> positionMap;
		Map<int, BuildingList> storageMap;

		void OnEnable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.RegisterResponse (OnRegisterBuilding);
			districtWriter.CommandReceiver.OnDeregisterBuilding.RegisterResponse (OnDeregisterBuilding);
			districtWriter.CommandReceiver.OnStorageUpdateHas.RegisterResponse (OnStorageUpdateHas);
			districtWriter.CommandReceiver.OnStorageUpdateOut.RegisterResponse (OnStorageUpdateOut);
			districtWriter.CommandReceiver.OnFindAnyItem.RegisterResponse (OnFindAnyItem);
			positionMap = districtWriter.Data.positionMap;
			storageMap = districtWriter.Data.storageMap;
		}

		void OnDisable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.DeregisterResponse ();
			districtWriter.CommandReceiver.OnDeregisterBuilding.DeregisterResponse ();
			districtWriter.CommandReceiver.OnStorageUpdateHas.DeregisterResponse ();
			districtWriter.CommandReceiver.OnStorageUpdateOut.DeregisterResponse ();
			districtWriter.CommandReceiver.OnFindAnyItem.DeregisterResponse ();
		}

		private Nothing OnRegisterBuilding(BuildingRegistrationRequest r, ICommandCallerInfo _) {
			positionMap.Add (r.buildingId, r.position);
			districtWriter.Send (new District.Update ()
				.SetPositionMap(positionMap)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterBuilding(BuildingDeregistrationRequest r, ICommandCallerInfo _) {
			positionMap.Remove (r.buildingId);
			districtWriter.Send (new District.Update ()
				.SetPositionMap(positionMap)
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

	}

}