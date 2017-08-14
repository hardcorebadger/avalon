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

	public class ActionStore : Action {

		private EntityId target;
		private int state = 0;
		private bool failed = false;
		private bool success = false;
		private Vector3 position;
		private StorageData storageData;
		private InventoryData inventoryData;
		private Action subAction;
		private ItemStackList storing;

		public ActionStore(CharacterController o, EntityId t) : base(o)	{
			target = t;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				var entityQuery = Query.HasEntityId (target).ReturnComponents (Position.ComponentId, Storage.ComponentId, Inventory.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulEntityQuery)
					.OnFailure (OnFailedEntityQuery);
				state = 1;
				break;
			case 1:
				// waiting on query
				break;
			case 2:
				// walking over...
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					state = 3;
				}
				break;
			case 3:
				// got there, now determine what u can store and throw it in there
				storing = owner.inventory.GetStorableItems(storageData,inventoryData);
				SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.GiveMultiple.Descriptor, storing, target)
					.OnSuccess(response => OnGiveResult(response))
					.OnFailure(response => OnRequestFailed());
				state = 4;
				break;
			case 4:
				// waiting on command to give shit
				break;
			}

			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Working;
		}

		private void OnSuccessfulEntityQuery(EntityQueryResult queryResult) {
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
			Improbable.Collections.Option<IComponentData<Storage>> c = e.Get<Storage>();
			Improbable.Collections.Option<IComponentData<Inventory>> i = e.Get<Inventory>();
			position = p.Value.Get().Value.coords.ToVector3();
			storageData = c.Value.Get().Value;
			inventoryData = i.Value.Get().Value;

			if (owner.inventory.CanStoreSomething (storageData, inventoryData)) {
				subAction = new ActionSeek (owner, position);
				state = 2;
			} else {
				success = true;
			}

		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

		public void OnGiveResult(GiveResponse response) {
			
			if (response.success) {
				Dictionary<int,int> d = new Dictionary<int,int> ();
				foreach (int id in storing.inventory.Keys) {
					d.Add (id, storing.inventory [id]);
				}
				owner.inventory.Drop (d);
				success = true;
			} else {
				success = true;
			}
		}

		public void OnRequestFailed() {
			failed = true;
		}
	}

}
