using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class ActionStash : Action {

		private bool working = false;
		private bool failed = false;
		private bool success = false;

		public ActionStash(CharacterController o) : base(o)	{}

		public override ActionCode Update () {

			if (!working) {
				working = true;
				SpatialOS.Commands.CreateEntity (owner.characterWriter, EntityTemplates.EntityTemplateFactory.CreateSackTemplate (owner.transform.position+owner.transform.forward))
					.OnSuccess (entityId => OnSackCreated (entityId.CreatedEntityId))
					.OnFailure (errorDetails => OnRequestFailed());
			}

			if (failed)
				return ActionCode.Failure;
			if (success)
				return ActionCode.Success;
			
			return ActionCode.Working;
		}

		public void OnSackCreated(EntityId id) {
			SpatialOS.Commands.SendCommand (owner.characterWriter, Inventory.Commands.GiveMultiple.Descriptor, owner.inventory.GetItemStackList(), id)
				.OnSuccess(response => OnGiveResult(response))
				.OnFailure(response => OnRequestFailed());
		}

		public void OnGiveResult(GiveResponse response) {
			if (response.success) {
				owner.inventory.Clear ();
				success = true;
			} else {
				Debug.LogWarning ("Stash Inventory Rejected Give");
				failed = true;
			}
		}

		public void OnRequestFailed() {
			failed = true;
			Debug.LogWarning ("Stash Request Failed");
		}

	}

}