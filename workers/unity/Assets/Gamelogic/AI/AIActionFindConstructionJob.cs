using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;
using Improbable;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class AIActionFindConstructionJob : AIAction {

		// 401 = no district
		// 402 = no construction
		// 501 = request failed

		private BuildingQueryResponse buildingQueryResponse;
		private EntityId prev;

		public AIActionFindConstructionJob(CharacterController o, EntityId p) : base(o,"findconstructionjob") {
			prev = p;
		}

		public override int Update(){
			if (shouldRespond != 100) {
				return shouldRespond;
			}
			
			switch (state) {
			case 0:
				if (!agent.district.HasValue)
					return 401;
				state++;
				break;
			case 1:
				// request to district for construction sites
				SpatialOS.Commands.SendCommand (agent.characterWriter, District.Commands.FindConstructionSite.Descriptor, new FindConstructionRequest (prev), agent.district.Value)
					.OnSuccess (response => OnFindResponse (response))
					.OnFailure (response => OnRequestFailed ());
				state++;
				break;
			case 2:
				// wait
				break;
			case 3:
				// if you got one, queue an action work for it
				if (!buildingQueryResponse.building.HasValue)
					return 402;
				agent.QueueAction (10, new AIActionWork (agent, buildingQueryResponse.building.Value));
				return 200;
			}
			return 100;
		}

		private void OnFindResponse(BuildingQueryResponse r) {
			buildingQueryResponse = r;
			state++;
		}

		private void OnRequestFailed() {
			shouldRespond = 501;
		}
	}

}