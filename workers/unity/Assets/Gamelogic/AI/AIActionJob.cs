using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {
	
	public class AIActionJob : AIAction {

		public AIActionJob(CharacterController o) : base(o) {
		}

		public override int Update(){
			switch (state) {
			case 0:
				break;
			}
			return 100;
		}
	}

}