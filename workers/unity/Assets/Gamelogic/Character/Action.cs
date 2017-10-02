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

		public virtual void OnKill () {}

		public virtual void OnDealHit() {}

		public virtual void Log() {}

	}

	public enum ActionCode {
		Success,
		Failure,
		Perpetual,
		Working,
	}

}