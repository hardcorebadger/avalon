using Improbable.Collections;
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

namespace Assets.Gamelogic.Core {

	public class AIJobTower : AIActionJob {

		// description //
		// agent goes inside from door and on timer farms grain

		// assumptions //
		// agent is at the building door

		// end condition //
		// agent has created one grain, still inside

		// reponse codes //
		// 502 = farm refused to create grain for some reason (possibly full)


		private AIActionWait wait;
		Vector3 interiorPositon;

		public AIJobTower(CharacterController o, EntityId w, Vector3 p, EntityId d, Vector3 ip, Vector3 dp) : base(o, w, p, d, dp ,"tower") {
			interiorPositon = ip;
		}

		public override int Update(){
			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:

				if (!agent.indoors) {
					agent.transform.position = interiorPositon;
					agent.SetIndoors (true, doorPosition);
				}
				state++;

				break;
			case 1:
				//Sit There
				if (wait == null)
					wait = new AIActionWait (agent, 60f);
				if (AIAction.OnSuccess (wait.Update ())) {
					wait = null;
					state++;
				}
				break;
			case 2:
				// requeue this job
				agent.QueueAction (10, new AIJobTower (agent, workSite, workSitePosition, district, interiorPositon, doorPosition.Value));
				// terminate
				return 200;
			}
			return 100;
		}

		public override void OnKill () {
			agent.SetIndoors (false, doorPosition);
			agent.QueueAction (10, new AIJobTower (agent, workSite, workSitePosition, district, interiorPositon, doorPosition.Value));
		}

	}

}