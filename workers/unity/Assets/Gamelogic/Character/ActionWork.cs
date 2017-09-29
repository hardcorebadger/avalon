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

		private WorkType workType;
		private Vector3 buildingPosition;

		public ActionWork(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				// ask to work for the entity
				SpatialOS.Commands.SendCommand (owner.characterWriter, WorkSite.Commands.Enlist.Descriptor, new EnlistRequest (owner.gameObject.EntityId()), target)
					.OnSuccess (response => OnEnlistResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 1;
				break;
			case 1:
				// waiting to enlist
				break;
			case 2:
				subAction = new ActionSeek (owner, target, buildingPosition);
				state = 3;
				break;
			case 3:
				ActionCode seekC = subAction.Update ();
				if (seekC == ActionCode.Failure || seekC == ActionCode.Success) {

					switch (workType) {
					case WorkType.WORK_BUILDING:
						subAction = new ActionConstruction (owner, target, buildingPosition);
						state = 4;

						break;
					case WorkType.WORK_LOGGING:
						subAction = new ActionForester (owner, target, buildingPosition);
						state = 4;
						break;
					case WorkType.WORK_MINING: 
						//TAKE ME BABAY
						SpatialOS.Commands.SendCommand (owner.characterWriter, WorkSite.Commands.StartWork.Descriptor, new StartWorkRequest (owner.gameObject.EntityId(), owner.characterWriter.Data.playerId), target)
							.OnSuccess (response => OnStartWorkResult (response))
							.OnFailure (response => OnRequestFailed ());
						state = 4;
						break;
					}
				}
				break;
			case 4:
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
			SpatialOS.Commands.SendCommand (owner.characterWriter, WorkSite.Commands.UnEnlist.Descriptor, new UnEnlistRequest (owner.gameObject.EntityId()), target);
		}

		private void OnEnlistResult(EnlistResponse response) {

			if (response.full) {
				failed = true;
				Debug.LogWarning ("Failed");
			} else {
				workType = response.workType;
				buildingPosition = response.position.ToUnityVector ();
				state = 2;
			}
		}

		private void OnStartWorkResult(StartWorkResponse response) {

			if (response.success) {

				//we were taken

			}

		}

		private void OnRequestFailed() {
			failed = true;
		}

	}

}