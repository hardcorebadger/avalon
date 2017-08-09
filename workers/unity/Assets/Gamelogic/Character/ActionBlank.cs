using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ActionBlank : Action {

		public ActionBlank(CharacterController o) : base(o)	{}

		public override ActionCode Update () {
			owner.SetVelocity (0f);
			return ActionCode.Perpetual;
		}
	}

}
