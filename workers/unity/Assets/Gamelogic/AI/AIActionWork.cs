using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

	public class AIActionWork : AIAction {

		// description //
		// agent enlists and goes to work site

		// assumptions //
		// work site has room

		// end condition //
		// agent is at the worksite and enlisted

		// reponse codes //
		// 401 = work site full
		// 501 = enlist request failed

		private AIActionGoTo seek;
		private EntityId target;
		private EnlistResponse enlistResponse;

		public AIActionWork(CharacterController o, EntityId t) : base(o,"work") {
			target = t;
		}

		public override int Update(){
			
			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				// ask to work for the entity
				SpatialOS.Commands.SendCommand (agent.characterWriter, WorkSite.Commands.Enlist.Descriptor, new EnlistRequest (agent.gameObject.EntityId ()), target)
					.OnSuccess (response => OnEnlistResult (response))
					.OnFailure (response => OnRequestFailed ());
				state++;
				break;
			case 1:
				// waiting
				break;
			case 2:
				// make sure we are good to enlist
				if (enlistResponse.full)
					return 401;
				else {
					agent.SetWorkSite (target);
					state++;
				}
				break;
			case 3:
				if (seek == null)
					seek = new AIActionGoTo (agent, target, enlistResponse.position.ToUnityVector());
				if (AIAction.OnSuccess (seek.Update ()))
					state++;
				break;
			case 4:
				switch (enlistResponse.workType) {
				case WorkType.WORK_BUILDING:
					agent.QueueAction (10, new AIJobConstruction (agent, target, enlistResponse.position.ToUnityVector (), enlistResponse.district));
					break;
				case WorkType.WORK_LOGGING:
					agent.QueueAction (10, new AIJobForester (agent, target, enlistResponse.position.ToUnityVector (), enlistResponse.district));
					break;
				case WorkType.WORK_MINING: 
					agent.QueueAction (10, new AIJobQuarry (agent, target, enlistResponse.position.ToUnityVector (), enlistResponse.district, enlistResponse.interiorPosition.Value.ToUnityVector(), enlistResponse.position.ToUnityVector()));
					break;
				case WorkType.WORK_FARMING: 
					agent.QueueAction (10, new AIJobFarm (agent, target, enlistResponse.position.ToUnityVector (), enlistResponse.district, enlistResponse.interiorPosition.Value.ToUnityVector(), enlistResponse.position.ToUnityVector()));
					break;
				case WorkType.WORK_STORAGE:
					
					break;
				}
				return 200;
			}
			return 100;
		}

		private void OnEnlistResult(EnlistResponse response) {
			enlistResponse = response;
			state++;
		}

		private void OnRequestFailed() {
			shouldRespond = 501;
		}

	}

}