using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;

namespace Assets.Gamelogic.Core {

	public class AIActionWait : AIAction {

		private float time = 0f;
		private float duration;

		public AIActionWait(CharacterController o, float d) : base(o,"wait") {
			duration = d;
		}

		public override int Update(){
			switch (state) {
			case 0:
				time += Time.deltaTime;
				if (time > duration) {
					return 200;
				}
				break;
			}
			return 100;
		}
	}

}