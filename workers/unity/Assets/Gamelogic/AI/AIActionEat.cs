using System.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Collections;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class AIActionEat : AIAction {


		// description //
		// agent finds food in district and tries to eat it

		// assumptions //
		// agent is empty handed

		// end condition //
		// agent's hunger has increased

		// reponse codes //
		// 401 = agent could not find food

		public AIActionGetItem getItem;
		public AIActionWait wait;

		public AIActionEat(CharacterController o) : base(o) {


		}

		public override int Update() {
			if (shouldRespond != 100) {
				agent.CancelEat ();
				Debug.LogWarning ("EAT CANCELLED: " + shouldRespond);
				return shouldRespond;
			}

			switch (state) {
			case 0:
				if (getItem == null) {
					List<int> i = new List<int> ();
					i.Add (2);
					getItem = new AIActionGetItem (agent, i, agent.district);
				}
				int responseCode = getItem.Update ();
				if (AIAction.OnSuccess (responseCode)) {
					// has food item!
					getItem = null;
					state++;
				} else if (AIAction.OnUserError (responseCode)) {

					shouldRespond = 401;
				} else if (AIAction.OnServerError (responseCode)) {

					shouldRespond = 401;
				}
				break;
			case 1:
				//has food
				if (wait == null)
					wait = new AIActionWait (agent, 2f);
				if (AIAction.OnSuccess (wait.Update ())) {
					wait = null;
					state++;
				}
				break;
			case 2: 
				//eat food. 
				agent.DropItem ();
				agent.Eat (30f);
				//tells agent it is not eating anymore
				agent.CancelEat ();
				return 200;
				break;
			}

			return 100;
		}

	}

}