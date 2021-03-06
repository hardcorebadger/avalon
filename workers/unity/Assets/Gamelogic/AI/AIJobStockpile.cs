﻿using UnityEngine;
using Improbable.Collections;
using Improbable.Core;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class AIJobStockpile : AIActionJob {

		public AIJobStockpile(CharacterController o, EntityId w, Vector3 p, EntityId d) : base(o, w, p, d, new Option<Vector3>(),"stockpile") {}

		public override int Update(){
			switch (state) {
			case 0:
				break;
			}
			return 100;
		}
	}

}