using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;

namespace Assets.Gamelogic.Core {

	public class AIActionCharacterState : AIAction {

		// description //
		// agent performs a character state for some duration

		// assumptions //
		// none

		// end condition //
		// agent is in default character state

		// reponse codes //

		private CharacterState charState;
		private AIActionWait wait;

		public AIActionCharacterState(CharacterController o, float d, CharacterState s) : base(o,"charstate") {
			charState = s;
			wait = new AIActionWait (o, d);
		}

		public override int Update(){
			switch (state) {
			case 0:
				agent.SetState (charState);
				state++;
				break;
			case 1:
				if (AIAction.OnSuccess(wait.Update()))
					state++;
				break;
			case 2:
				agent.SetState (CharacterState.DEFAULT);
				return 200;
			}
			return 100;
		}

		public override void OnKill () {
			base.OnKill ();
			agent.SetState (CharacterState.DEFAULT);
		}
	}

}