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
	
	public class ActionStockpile : Action {

		private int state = 0;
		private bool failed = false;
		private bool success = false;
		private EntityId target;
		private Vector3 hqPosition;
		private Action subAction;
		private float timeIdle = 0f;

		public ActionStockpile(CharacterController o, EntityId t, Vector3 p) : base(o)	{
			target = t;
			hqPosition = p;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				// send query
				var entityQuery = Query.HasEntityId (target).ReturnComponents (Inventory.ComponentId, Storage.ComponentId);
				SpatialOS.WorkerCommands.SendQuery (entityQuery)
					.OnSuccess (OnSuccessfulEntityQuery)
					.OnFailure (OnFailedEntityQuery);
				state = 1;
				break;
			case 1:
				// waiting
				break;
			case 2:
				// run resource get
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					if (((ActionResourceGet)subAction).foundViableStorage) {
						subAction = new ActionSeek (owner, target, hqPosition);
						state = 3;
					} else {
						state = 5;
					}
				}
				break;
			case 3:
				// walking back
				c = subAction.Update ();
				if (c == ActionCode.Failure || c == ActionCode.Success) {
					// restart
					state = 4;
					SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.Give.Descriptor, new ItemStack (owner.characterWriter.Data.itemInHand, 1), target)
						.OnSuccess (response => OnGiveResult (response))
						.OnFailure (response => OnRequestFailed ());
				}
				break;
			case 4:
				// waiting on give
				break;
			case 5:
				// nothing to do
				timeIdle += Time.deltaTime;
				if (timeIdle > 60f) {
					state = 0;
					timeIdle = 0f;
				}
				break;
			default:
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
				failed = true; // the hq is gone
				return;
			}
			Entity e = resultMap.First.Value.Value;
			Improbable.Collections.Option<IComponentData<Inventory>> i = e.Get<Inventory>();
			InventoryData invData = i.Value.Get().Value;
			Improbable.Collections.Option<IComponentData<Storage>> s = e.Get<Storage>();
			StorageData storageData = s.Value.Get().Value;
			Dictionary<int,int> toGet = ParseToGet (invData, storageData);
			if (toGet.Count < 1)
				state = 5; // nothing to get
			else {
				subAction = new ActionResourceGet (owner, storageData.workSourcing, toGet, target);
				state = 2;
			}
		}

		private void OnFailedEntityQuery(ICommandErrorDetails _) {
			failed = true;
		}

		private void OnGiveResult(GiveResponse r) {
			if (r.success) {
				owner.DropItem ();
				state = 0;
			} else {
				// theres no room I guess
				owner.DropItem ();
				state = 0;
			}
		}

		private void OnRequestFailed () {
			failed = true;
		}

		private Dictionary<int,int> ParseToGet(InventoryData inventory, StorageData storage) {
			Dictionary<int,int> d = new Dictionary<int,int> ();
			// for each thing in the quota, get the diff between that and the inv, add that
			foreach (int id in storage.quotas.Keys) {
				if (!inventory.inventory.ContainsKey (id)) {
					d.Add (id, storage.quotas [id]);
				} else {
					int i =  storage.quotas [id] - inventory.inventory [id];
					if (i > 0)
						d.Add (id, i);
				}
			}

			return d;
		}


	}

	// waiting around: just loop blank on an action 
	// and stay enlisted, when the building needs them 
	// it sends a command to the character who forwards 
	// it to current action

}