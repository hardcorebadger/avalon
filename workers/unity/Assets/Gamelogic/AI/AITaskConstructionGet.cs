using Improbable.Collections;
using UnityEngine;
using Improbable.Core;

namespace Assets.Gamelogic.Core {

	public class AITaskConstructionGet : AIAction {

		// 203 = assignment didn't request an item
		// forwards GetItem response codes
		
		private ConstructionJobAssignment assignment;
		private AIActionGetItem getAction;
		private AITaskConstruction task;
		private AIActionGoTo seek;
		private List<int> toGet;
		private int getResult;

		public AITaskConstructionGet(CharacterController o, ConstructionJobAssignment a, AITaskConstruction c) : base(o,"constructiongettask") {
			assignment = a;
			task = c;
			toGet = new List<int> ();
		}

		public override int Update(){
			switch (state) {
			case 0:
				//set up
				if (!assignment.toGet.HasValue)
					return 203;
				toGet.Add (assignment.toGet.Value);
				state++;
				break;
			case 1:
				// get item
				if (getAction == null)
					getAction = new AIActionGetItem (agent, toGet, task.district);
				getResult = getAction.Update ();
				if (AIAction.OnTermination (getResult))
					state++;
				break;
			case 2:
				// walk back, return get response
				if (seek == null)
					seek = new AIActionGoTo (agent, task.constructionSite);
				if (AIAction.OnSuccess (seek.Update ()))
					return getResult;
				break;
			}
			return 100;
		}
	}

}