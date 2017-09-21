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

	public class ActionBuild : Action {

		private EntityId target;
		private Vector3 position;
		public int stage = -1;
		public bool failed = false;
		public bool succeeded = false;
		private Dictionary<int,int> overlap;
		public ActionSeek seek;
		private float time = 0;

		public ActionBuild(CharacterController o, EntityId t, ConstructionData cdata, Vector3 pos) : base(o)	{
			target = t;
			position = pos;
			if (!owner.HasApplicableItem (cdata))
				succeeded = true;
			stage = 0;
		}

		public ActionBuild(CharacterController o, EntityId t) : base(o)	{
			target = t;
			var entityQuery = Query.HasEntityId(target).ReturnComponents(Position.ComponentId, Construction.ComponentId);

			SpatialOS.WorkerCommands.SendQuery(entityQuery)
				.OnSuccess(OnSuccessfulEntityQuery)
				.OnFailure(OnFailedEntityQuery);
		}

		public override ActionCode Update () {
			if (failed)
				return ActionCode.Failure;
			if (succeeded)
				return ActionCode.Success;

			if (stage == -1) {
				seek = new ActionSeek (owner, target, position);
				stage = 1;
			} else if (stage == 0) {
				seek = new ActionSeek (owner, target, position);
				stage = 1;
			} else if (stage == 1) {
				ActionCode seekProgress = seek.Update ();
				if (seekProgress == ActionCode.Success)
					stage = 2;
				if (seekProgress == ActionCode.Failure)
					failed = true;
			} else if (stage == 2) {
				owner.SetState (CharacterState.BUILDING);
				stage = 3;
			} else if (stage == 3) {
				time += Time.deltaTime;
				if (time > 5f)
					stage = 4;
			} else if (stage == 4) {
				owner.SetState (CharacterState.DEFAULT);
				SpatialOS.Commands.SendCommand (owner.characterWriter, Construction.Commands.Give.Descriptor, new ItemStack(owner.characterWriter.Data.itemInHand,1), target)
					.OnSuccess(response => OnGiveResult(response))
					.OnFailure(response => OnRequestFailed());
				stage = 5;
			}

			return ActionCode.Working;
		}

		private void OnSuccessfulEntityQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			Improbable.Collections.Option<IComponentData<Construction>> c = e.Get<Construction>();
			position = p.Value.Get().Value.coords.ToVector3();
			ConstructionData construction = c.Value.Get().Value;

			if (!owner.HasApplicableItem (construction))
				succeeded = true;
			
			stage = 0;
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

		public void OnGiveResult(GiveResponse response) {
			if (response.success) {
				owner.DropItem ();
				succeeded = true;
			} else {
				Debug.LogWarning ("Construction Rejected Give");
				failed = true;
			}
		}

		public void OnRequestFailed() {
			failed = true;
			Debug.LogWarning ("Construction Request Failed");
		}

	}

}