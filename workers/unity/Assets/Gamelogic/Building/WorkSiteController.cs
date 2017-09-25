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

namespace Assets.Gamelogic.Core {

	public class WorkSiteController : MonoBehaviour {

		[Require] private WorkSite.Writer workSiteWriter;

		private Improbable.Collections.List<WorkerData> inside = new Improbable.Collections.List<WorkerData>();
		private Improbable.Collections.List<EntityId> workers = new Improbable.Collections.List<EntityId>();


		// Use this for initialization
		void OnEnable () {

			workSiteWriter.CommandReceiver.OnEnlist.RegisterResponse (OnEnlist);
			workSiteWriter.CommandReceiver.OnStartWork.RegisterResponse (OnStartWork);
		}

		// Update is called once per frame
		void OnDisable () {
			workSiteWriter.CommandReceiver.OnEnlist.DeregisterResponse ();
		}

		private EnlistResponse OnEnlist(EnlistRequest request, ICommandCallerInfo callerinfo) {
			bool full = false;
			if (workers.Count + inside.Count >= 4) {
				full = true;
			} else {
				workers.Add (request.worker);
				workSiteWriter.Send (new WorkSite.Update ()
					.SetWorkers (workers)
				);
			}
			return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d(transform.position.x, transform.position.y, transform.position.z), workSiteWriter.Data.interior, full);
		}

		private StartWorkResponse OnStartWork(StartWorkRequest request, ICommandCallerInfo callerinfo) {
			bool success = true;
			if (workers.Contains(request.worker)) {
				workers.Remove (request.worker);
				inside.Add (new WorkerData (1));
				workSiteWriter.Send (new WorkSite.Update ()
					.SetInside (inside)
					.SetWorkers (workers)
				);
				SpatialOS.Commands.DeleteEntity(workSiteWriter, request.worker)
					.OnSuccess(entityId => Debug.Log("Deleted entity: " + entityId))
					.OnFailure(errorDetails => Debug.Log("Failed to delete entity with error: " + errorDetails.ErrorMessage));
			} else {
				success = false;
			}

			return new StartWorkResponse (success);
		}

	}

}