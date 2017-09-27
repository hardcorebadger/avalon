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
		private int branch = 0;
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
			switch (branch) {
			case 0:
				GetJob ();
				break;
			case 1:
				Chop ();
				break;
			case 2:
				Plant ();
				break;
			}
			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}

		// GET JOB

		// assumes empty hand and at work site
		private void GetJob() {
			switch (state) {
			case 0:
				// ask for a job
				SpatialOS.Commands.SendCommand (owner.characterWriter, Forester.Commands.GetJob.Descriptor, new Nothing (), target)
					.OnSuccess (response => OnJobResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 1;
				break;
			case 1:
				// waiting for response - see callback
				break;
			}
		}

		private void OnJobResult (ForesterJobResponse response) {
			// either we get a tree entity id or if not we plant a tree
			if (response.tree.HasValue) {
				subAction = new ActionGather (owner, response.tree.Value);
				SetBranch (1);
			} else {
				subAction = new ActionSeek (owner, GetRandomTreePosition ());
				SetBranch (2);
			}
		}

		// CHOP

		// assumes subAction is set to gather a tree, 
		// gathers tree, returns to HQ, and places it in storage
		private void Chop() {
			switch (state) {
			case 0:
				// run gather action, walk back when done
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {		
					subAction = new ActionSeek (owner, target, hqPosition);
					state = 1;
				}
				break;
			case 1:
				// walk back. put log in there when you arrive
				c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					// basically, something went wrong, so lets just restart since we're back we can
					if (owner.characterWriter.Data.itemInHand != 0)
						SetBranch (0);
					else {
						SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.Give.Descriptor, new ItemStack (owner.characterWriter.Data.itemInHand, 1), target)
							.OnSuccess (response => OnGiveResult (response))
							.OnFailure (response => OnRequestFailed ());
						state = 2;
					}
				}
				break;
			case 2:
				// waiting for response - see callback
				break;
			}
		}

		private void OnGiveResult(GiveResponse r) {
			if (r.success) {
				owner.DropItem ();
				SetBranch (0);
			} else {
				// theres no room in the forester
				Debug.Log("ish: theres no room in the forester - quitting");
				success = true;
			}
		}

		// PLANT

		private void Plant() {
			Debug.LogWarning("ish: no action for planting, quitting");
			success = true;
		}

		// HELPERS

		private void SetBranch(int i) {
			state = 0;
			branch = i;
		}

		private Vector3 GetRandomTreePosition() {
			return Vector3.zero;
		}

		private void OnRequestFailed () {
			failed = true;
		}

	}

}
