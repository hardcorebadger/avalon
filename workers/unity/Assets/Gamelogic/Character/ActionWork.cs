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

	public class ActionWork : Action {

		private int state = 0;
		private EntityId target;
		private bool failed = false;
		private Action subAction = null;
		
		public ActionWork(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				// ask to work for the entity
				SpatialOS.Commands.SendCommand (owner.characterWriter, WorkSite.Commands.Enlist.Descriptor, new EnlistRequest (), target)
					.OnSuccess (response => OnEnlistResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 1;
				break;
			case 1:
				// waiting to enlist
				break;
			case 2:
				// enlisted, executing subaction, terminate on termination
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Success)
					return ActionCode.Success;
				else if (c == ActionCode.Failure)
					return ActionCode.Failure;
				break;
			}

			if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}

		public override void OnKill() {
			SpatialOS.Commands.SendCommand (owner.characterWriter, WorkSite.Commands.UnEnlist.Descriptor, new UnEnlistRequest (), target);
		}

		private void OnEnlistResult(EnlistResponse response) {
			switch (response.workType) {
			case WorkType.WORK_BUILDING:
				subAction = new ActionConstruction (owner, target);
				break;
			default:
				break;
			}
			state = 2;
		}

		private void OnRequestFailed() {
			failed = true;
		}

	}

}