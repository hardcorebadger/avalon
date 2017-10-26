﻿using Improbable.Collections;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {
	
	public class AIActionJob : AIAction {

		public EntityId workSite;
		public Vector3 workSitePosition;
		public Option<EntityId> district;

		public AIActionJob(CharacterController o, EntityId w, Vector3 p, Option<EntityId> d) : base(o) {
			workSite = w;
			workSitePosition = p;
			district = d;
		}

		public override int Update(){
			switch (state) {
			case 0:
				break;
			}
			return 100;
		}

		// task response codes
		// 100 = keep working (requeue)
		// 200 = quit, it's all good
		// 400 = quit, it is your fault
		// 500 = quit, it's work site's fault
	}

}