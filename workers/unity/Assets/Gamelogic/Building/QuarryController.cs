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
		[Require] private WorkSite.Writer workSiteWriter;

		public float quarryStoneGenerationRate = 5f;
		private float timer = -1f;
		private int workerCount;
		private InventoryController inventoryController;

		void OnEnable () {
			workSiteWriter.InsideUpdated.Add (OnInsideUpdated);
			inventoryController = GetComponent<InventoryController> ();
		}

		void OnDisable () {
			quarryWriter.CommandReceiver.OnChangeWorkers.DeregisterResponse ();
		}


		void Update() {

			if (timer < 0f) {
				inventoryController.Insert (1, workerCount);
				timer = 0f + Time.deltaTime;

			} else {
				timer += Time.deltaTime;
				if (timer >= quarryStoneGenerationRate)
					timer = -1f;
			}
		}

		private void OnInsideUpdated(List<WorkerData> inside) {

			workerCount = inside.Count;

		}

	}

}