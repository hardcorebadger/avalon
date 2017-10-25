using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class AITaskGoAndGather : AIAction {
		
		private EntityId target;
		private AIActionGoTo seek;
		private AIActionGather gather;

		public AITaskGoAndGather(CharacterController o, EntityId e) : base(o) {
			target = e;
		}

		public override int Update(){
			switch (state) {
			case 0:
				if (seek == null)
					seek = new AIActionGoTo (agent, target);
				int s = seek.Update ();
				if (AIAction.OnTermination(s)) {
					if (AIAction.OnSuccess(s))
						state++;
					else 
						return s;
				}
				break;
			case 1:
				if (gather == null)
					gather = new AIActionGather (agent, target);
				int g = gather.Update ();
				if (AIAction.OnTermination (g))
					return g;
				break;
			}
			return 100;
		}
	}

}