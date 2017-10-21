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

	public class ActionWorkInterior : Action {

		private int state = 0;
		private bool failed = false;
		private bool success = false;

		EntityId target;
		Vector3 interiorPositon;
		Vector3 doorPosition;

		public ActionWorkInterior(CharacterController o, EntityId t, Vector3 ip, Vector3 dp) : base(o)	{
			target = t;
			interiorPositon = ip;
			doorPosition = dp;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				owner.transform.position = interiorPositon;
				state++;
				owner.SetIndoors (true);
				break;
			default:
				break;
			}

			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}

		public override void OnKill () {
			owner.transform.position = doorPosition;
			owner.SetIndoors (false);
		}


	}

}