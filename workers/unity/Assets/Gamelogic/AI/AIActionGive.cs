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

	public class AIActionGive : AIAction {

		// description //
		// agent puts in-hand item into an inventory

		// assumptions //
		// agent has something in hand to give

		// end condition //
		// agent is empty handed (200's)

		// reponse codes //
		// 401 = nothing in hand
		// 402 = inventory denied give, maybe it was full
		// 501 = inventory give request failed, probably no inventory component on entity

		private EntityId toGive;
		private GiveResponse giveResponse;

		public AIActionGive(CharacterController o, EntityId g) : base(o,"give") {
			toGive = g;
		}

		public override int Update(){

			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				if (agent.EmptyHanded ())
					return 401;
				state++;
				break;
			case 1:
				SpatialOS.Commands.SendCommand (agent.characterWriter, Inventory.Commands.Give.Descriptor, new ItemStack (agent.characterWriter.Data.itemInHand, 1), toGive)
					.OnSuccess (response => OnGiveResult (response))
					.OnFailure (response => OnRequestFailed ());
				state++;
				break;
			case 2:
				break;
			case 3:
				if (giveResponse.success)
					agent.DropItem ();
				else
					return 402;
				return 200;
			}
			return 100;
		}

		private void OnGiveResult(GiveResponse r) {
			giveResponse = r;
			state++;
		}

		private void OnRequestFailed() {
			shouldRespond = 501;
		}
	}

}