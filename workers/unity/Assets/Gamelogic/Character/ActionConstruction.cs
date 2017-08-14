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

	public class ActionConstruction : Action {

		private int state = 0;
		private EntityId target;
		private bool failed = false;
		private bool success = false;
		private Action subAction = null;
		private ConstructionData constructionData;
		private Dictionary<int,int> overlap;
		private bool didSource = false;

		public ActionConstruction(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				var entityQuery = Query.HasEntityId (target).ReturnComponents (Position.ComponentId, Construction.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulEntityQuery)
					.OnFailure (OnFailedEntityQuery);
				state = 1;
				break;
			case 1:
				// waiting on query
				break;
			case 2:
				// query is back, try to just drop resources into it
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					state = 3;
					subAction = new ActionResourceGet (owner, constructionData.workSourcing, ParseConstructionRequirements());
				}
				break;
			case 3:
				// time to go get some more resources
				c = subAction.Update ();
				if (c == ActionCode.Failure)
					return ActionCode.Failure;
				else if (c == ActionCode.Success) {
					didSource = true;
					state = 0;
				}
				// and then restart
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
			Improbable.Collections.Option<IComponentData<Construction>> c = e.Get<Construction>();
			Vector3 position = p.Value.Get().Value.coords.ToVector3();
			constructionData = c.Value.Get().Value;

			overlap = owner.inventory.GetConstructionOverlap (constructionData);

			if (overlap.Count == 0 && didSource)
				success = true;

			subAction = new ActionBuild (owner, target, overlap, position);
			state = 2;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

		private Dictionary<int,int> ParseConstructionRequirements() {
			Dictionary<int,int> p = new Dictionary<int,int> ();
			foreach (int id in constructionData.requirements.Keys) {
				p.Add (id, constructionData.requirements [id].required - constructionData.requirements [id].amount);
			}
			return p;
		}
	}

}

