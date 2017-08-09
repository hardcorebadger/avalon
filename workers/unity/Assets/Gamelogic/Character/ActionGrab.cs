using System.Collections;
using System.Collections.Generic;
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
	
	public class ActionGrab : Action {

		private Dictionary<int,int> toGrab;
		public EntityId target;
		private int stage = 0;

		public ActionGrab(CharacterController o, EntityId t, Dictionary<int,int> g) : base(o)	{
			target = t;
			toGrab = g;
		}

		public override ActionCode Update () {
			if (stage == 0) {

			}
			return ActionCode.Working;
		}

	}

}