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
			if (buildingWriter.Data.town.HasValue) {
				SpatialOS.Commands.SendCommand (buildingWriter, TownCenter.Commands.AddBuilding.Descriptor, new TownAddRequest (gameObject.EntityId()), buildingWriter.Data.town.Value);
			}
		}

		private void OnDisable() {
			if (buildingWriter.Data.town.HasValue) {
				SpatialOS.Commands.SendCommand (buildingWriter, TownCenter.Commands.RemoveBuilding.Descriptor, new TownRemoveRequest (gameObject.EntityId()), buildingWriter.Data.town.Value);
			}
		}

		public Option<EntityId> GetTown() {
			return buildingWriter.Data.town;
		}

	}

}