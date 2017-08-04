using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class ActionDistributedGather : ActionDistributed {
		public ActionDistributedGather(CharacterController o, int gId, int size, EntityId[] t) : base(o, gId, size, t)	{}

		protected override Action CreateSubAction (CharacterController o, EntityId t) {
			return new ActionGather (o, t);
		}
	}

}