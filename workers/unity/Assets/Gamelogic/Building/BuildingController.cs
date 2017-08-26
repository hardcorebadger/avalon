using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class BuildingController : MonoBehaviour {

		[Require] private Building.Writer buildingWriter;

		private void OnEnable() {
			buildingWriter.CommandReceiver.OnLeaveTown.RegisterResponse(OnLeaveTown);

			if (buildingWriter.Data.town.HasValue) {
				SpatialOS.Commands.SendCommand (buildingWriter, TownCenter.Commands.AddBuilding.Descriptor, new TownAddRequest (gameObject.EntityId()), buildingWriter.Data.town.Value);
			}
		}

		private void OnDisable() {

			buildingWriter.CommandReceiver.OnLeaveTown.DeregisterResponse();


			if (buildingWriter.Data.town.HasValue) {
				SpatialOS.Commands.SendCommand (buildingWriter, TownCenter.Commands.RemoveBuilding.Descriptor, new TownRemoveRequest (gameObject.EntityId()), buildingWriter.Data.town.Value);
			}
		}

		public Option<EntityId> GetTown() {
			return buildingWriter.Data.town;
		}

		private Nothing OnLeaveTown(Nothing request, ICommandCallerInfo callerinfo) {
			buildingWriter.Send (new Building.Update ()
				.SetTown (new Option<EntityId> ())
			);
			return request;
		}

		public void SetTown(EntityId i) {
			if (buildingWriter.Data.town.HasValue) {
				SpatialOS.Commands.SendCommand (buildingWriter, TownCenter.Commands.TentativeRemoveCitizen.Descriptor, new TownRemoveRequest (gameObject.EntityId ()), buildingWriter.Data.town.Value);
			}
			buildingWriter.Send (new Building.Update ()
				.SetTown (i)
			);
		}

	}

}