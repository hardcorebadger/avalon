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

		private int state = -2;
		private EntityId target;
		private bool failed = false;
		private bool success = false;
		private Action subAction = null;
		private Vector3 hqPosition;

		public ActionForester(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override ActionCode Update () {
			switch (state) {
			case -2:
				var entityQuery = Query.HasEntityId(target).ReturnComponents(Position.ComponentId);
				SpatialOS.WorkerCommands.SendQuery(entityQuery)
					.OnSuccess(OnSuccessfulHQQuery)
					.OnFailure(OnFailedEntityQuery);
				break;
			case -1:
				//waiting
				break;
			case 0:
				// set up walk to forester
				subAction = new ActionSeek (owner, target, hqPosition);
				if (owner.characterWriter.Data.itemInHand == 0) // split to 5 to stash the log in his hand
					state = 5;
				else if (owner.EmptyHanded ()) // gotta go chop a tree
					state = 1;
				else
					failed = true;// gotta handle this better, he has something else in his hand..
				break;
			case 1:
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					state = 2;
				}
				break;
			case 2:
				SpatialOS.Commands.SendCommand (owner.characterWriter, Forester.Commands.GetJob.Descriptor, new Nothing (), target)
					.OnSuccess (response => OnJobResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 3;
				break;
			case 3:
				// waiting for response
				break;
			case 4:
				// got it back, run a gather action
				c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					if (owner.characterWriter.Data.itemInHand != 0) // that tree may be gone, just restart if so
						state = 0;
					else {
						subAction = new ActionSeek (owner, target, hqPosition);
						state = 5;
					}
				}
				break;
			case 5:
				// walk back to forester
				c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					state = 6;
				}
				break;
			case 6:
				// put the log in the forester
				SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.Give.Descriptor, new ItemStack (owner.characterWriter.Data.itemInHand, 1), target)
					.OnSuccess (response => OnGiveResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 7;
				break;
			case 7:
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
				state = 4;
			} else {
				Debug.LogWarning ("boss says its quittin time");
				success = true;
			}
		}

		private void OnGiveResult(GiveResponse r) {
			if (r.success) {
				owner.DropItem ();
				state = 2;
			} else {
				// theres no room in the forester
				success = true;
			}
		}

		private void OnRequestFailed () {
			failed = true;
		}

		private void OnSuccessfulHQQuery (EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			if (resultMap.Count < 1)
				failed = true;
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position> ();
			hqPosition = p.Value.Get().Value.coords.ToVector3();
			state = 0;
		}

		private void OnFailedEntityQuery (ICommandErrorDetails _) {
			failed = true;
		}

	}

}
