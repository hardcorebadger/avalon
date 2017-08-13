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

	public class ActionGather : Action {

		public EntityId target;
		public GatherableData gatherable;
		public GatherResponse response;
		public Vector3 position;
		private int state = 0;
		public bool failed = false;
		private bool success = false;

		public ActionSeek seek;

		private float time = -1f;


		public ActionGather(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override void Log() {
			seek.Log ();
		}

		public override ActionCode Update () {

			switch (state) {
			case 0:
				var entityQuery = Query.HasEntityId(target).ReturnComponents(Position.ComponentId, Gatherable.ComponentId);
				SpatialOS.WorkerCommands.SendQuery(entityQuery)
					.OnSuccess(OnSuccessfulEntityQuery)
					.OnFailure(OnFailedEntityQuery);
				state = 1;
				break;
			case 1:
				// waiting on query
				break;
			case 2:
				// query is back, can we harvest? if so, move to
				int itemID = gatherable.inventory.Keys.GetEnumerator ().Current;

				if (Item.GetWeight (itemID) <= owner.inventory.GetAvailableWeight ()) {
					// can harvest
					if (seek == null) {
						seek = new ActionSeek (owner, position);
					}

					ActionCode seekProgress = seek.Update ();
					if (seekProgress == ActionCode.Success) {
						state = 3;
						owner.SetState (CharacterState.CHOPPING);
						time = 0f;
					}
				} else {
					// too full to harvest this
					success = true;
				}
				break;
			case 3:
				// we're there! Lets do this bitch
				time+= 0.1F;
				if (time >= gatherable.strength) {
					//successfully chopped, send gather request
					SpatialOS.Commands.SendCommand (owner.characterWriter, Gatherable.Commands.RequestGather.Descriptor , new GatherRequest(owner.characterWriter.EntityId), target)
						.OnSuccess(response => OnGatherResponse(response))
						.OnFailure(response => OnGatherFailed());
					state = 4;
				}
				break;
			case 4:
				break;
			case 5:
				//we got the gather response
				if (response != null && response.success) {
					var first = response.inventory.First;
					int id = first.Value.Key;
					int amount = first.Value.Value;
					owner.inventory.Insert (id, amount);
					success = true;
				} else {
					//gatherable said no!
					failed = true;
				}
				owner.SetState (CharacterState.DEFAULT);

				break;
			}

			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;

		}

		private void OnSuccessfulEntityQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			Improbable.Collections.Option<IComponentData<Gatherable>> g = e.Get<Gatherable>();
			gatherable = g.Value.Get().Value;
			position = p.Value.Get().Value.coords.ToVector3();
			state = 2;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

		public void OnGatherResponse(GatherResponse response) {

			this.response = response;
			state = 5;

		}

		public void OnGatherFailed() {

			state = 5;
			this.response = null;

		}

	}
}