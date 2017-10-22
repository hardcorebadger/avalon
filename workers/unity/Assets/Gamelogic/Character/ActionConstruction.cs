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
		private bool didSource = false;
		private Vector3 hqPosition;
		private Option<EntityId> district;

		public ActionConstruction(CharacterController o, EntityId t, Option<EntityId> d, Vector3 pos) : base(o)	{
			target = t;
			hqPosition = pos;
			district = d;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				// get construction data
				var entityQuery = Query.HasEntityId (target).ReturnComponents (Construction.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulEntityQuery)
					.OnFailure (OnFailedEntityQuery);
				state = 1;
				break;
			case 1:
				// waiting on query
				// if you already sourced resources and you still dont have a viable item, quit
				// if you have a viable item, put it in
				// if you dont have a viable item, skip the next action (it happens in action build init)
				break;
			case 2:
				// query is back, try to just drop resources into it
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					if (((ActionBuild)subAction).constructionComplete)
						success = true;
					else {
						state = 3;
						subAction = new ActionResourceGetDistrict (owner, district, ParseConstructionRequirements ());
					}
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
			case 4:
				// construction is finished, just walk back to the building
				c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
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
			if (resultMap.Count < 1) {
				state = 4;
				return;
			}
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Construction>> c = e.Get<Construction>();
			constructionData = c.Value.Get().Value;

			if (ParseConstructionRequirements ().Count == 0) {
				subAction = new ActionSeek (owner, target, hqPosition);
				state = 4;
			}

			if (!owner.HasApplicableItem (constructionData) && didSource)
				success = true;

			subAction = new ActionBuild (owner, target, constructionData, hqPosition);
			state = 2;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			state = 4;
		}

		private List<int> ParseConstructionRequirements() {
			List<int> p = new List<int> ();
			foreach (int id in constructionData.requirements.Keys) {
				if (constructionData.requirements [id].required - constructionData.requirements [id].amount > 0)
					p.Add (id);
			}
			return p;
		}
	}

}

