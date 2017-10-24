using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;
using Improbable.Worker;
using Improbable.Unity.Core.Acls;
using Improbable.Unity.Entity;
using Improbable;
using Improbable.Collections;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Collections;


namespace Assets.Gamelogic.Core {

	public class AIActionPlace : AIAction {

		// description //
		// agent creates an entity given

		// assumptions //
		// none

		// end condition //
		// agent has placed the entity

		// reponse codes //
		// 501 

		private Entity entityToPlace;

		public AIActionPlace(CharacterController o, Entity e) : base(o) {
			entityToPlace = e;
		}

		public override int Update(){
			
			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				SpatialOS.Commands.CreateEntity (agent.characterWriter, entityToPlace)
					.OnSuccess (entityId => OnCreated ())
					.OnFailure (result => OnFailure ());
				state++;
				break;
			}
			return 100;
		}

		private void OnCreated() {
			shouldRespond = 200;
		}

		private void OnFailure() {
			shouldRespond = 501;
		}
	}

}