using System.Collections;
using System.Collections.Generic;
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

	public class AIActionStore : AIAction {

		// disclaimer
		// this action isnt done, it just deletes the item

		// description //
		// agent stores any in hand item

		// assumptions //
		// none

		// end condition //
		// agent is empty handed and has stored their in hand item if they had one

		// reponse codes //
		// 201 = agent was already empty handed

		private EntityId districtId;
		private ItemStorageResponse storageResponse;
		private GiveResponse giveResponse;


		public AIActionStore(CharacterController o, EntityId d) : base(o,"store") {
			districtId = d;
		}

		public override int Update(){

			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				if (agent.EmptyHanded ())
					return 201;
				agent.DropItem();
				return 200;
				break;
			}
			return 100;
		}

		private void OnFindResult (ItemStorageResponse response) {
			storageResponse = response;
			state++;
		}

		private void OnGiveResult (GiveResponse response) {
			giveResponse = response;
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