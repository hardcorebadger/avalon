using Improbable.Collections;
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

namespace Assets.Gamelogic.Core
{

	public class ActionResourceGetDistrict : Action {

		private int state = 0;
		private bool failed = false;
		private bool success = false;
		private EntityId target;
		private Option<EntityId> district;
		private EntityId asker;
		private List<int> toGet;
		private Action subAction;
		private bool hasAsker;
		private int gettingId = -1;
		public bool foundViableStorage = false;

		public ActionResourceGetDistrict(CharacterController o, Option<EntityId> d, List<int> tg) : base(o)	{
			district = d;
			toGet = tg;
			if (toGet.Count < 1)
				success = true;
		}

		public ActionResourceGetDistrict(CharacterController o, Option<EntityId> d, List<int> tg, EntityId ask) : this(o, d, tg)	{
			asker = ask;
			hasAsker = true;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				if (!owner.EmptyHanded ())
					return ActionCode.Success;
				else if (district.HasValue)
					state = 1;
				else
					return ActionCode.Success;
				break;
			case 1:
				// ask district where to go REQUEST
				Option<EntityId> opt;
				if (hasAsker)
					opt = new Option<EntityId> (asker);
				else
					opt = new Option<EntityId> ();
				SpatialOS.Commands.SendCommand (owner.characterWriter, District.Commands.FindAnyItem.Descriptor, new ItemFindRequest(toGet,opt), district.Value)
					.OnSuccess (response => OnFindResult (response))
					.OnFailure (response => OnRequestFailed ());
				break;
			case 2:
				//waiting... (processes request on return)
				break;
			case 3:
				// found a storage to go to, walking there...
				ActionCode seekProgress = subAction.Update ();
				if (seekProgress == ActionCode.Success) {
					state = 4;
				}
				if (seekProgress == ActionCode.Failure)
					failed = true;
				break;
			case 4:
				SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.Take.Descriptor, new ItemStack (gettingId, 1), target)
					.OnSuccess (response => OnTakeResult (response))
					.OnFailure (response => OnRequestFailed ());
				state = 5;
				break;
			case 5:
				// waiting...
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


		private void OnFindResult (ItemFindResponse response) {
			if (!response.storage.HasValue) {
				success = true;
			} else {
				foundViableStorage = true;
				gettingId = response.id;
				target = response.storage.Value;
				subAction = new ActionSeek (owner, target, response.position.ToUnityVector());
				state = 3;
			}
		}

		private void OnTakeResult (TakeResponse response) {
			if (response.success) {
				if (!owner.SetInHandItem(gettingId))
					failed = true;
				else
					state = 0;
			} else {
				// kinda weird, we should've been able to get it...
				state = 0;
			}
		}

		private void OnRequestFailed () {
			failed = true;
		}

	}

}
