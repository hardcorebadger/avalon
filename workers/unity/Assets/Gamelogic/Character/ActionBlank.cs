using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ActionBlank : Action {

		public ActionBlank(CharacterController o) : base(o)	{}

		public override ActionCode Update () {
			return ActionCode.Perpetual;
		}
	}

}
