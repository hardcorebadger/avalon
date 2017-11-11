using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;

namespace Assets.Gamelogic.Core {

	public class AITaskChopTree : AIAction {

		// description //
		// agent walks to, chops a tree, walks back, and gives the log to the forester

		// assumptions //
		// none

		// end condition //
		// agent is empty handed, and compelted the above

		// reponse codes //
		// 401 = assignment didn't ask to chop
		// 402 = chop had a user error
		// 403 = give had a user error
		// 404 = failed to find entity
		// 501 = chop had a server error
		// 502 = give had a server error

		private AIActionGoTo seek;
		private AIActionGather gather;
		private AIActionGive give;
		private AIJobForester job;
		private ForesterJobAssignment assignment;
		private int chopResult;

		public AITaskChopTree(CharacterController o, ForesterJobAssignment a, AIJobForester j) : base(o,"choptreetask") {
			assignment = a;
			job = j;
		}

		public override int Update(){
			switch (state) {
			case 0:
				// setup
				if (!assignment.chop.HasValue)
					return 401;
				state++;
				break;
			case 1:
				// walk to tree
				if (seek == null)
					seek = new AIActionGoTo (agent, assignment.chop.Value);
				int r = seek.Update ();
				if (AIAction.OnTermination (r)) {
					seek = null;
					if (AIAction.OnSuccess (r))
						state++;
					else
						return 404;
				}
				break;
			case 2:
				if (gather == null)
					gather = new AIActionGather (agent, assignment.chop.Value);
				chopResult = gather.Update ();
				if (AIAction.OnTermination (chopResult))
					state++;
				break;
			case 3:
				// walk back, return get response
				if (seek == null)
					seek = new AIActionGoTo (agent, job.workSite, job.workSitePosition);
				if (AIAction.OnSuccess (seek.Update ()))
					state++;
				break;
			case 4:
				// put log into the inventory
				if (give == null)
					give = new AIActionGive (agent, job.workSite);
				int a = give.Update ();
				if (AIAction.OnTermination (a)) {
					return GetResponse(chopResult, a);
				}
				break;
			}
			return 100;
		}

		private int GetResponse(int chop, int give) {
			if (AIAction.OnServerError (chop))
				return 501;
			if (AIAction.OnServerError (give))
				return 502;
			if (AIAction.OnUserError (chop))
				return 402;
			if (AIAction.OnUserError (give))
				return 403;
			return 200;
		}

		public override void OnKill () {
			if (state == 2)
				gather.OnKill ();
		}
	}

}