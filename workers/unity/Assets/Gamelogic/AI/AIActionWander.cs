using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;
using Improbable;
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

	public class AIActionWander : AIAction {

		private float time = 0f;
		private float duration;
		private EntityId pivotEntity;
		private Vector3 pivot;
		private AIAction subAction;
		private float radius;

		public AIActionWander(CharacterController o, float d, Vector3 p, float r) : base(o,"wander") {
			duration = d;
			pivot = p;
			radius = r;
		}

		public AIActionWander(CharacterController o, float d, EntityId pe, float r) : base(o,"wander") {
			duration = d;
			// request entity position
			state = -2;
			pivotEntity = pe;
			radius = r;
		}

		public override int Update(){

			if (shouldRespond != 100) {
				Debug.LogWarning ("error");
				return shouldRespond;
			}
			
			if (state > 1) {
				time += Time.deltaTime;
				if (time > duration) {
					Debug.LogWarning ("done wandering");
					return 200;
				}
			}

			switch (state) {
			case -2:
				Debug.LogWarning ("req");
				// query for position
				var entityQuery = Query.HasEntityId(pivotEntity).ReturnComponents(Position.ComponentId);
				SpatialOS.WorkerCommands.SendQuery(entityQuery)
					.OnSuccess(OnSuccessfulEntityQuery)
					.OnFailure(OnFailedEntityQuery);
				state++;
				break;
			case -1:
				// wait for response
				break;
			// initial wait
			case 0:
				Debug.LogWarning ("wait init");
				subAction = new AIActionWait (agent, 30f);
				state++;
				break;
			case 1:
				if (AIAction.OnTermination (subAction.Update ()))
					state++;
				break;
			//LOOP go somewhere
			case 2:
				Vector3 v = GetRandomLocation ();
				Debug.LogWarning ("walk: " + v);
				subAction = new AIActionGoTo (agent, GetRandomLocation ());
				state++;
				break;
			case 3:
				if (AIAction.OnTermination(subAction.Update()))
					state++;
				break;
			// hang there a bit
			case 4:
				Debug.LogWarning ("wait");
				subAction = new AIActionWait (agent, Random.Range(5f,30f));
				state++;
				break;
			case 5:
				if (AIAction.OnTermination(subAction.Update()))
					state = 2;
				break;
			//END LOOP
			}
			return 100;
		}

		private Vector3 GetRandomLocation() {
			return pivot + new Vector3 (DonutRandom (), 0f, DonutRandom ());
		}

		private float DonutRandom() {
			float f = Random.Range (14f, radius);
			if (Random.Range(0,2) == 0)
				f *= -1;
			return f;
		}

		private void OnSuccessfulEntityQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			if (resultMap.Count == 0) {
				shouldRespond = 401;
				return;
			}
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			pivot = p.Value.Get().Value.coords.ToVector3();
			state++;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			shouldRespond = 501;
		}
	}

}