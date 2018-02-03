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
	public class GatherableController : MonoBehaviour {

		[Require] private Gatherable.Writer gatherableWriter;
		// Use this for initialization
		void OnEnable () {
			gatherableWriter.CommandReceiver.OnRequestGather.RegisterResponse(OnGatherRequest);
		}	

		void OnDisable () {
			gatherableWriter.CommandReceiver.OnRequestGather.DeregisterResponse();
		}

		private GatherResponse OnGatherRequest(GatherRequest request, ICommandCallerInfo callerinfo) {
			if (gatherableWriter.Data.items.amount - 1 == 0)
				SpatialOS.WorkerCommands.DeleteEntity (gatherableWriter.EntityId);
			else {
				gatherableWriter.Send (new Gatherable.Update ()
					.SetItems (new ItemStack (gatherableWriter.Data.items.id, gatherableWriter.Data.items.amount - 1))
				);
			}

			return new GatherResponse (true, new ItemStack(gatherableWriter.Data.items.id,1));
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}
