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

	public class ActionMigrate : Action {

		public EntityId target;
		public Vector3 position;
		private int state = 0;
		public bool failed = false;
		private bool success = false;

		public ActionSeek seek;

		public ActionMigrate(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override void Log() {
			seek.Log ();
		}

		public override ActionCode Update () {

			switch (state) {
			case 0:
				var entityQuery = Query.HasEntityId (target).ReturnComponents (Position.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulEntityQuery)
					.OnFailure (OnFailedEntityQuery);
				state = 1;
				break;
			case 1:
				// waiting on query
				break;
			case 2:
				if (seek == null) {
					seek = new ActionSeek (owner, position);
				}

				ActionCode seekProgress = seek.Update ();
				if (seekProgress == ActionCode.Success) {
					owner.SetTown (target);
					SpatialOS.Commands.SendCommand (owner.characterWriter, TownCenter.Commands.AddCitizen.Descriptor, new TownAddRequest (owner.gameObject.EntityId ()), target);
					success = true;
				}
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
			if (resultMap.Count == 0) {
				Debug.Log("Town no longer exits");
				success = true;
				return;
			}
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			position = p.Value.Get().Value.coords.ToVector3();
			state = 2;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

	}

}