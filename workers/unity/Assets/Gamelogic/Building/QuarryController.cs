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


		void OnEnable () {
			quarryWriter.CommandReceiver.OnCompleteQuarryJob.RegisterResponse (OnCompleteJob);
		}

		void Update() {
		}

		private Nothing OnCompleteJob(Nothing n, ICommandCallerInfo _) {
			SpatialOS.Commands.SendCommand (quarryWriter, Inventory.Commands.Give.Descriptor, new ItemStack (1, 1), GetComponent<BuildingController> ().district.Value);
			return new Nothing ();
		}

	}

}