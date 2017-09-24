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

	public class ActionForester : Action {

		private int state = 0;
		private EntityId target;
		private bool failed = false;
		private bool success = false;
		private Action subAction = null;
		private Vector3 hqPosition;

		//do not use itemInHand synchonously, srsly guyz. fix at own risk
		public ActionForester(CharacterController o, EntityId t, Vector3 pos) : base(o)	{
			target = t;
			hqPosition = pos;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				// set up walk to forester
				if (owner.characterWriter.Data.itemInHand == 0) // split to 5 to stash the log in his hand
					state = 5;
				else if (owner.EmptyHanded ()) // gotta go chop a tree
					state = 1;
				else
					failed = true;// gotta handle this better, he has something else in his hand..
				break;
			case 1:
				SpatialOS.Commands.SendCommand (owner.characterWriter, Forester.Commands.GetJob.Descriptor, new Nothing (), target)
					.OnSuccess (response => OnJobResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 2;
				break;
			case 2:
				// waiting for response
				break;
			case 3:
				// got it back, run a gather action
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {		
					subAction = new ActionSeek (owner, target, hqPosition);
					state = 4;
			
				}
				break;
			case 4:
				// walk back to forester
				c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					state = 5;
				}
				break;
			case 5:
				// put the log in the forester
				SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.Give.Descriptor, new ItemStack (owner.characterWriter.Data.itemInHand, 1), target)
					.OnSuccess (response => OnGiveResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 6;
				break;
			case 6:
				// waiting for response
				break;
			}

			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}

		private void OnJobResult (ForesterJobResponse response) {
			if (response.tree.HasValue) {
				subAction = new ActionGather (owner, response.tree.Value);
				state = 3;
			} else {
				Debug.LogWarning ("boss says its quittin time");
				success = true;
			}
		}

		private void OnGiveResult(GiveResponse r) {
			if (r.success) {
				owner.DropItem ();
				state = 1;
			} else {
				// theres no room in the forester
				success = true;
			}
		}

		private void OnRequestFailed () {
			failed = true;
		}

	}

}
