using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Entity.Component;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Entity;

namespace Assets.Gamelogic.Core {

	public class AIActionDamage : AIAction {

		// description //
		// agent attacks building until death

		// assumptions //
		// empty hand (just aesthetically)
		// building exists 

		// end condition //
		// agent has destroyed the building / building is destroyed

		// reponse codes //

		private EntityId targetId;
		private AIActionGoTo seek;
		private AIActionWait wait;
		private float waitDuration = 3f;

		public AIActionDamage(CharacterController o, EntityId i) : base(o) {
			targetId = i;
		}

		public override int Update() {

			switch (state) {

			case 0:
				if (seek == null)
					seek = new AIActionGoTo (agent, targetId);
				if (AIAction.OnSuccess (seek.Update ())) {
					seek = null;
					state++;
				}
				break;
			case 1:
				//attack the building
				if (wait == null)
					wait = new AIActionWait (agent, waitDuration);
				if (AIAction.OnSuccess (wait.Update ())) {
					wait = null;
					state++;
				}
				break;
			case 2: 
				// deal the hit
				agent.Hit (targetId);

				state++;
				break;
			case 3: 
				//wait for hit
				break;
			case 4: 
				SpatialOS.Commands.SendCommand (agent.characterWriter, Building.Commands.ReceiveDamage.Descriptor, new ReceiveDamageRequest(agent.characterWriter.EntityId, agent.characterWriter.Data.playerId), targetId);
				waitDuration = Random.Range (1.0f, 2.0f);
				state = 1;
				break;
			}

			return 100;
		}

		public override void OnDealHit () {
			base.OnDealHit ();
			state++;
		}
	}

}