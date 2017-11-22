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

	public class AIActionAttack : AIAction {

		// description //
		// agent attacks other entity until death

		// assumptions //
		// empty hand (just aesthetically)
		// other entity exists (on the same worker for now) (401)

		// end condition //
		// agent has killed the other entity

		// reponse codes //
		// 401 = entity doesnt exist

		private GameObject targetObject;
		private EntityId targetId;
		private AIActionGoTo seek;
		private AIActionWait wait;
		private float waitDuration = 3f;

		public AIActionAttack(CharacterController o, EntityId i) : base(o,"attack") {
			targetId = i;
		}

		public override int Update() {
			return UpdateLocalEntity ();
		}

		private int UpdateLocalEntity(){
			if (state != 0 && targetObject == null) {
				return 401;
			}
			switch (state) {
			case 0:
				if (LocalEntities.Instance.ContainsEntity (targetId)) {
					IEntityObject g = LocalEntities.Instance.Get (targetId);
					targetObject = g.UnderlyingGameObject;
					state++;
				} else {
					return 401;
				}
				break;
			case 1:
				// wait while seeking
				if (seek == null)
					seek = new AIActionGoTo (agent, targetId, targetObject.transform.position);
				seek.target = targetObject.transform.position;
				if (AIAction.OnSuccess (seek.Update ()))
					state++;
				break;
			case 2:
				// attack
				if (wait == null)
					wait = new AIActionWait (agent, waitDuration);
				seek.target = targetObject.transform.position;
				seek.Update ();
				if (AIAction.OnSuccess (wait.Update ())) {
					wait = null;
					state++;
				}
				break;
			case 3:
				// deal the hit
				seek.target = targetObject.transform.position;
				seek.Update ();
				agent.Hit (targetId);
				state++;
				break;
			case 4: 
				// wait for on deal hit
				seek.Update ();
				break;
			case 5:
				
				// loop and change wait time
				state = 2;
				SpatialOS.Commands.SendCommand (agent.characterWriter, Character.Commands.ReceiveHit.Descriptor, new ReceiveHitRequest (agent.characterWriter.EntityId, agent.characterWriter.Data.playerId), targetId);
				waitDuration = Random.Range (1.0f, 2.0f);
				break;
			}
			return 100;
		}

		private int UpdateNonLocalEntity(){
			return 100;
		}

		public override void OnDealHit () {
			base.OnDealHit ();
			state++;
		}
	}

}