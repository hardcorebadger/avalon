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

		public Transform door;

		public Improbable.Collections.List<EntityId> workers = new Improbable.Collections.List<EntityId>();

		private InteriorPosition[] interiorPositions;

		// Use this for initialization
		void OnEnable () {
			workSiteWriter.CommandReceiver.OnEnlist.RegisterResponse (OnEnlist);
			workSiteWriter.CommandReceiver.OnUnEnlist.RegisterResponse (OnUnEnlist);
			workSiteWriter.CommandReceiver.OnFireWorker.RegisterResponse (OnFireWorker);
			interiorPositions = GetComponentsInChildren<InteriorPosition> ();
			door = transform.FindChild ("door");
		}

		// Update is called once per frame
		void OnDisable () {
			workSiteWriter.CommandReceiver.OnEnlist.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnUnEnlist.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnFireWorker.DeregisterResponse (); 
		}

		private EnlistResponse OnEnlist(EnlistRequest request, ICommandCallerInfo callerinfo) {
			bool full = false;
			if (workers.Count >= workSiteWriter.Data.maxWorkers) {
				full = true;
			} else {
				workers.Add (request.worker);
				workSiteWriter.Send (new WorkSite.Update ()
					.SetWorkers (workers)
				);
			}
			if (full || interiorPositions.Length < workers.Count) {
				return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d (door.position.x, door.position.y, door.position.z), full, buildingWriter.Data.district, new Option<Vector3d> ());
			} else {
				Vector3 v = interiorPositions [workers.Count - 1].transform.position;
				Option<Vector3d> v3d = new Option<Vector3d> (new Vector3d(v.x,v.y,v.z));
				return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d (door.position.x, door.position.y, door.position.z), full, buildingWriter.Data.district, v3d);
			}
		}

		private UnEnlistResponse OnUnEnlist(UnEnlistRequest request, ICommandCallerInfo callerinfo) {
			workers.RemoveAll (x => x.Id == request.worker.Id);
			workSiteWriter.Send (new WorkSite.Update ()
				.SetWorkers (workers)
			);
			return new UnEnlistResponse ();
		}

//		private StartWorkResponse OnStartWork(StartWorkRequest request, ICommandCallerInfo callerinfo) {
//			bool success = true;
//			if (workers.Contains(request.worker)) {
//				workers.Remove (request.worker);
//				inside.Add (new WorkerData (request.playerId));
//				workSiteWriter.Send (new WorkSite.Update ()
//					.SetInside (inside)
//					.SetWorkers (workers)
//				);
//				SpatialOS.Commands.DeleteEntity(workSiteWriter, request.worker)
//					.OnSuccess(entityId => Debug.Log("Deleted entity: " + entityId))
//					.OnFailure(errorDetails => Debug.Log("Failed to delete entity with error: " + errorDetails.ErrorMessage));
//			} else {
//				success = false;
//			}
//
//			return new StartWorkResponse (success);
//		}

		private FireWorkerResponse OnFireWorker(FireWorkerRequest request, ICommandCallerInfo callerinfo) {
			SpatialOS.Commands.SendCommand (workSiteWriter, Character.Commands.Fire.Descriptor, new Nothing (), workers[workers.Count-1]);
			return new FireWorkerResponse (true);
		}

		private void Respawn(WorkerData d) {
			SpatialOS.Commands.CreateEntity(workSiteWriter, EntityTemplates.EntityTemplateFactory.CreateCharacterTemplate(door.transform.position,d.playerId))
				.OnSuccess(entityId => Debug.Log("Created entity with ID: " + entityId))
				.OnFailure(errorDetails => Debug.Log("Failed to create entity with error: " + errorDetails.ErrorMessage));
		}

	}

}