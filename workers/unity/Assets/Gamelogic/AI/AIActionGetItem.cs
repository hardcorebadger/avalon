﻿using System.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Collections;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class AIActionGetItem : AIAction {

		// description //
		// agent gets 1 of a list of item types

		// assumptions //
		// agent is empty handed

		// end condition //
		// agent has 1 of the list's item types in its hand (200's)

		// reponse codes //
		// 201 = no items were asked for (no item is in hand)
		// 202 = agent had an applicable item in hand
		// 401 = agent had inapplicable item in hand (that item is in their hand)
		// 402 = no applicable items could be found (no district) (no item is in hand)
		// 403 = no applicable items could be found (in district) (no item is in hand)
		// 501 = district request failed (no item in hand)
		// 502 = storage take request failed (no item in hand)
		// 503 = storage denied the take request (no item in hand)

		private int toGet;
		private Option<EntityId> districtId;
		private Option<EntityId> asker;
		private ItemFindResponse findResponse;
		private TakeResponse takeResponse;
		private AIActionGoTo seek;
		private bool needToWalk = true;

		public AIActionGetItem(CharacterController o, int tg, Option<EntityId> d) : base(o,"get") {
			toGet = tg;
			districtId = d;
			asker = new Option<EntityId>();
		}

		public AIActionGetItem(CharacterController o, int tg, Option<EntityId> d, EntityId a) : base(o,"get") {
			toGet = tg;
			districtId = d;
			asker = new Option<EntityId>(a);
		}

		public AIActionGetItem DontWalkThere() {
			needToWalk = false;
			return this;
		}

		public override int Update(){

			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				// do some prelim checks
				if (toGet == -1)
					return 201;
				if (!agent.EmptyHanded ()) {
					if (toGet == agent.GetItemInHand ())
						return 202;
					else {
						return 401;
					}
				}
				if (!districtId.HasValue)
					return 402;
				state++;
				break;
			case 1:
				// ask district for a place to get this stuff
				SpatialOS.Commands.SendCommand (agent.characterWriter, District.Commands.FindItemGet.Descriptor, new ItemFindRequest (toGet, asker), districtId.Value)
					.OnSuccess (response => OnFindResult (response))
					.OnFailure (response => OnFindRequestFailed ());
				state++;
				break;
			case 2:
				// waiting for request
				break;
			case 3:
				// process reponse
				if (!findResponse.building.HasValue) {
					return 403;
				} else {
					if (!needToWalk)
						state += 2;
					else {
						seek = new AIActionGoTo (agent, findResponse.building.Value, findResponse.position.Value.ToUnityVector ());
						state++;
					}
				}
				break;
			case 4:
				// walk to storage
				if (AIAction.OnSuccess (seek.Update ()))
					state++;
				break;
			case 5:
				// take the item
				SpatialOS.Commands.SendCommand (agent.characterWriter, Inventory.Commands.Take.Descriptor, new ItemStack (toGet, 1), districtId.Value)
					.OnSuccess (response => OnTakeResult (response))
					.OnFailure (response => OnTakeRequestFailed ());
				state++;
				break;
			case 6:
				// waiting on take request
				break;
			case 7:
				if (takeResponse.success) {
					if (!agent.SetInHandItem (toGet))
						return 402;
					else
						return 200;
				} else {
					return 503;
				}
				break;
			}
			return 100;
		}

		private void OnFindResult (ItemFindResponse response) {
			findResponse = response;
			state++;
		}

		private void OnTakeResult (TakeResponse response) {
			takeResponse = response;
			state++;
		}

		private void OnFindRequestFailed () {
			shouldRespond = 501;
		}

		private void OnTakeRequestFailed () {
			shouldRespond = 502;
		}

	}

}