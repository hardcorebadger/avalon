using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public abstract class Action {

		public CharacterController owner;

		public Action(CharacterController o) {
			owner = o;
		}

		public abstract ActionCode Update();

	}

	public enum ActionCode {
		Success,
		Failure,
		Perpetual,
		Working,
	}

}