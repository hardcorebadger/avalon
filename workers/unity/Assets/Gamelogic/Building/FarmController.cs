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

	public class FarmController : MonoBehaviour {

		[Require] private Farm.Writer farmWriter;

		private InventoryController inventoryController;

		void OnEnable () {
			farmWriter.CommandReceiver.OnCompleteFarmJob.RegisterResponse (OnCompleteJob);

			inventoryController = GetComponent<InventoryController> ();
		}

		void Update() {

		}

		private Nothing OnCompleteJob(Nothing n, ICommandCallerInfo _) {

			inventoryController.Insert (2, 1);

			return new Nothing ();
		}

	}

}