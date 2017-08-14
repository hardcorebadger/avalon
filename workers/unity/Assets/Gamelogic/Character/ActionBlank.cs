using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ActionBlank : Action {

		private int state = 0;
		private bool failed = false;
		private bool success = false;

		public ActionBlank(CharacterController o) : base(o)	{}

		public override ActionCode Update () {
			switch (state) {
			default:
				break;
			}

			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}
	}

}
