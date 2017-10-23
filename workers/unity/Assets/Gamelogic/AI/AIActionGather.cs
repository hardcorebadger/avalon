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

	public class AIActionGather : AIAction {

		// description //
		// agent gathers a gatherable

		// assumptions //
		// empty hand (401)
		// agent is at the gatherable

		// end condition //
		// agent has gathered object, has yield in his hand, at place of gather

		// reponse codes //
		// 401 = owner didn't have room in hand to hold gathered item
		// 501 = gather request failed
		// 502 = gatherable refused gather for some reason

		private EntityId target;
		private Vector3 position;
		private AIAction subAction;
		private GatherResponse response;
		
		public AIActionGather(CharacterController o, EntityId t) : base(o) {
			target = t;
		}

		public override int Update() {

			if (shouldRespond != 100) {
				return shouldRespond;
			}
			
			switch (state) {
			case 0:
				if (!agent.EmptyHanded ())
					return 401;
				state++;
				break;
			case 1:
				// already there, gather it
				if (subAction == null)
					subAction = new AIActionCharacterState (agent, 3, CharacterState.CHOPPING);
				if (AIAction.OnSuccess(subAction.Update()))
					state++;
				break;
			case 2:
				//successfully chopped, send gather request
				SpatialOS.Commands.SendCommand (agent.characterWriter, Gatherable.Commands.RequestGather.Descriptor, new GatherRequest (), target)
					.OnSuccess (response => OnGatherResponse (response))
					.OnFailure (response => OnGatherFailed ());
				state++;
				break;
			case 3:
				// waiting on gather request
				break;
			case 4:
				//we got the gather response
				if (response.success) {
					agent.SetInHandItem (response.items.id);
					return 200;
				} else
					return 502;
				break;
			}

			return 100;
		}

		private void OnGatherResponse(GatherResponse r) {
			response = r;
			state++;
		}

		private void OnGatherFailed() {
			shouldRespond = 501;
		}
	}

}