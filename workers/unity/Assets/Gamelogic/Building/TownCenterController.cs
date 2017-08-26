using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class TownCenterController : MonoBehaviour {

		[Require] private TownCenter.Writer townCenterWriter;

		private void OnEnable() {
			townCenterWriter.CommandReceiver.OnAddCitizen.RegisterResponse (OnAddCitizen);
			townCenterWriter.CommandReceiver.OnAddBuilding.RegisterResponse (OnAddBuilding);
			townCenterWriter.CommandReceiver.OnRemoveCitizen.RegisterResponse (OnRemoveCitizen);
			townCenterWriter.CommandReceiver.OnRemoveBuilding.RegisterResponse (OnRemoveBuilding);
		}

		private void OnDisable() {
			townCenterWriter.CommandReceiver.OnAddCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnAddBuilding.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnRemoveCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnRemoveBuilding.DeregisterResponse ();
		}

		private TownAddResponse OnAddCitizen(TownAddRequest request, ICommandCallerInfo callerinfo) {
			List<EntityId> newList = townCenterWriter.Data.citizens;
			newList.Add (request.entity);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetCitizens (newList)
			);
			return new TownAddResponse ();
		}

		private TownAddResponse OnAddBuilding(TownAddRequest request, ICommandCallerInfo callerinfo) {
			List<EntityId> newList = townCenterWriter.Data.buildings;
			newList.Add (request.entity);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetBuildings (newList)
			);
			return new TownAddResponse ();
		}

		private TownRemoveResponse OnRemoveCitizen(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			List<EntityId> newList = townCenterWriter.Data.citizens;
			newList.Remove (request.entity);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetCitizens (newList)
			);
			return new TownRemoveResponse ();
		}

		private TownRemoveResponse OnRemoveBuilding(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			List<EntityId> newList = townCenterWriter.Data.buildings;
			newList.Remove (request.entity);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetBuildings (newList)
			);
			return new TownRemoveResponse ();
		}

	}

}
