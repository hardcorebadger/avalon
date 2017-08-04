using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {

	public abstract class ActionDistributed : Action {

		private int groupId;
		private int groupSize;
		private Queue<EntityId> targets;
		private Action subAction;

		public ActionDistributed(CharacterController o, int gId, int size, EntityId[] overall) : base(o)	{
			groupId = gId;
			groupSize = size;
			targets = Micromanage(overall);
		}

		public override ActionCode Update () {
			if (targets.Count == 0 && subAction == null)
				return ActionCode.Success;

			if (subAction == null) {
				subAction = CreateSubAction (owner, targets.Dequeue ());
			}

			ActionCode result = subAction.Update ();

			if (result == ActionCode.Success)
				subAction = null;

			return ActionCode.Working;
		}

		private Queue<EntityId> Micromanage(EntityId[] overall) {
			Queue<EntityId> parse = new Queue<EntityId> ();
			for (int i = groupId; i < overall.Length; i+=groupSize) {
				parse.Enqueue (overall [i]);
			}
			return parse;
		}

		protected abstract Action CreateSubAction (CharacterController o, EntityId t);

	}

}
