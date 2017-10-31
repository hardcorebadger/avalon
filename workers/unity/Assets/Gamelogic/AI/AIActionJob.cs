using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {
	
	public class AIActionJob : AIAction {

		public EntityId workSite;
		public Vector3 workSitePosition;
		public Option<EntityId> district;

		public AIActionJob(CharacterController o, EntityId w, Vector3 p, Option<EntityId> d, string n) : base(o,n) {
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