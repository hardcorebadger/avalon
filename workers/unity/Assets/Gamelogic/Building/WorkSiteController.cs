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
		[Require] private Building.Writer buildingWriter;

		public GameObject door;

		private Improbable.Collections.List<WorkerData> inside = new Improbable.Collections.List<WorkerData>();
		private Improbable.Collections.List<EntityId> workers = new Improbable.Collections.List<EntityId>();


		// Use this for initialization
		void OnEnable () {

			workSiteWriter.CommandReceiver.OnEnlist.RegisterResponse (OnEnlist);
			workSiteWriter.CommandReceiver.OnUnEnlist.RegisterResponse (OnUnEnlist);
			workSiteWriter.CommandReceiver.OnStartWork.RegisterResponse (OnStartWork);
			workSiteWriter.CommandReceiver.OnFireWorker.RegisterResponse (OnFireWorker);
		}

		// Update is called once per frame
		void OnDisable () {
			workSiteWriter.CommandReceiver.OnEnlist.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnUnEnlist.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnStartWork.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnFireWorker.DeregisterResponse (); 
		}

		private EnlistResponse OnEnlist(EnlistRequest request, ICommandCallerInfo callerinfo) {
			bool full = false;
			if (workers.Count + inside.Count >= workSiteWriter.Data.maxWorkers) {
				full = true;
			} else {
				workers.Add (request.worker);
				workSiteWriter.Send (new WorkSite.Update ()
					.SetWorkers (workers)
				);
			}
			return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d(transform.position.x, transform.position.y, transform.position.z), workSiteWriter.Data.interior, full, buildingWriter.Data.district);
		}

		private UnEnlistResponse OnUnEnlist(UnEnlistRequest request, ICommandCallerInfo callerinfo) {
			workers.RemoveAll (x => x.Id == request.worker.Id);
			workSiteWriter.Send (new WorkSite.Update ()
				.SetWorkers (workers)
			);
			return new UnEnlistResponse ();
		}

		private StartWorkResponse OnStartWork(StartWorkRequest request, ICommandCallerInfo callerinfo) {
			bool success = true;
			if (workers.Contains(request.worker)) {
				workers.Remove (request.worker);
				inside.Add (new WorkerData (request.playerId));
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

		private FireWorkerResponse OnFireWorker(FireWorkerRequest request, ICommandCallerInfo callerinfo) {
			if (workers.Count > 0) {
				SpatialOS.Commands.SendCommand (workSiteWriter, Character.Commands.Fire.Descriptor, new Nothing (), workers[0]);
				// they get removed from workers when they send back their unenlist request
			} else if (inside.Count > 0) {
				Respawn (inside [0]);
				inside.RemoveAt (0);
				workSiteWriter.Send (new WorkSite.Update ()
					.SetInside (inside)
				);
			}

			return new FireWorkerResponse (true);
		}

		private void Respawn(WorkerData d) {
			SpatialOS.Commands.CreateEntity(workSiteWriter, EntityTemplates.EntityTemplateFactory.CreateCharacterTemplate(door.transform.position,d.playerId))
				.OnSuccess(entityId => Debug.Log("Created entity with ID: " + entityId))
				.OnFailure(errorDetails => Debug.Log("Failed to create entity with error: " + errorDetails.ErrorMessage));
		}

	}

}