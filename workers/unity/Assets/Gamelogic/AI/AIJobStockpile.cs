using UnityEngine;
using Improbable.Collections;
using Improbable.Core;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class AIJobStockpile : AIActionJob {

		public AIJobStockpile(CharacterController o, EntityId w, Vector3 p, Option<EntityId> d) : base(o, w, p, d) {
		}

		public override int Update(){
			switch (state) {
			case 0:
				break;
			}
			return 100;
		}
	}

}