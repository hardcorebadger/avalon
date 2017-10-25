using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {
	
	public class AIJobConstruction : AIActionJob {

		public AIJobConstruction(CharacterController o) : base(o) {
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