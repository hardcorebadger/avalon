using System.Collections;
using System.Collections.Generic;
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

		Improbable.Collections.List<EntityId> buildings;

		void OnEnable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.RegisterResponse (OnRegisterBuilding);
			buildings = districtWriter.Data.buildings;
		}

		void OnDisable() {
			districtWriter.CommandReceiver.OnRegisterBuilding.DeregisterResponse ();
		}

		private Nothing OnRegisterBuilding(BuildingRegistrationRequest r, ICommandCallerInfo _) {
			buildings.Add (r.buildingId);
			districtWriter.Send (new District.Update ()
				.SetBuildings(buildings)
			);
			return new Nothing ();
		}

	}

}