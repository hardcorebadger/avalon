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

		void OnEnable () {
			farmWriter.CommandReceiver.OnCompleteFarmJob.RegisterResponse (OnCompleteJob);
		}

		void Update() {
		}

		private Nothing OnCompleteJob(Nothing n, ICommandCallerInfo _) {
			GetComponent<BuildingController> ().PushItemGetNotification (2);
			SpatialOS.Commands.SendCommand (farmWriter, Inventory.Commands.Give.Descriptor, new ItemStack (2, 1), GetComponent<BuildingController> ().district.Value);
			return new Nothing ();
		}

	}

}