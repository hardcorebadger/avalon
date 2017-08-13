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
		void Start () {
		
			gatherableWriter.CommandReceiver.OnRequestGather.RegisterResponse(OnGatherRequest);

		
		}	

		private GatherResponse OnGatherRequest(GatherRequest request, ICommandCallerInfo callerinfo) {
			SpatialOS.WorkerCommands.DeleteEntity (gatherableWriter.EntityId);
			return new GatherResponse (true, gatherableWriter.Data.inventory);
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}
