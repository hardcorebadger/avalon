using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class ActionGatherRadial : Action {

		private EntityId[] ids;
		private ActionGather subGather;

		private int index = 0;

		public ActionGatherRadial(CharacterController o, EntityId[] i) : base(o) {
			ids = i;
		}

		public override ActionCode Update () {
			if (ids.Length == 0)
				return ActionCode.Success;
			
			if (subGather == null)
				subGather = new ActionGather (owner, ids[index]);
			
			ActionCode result = subGather.Update ();

			if (result != ActionCode.Working) {
				index++;
				if (ids.Length <= index)
					return ActionCode.Success;
				else
					subGather = new ActionGather (owner, ids[index]);
			}

			return ActionCode.Working;

		}
	}

}
