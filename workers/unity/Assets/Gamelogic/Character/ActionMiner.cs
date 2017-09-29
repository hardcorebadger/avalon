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

	public class ActionMiner : Action {

		private int state = 0;
		private EntityId target;
		private bool failed = false;
		private bool success = false;
		private Action subAction = null;
		private Vector3 hqPosition;

		public ActionMiner(CharacterController o, EntityId t, Vector3 pos) : base(o)	{
			target = t;
			hqPosition = pos;
		}


		// Use this for initialization
		public override ActionCode Update () {
			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}
		

	}

}