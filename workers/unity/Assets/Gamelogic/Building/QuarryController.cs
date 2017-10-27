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

	public class QuarryController : MonoBehaviour {

		[Require] private Quarry.Writer quarryWriter;

		private InventoryController inventoryController;

		void OnEnable () {
			quarryWriter.CommandReceiver.OnCompleteQuarryJob.RegisterResponse (OnCompleteJob);
			inventoryController = GetComponent<InventoryController> ();
		}

		void Update() {

		}

		private Nothing OnCompleteJob(Nothing n, ICommandCallerInfo _) {

			inventoryController.Insert (1, 1);

			return new Nothing ();
		}

	}

}