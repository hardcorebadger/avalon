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

	public class AIActionGoTo : AIActionLocomotion {

		// description //
		// agent walks to position or entity (stationary)

		// assumptions //
		// entity is stationary

		// end condition //
		// agent is near the target

		// reponse codes //
		// 201 = has entity target but got to position
		// 401 = entity didnt exist
		// 501 = entity query failed

		public Vector3 target;
		public EntityId targetId; //optional
		public bool hasTargetEntity = false;

		public AIActionGoTo(CharacterController o, Vector3 pos) : base(o,"goto")	{
			target = pos;
			state = 2;
		}

		public AIActionGoTo(CharacterController o, EntityId eid, Vector3 pos) : base(o,"goto")	{
			target = pos;
			targetId = eid;
			hasTargetEntity = true;
			state = 2;
		}

		public AIActionGoTo(CharacterController o, EntityId eid) : base(o,"goto")	{
			targetId = eid;
			hasTargetEntity = true;
		}
			
		public override int Update () {

			if (shouldRespond != 100)
				return shouldRespond;

			// you are querying for position data
			if (state < 2)
				return QueryUpdate ();
			else
				return SeekUpdate ();
		
		}

		private int SeekUpdate() {
			Vector3 dir = (target - agent.transform.position);
			dir.Normalize ();
			Steer (ref dir);

			Collider[] colliders;

			// do this for either - if the entity ts looking for gets deleted this will trigger saying it got there
			colliders = Physics.OverlapSphere (target, agent.arrivalRadius);
			foreach (Collider c in colliders) {
				if (c.gameObject == agent.gameObject) {
					agent.SetVelocity (0f);
					agent.rigidBody.angularVelocity = Vector3.zero;
					if (hasTargetEntity)
						return 201;
					return 200;
				}
			}

			// extra overlap for large collider objects
			if (hasTargetEntity) {
				colliders = Physics.OverlapSphere (agent.transform.position, agent.arrivalRadius);
				foreach (Collider c in colliders) {
					if (c.gameObject.EntityId() == targetId) {
						agent.SetVelocity (0f);
						agent.rigidBody.angularVelocity = Vector3.zero;
						return 200;
					}
				}
			}

			float f = GetRotationTo (dir);
			Vector3 v = agent.facing.eulerAngles;
			v += new Vector3 (0, f, 0);
			agent.facing.eulerAngles = v;
			agent.SetVelocity (agent.speed);

			return 100;
		}

		private int QueryUpdate() {
			switch (state) {
			case 0:
				// query for position
				var entityQuery = Query.HasEntityId(targetId).ReturnComponents(Position.ComponentId);
				SpatialOS.WorkerCommands.SendQuery(entityQuery)
					.OnSuccess(OnSuccessfulEntityQuery)
					.OnFailure(OnFailedEntityQuery);
				state++;
				break;
			case 1:
				// wait for response
				break;
			}
			return 100;
		}

		private void OnSuccessfulEntityQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			if (resultMap.Count == 0) {
				shouldRespond = 401;
				return;
			}
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			target = p.Value.Get().Value.coords.ToVector3();
			state++;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			shouldRespond = 501;
		}

	}


}