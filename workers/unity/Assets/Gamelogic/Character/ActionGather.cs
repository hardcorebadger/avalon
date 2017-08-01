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
		public InventoryData inventory;
		public Vector3 position;
		public bool loaded = false;
		public bool failed = false;
		public bool complete = false;

		public ActionSeek seek;

		public ActionGather(CharacterController o, EntityId t) : base(o)	{
			target = t;
			var entityQuery = Query.HasEntityId(target).ReturnComponents(Position.ComponentId, Inventory.ComponentId);

			SpatialOS.WorkerCommands.SendQuery(entityQuery)
				.OnSuccess(OnSuccessfulEntityQuery)
				.OnFailure(OnFailedEntityQuery);
		}

		public override ActionCode Update () {
			if (loaded && !complete) {

				if (seek == null) {
					seek = new ActionSeek (owner, position);
				}
				ActionCode seekProgress = seek.Update ();
				if (seekProgress == ActionCode.Success) {
					complete = true;
					int removalCount = inventory.inventory [1];
					owner.inventory.Insert (1, removalCount);
					SpatialOS.WorkerCommands.DeleteEntity (target);
					return ActionCode.Success;
				}
			}

			if (failed) {
				return ActionCode.Failure;
			} else {
				return ActionCode.Working;
			}
		}

		private void OnSuccessfulEntityQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			Debug.LogWarning (queryResult.EntityCount);
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			Improbable.Collections.Option<IComponentData<Inventory>> i = e.Get<Inventory>();
			inventory = i.Value.Get().Value;
			position = p.Value.Get().Value.coords.ToVector3();
			loaded = true;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

	}
}