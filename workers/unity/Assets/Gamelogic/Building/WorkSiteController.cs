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

		[Require] public WorkSite.Writer workSiteWriter;
		[Require] private Building.Writer buildingWriter;

		public Improbable.Collections.List<EntityId> workers = new Improbable.Collections.List<EntityId>();

		private OwnedController owned;
		private BuildingController building;
		private InteriorPosition[] interiorPositions;

		// Use this for initialization
		void OnEnable () {
			workSiteWriter.CommandReceiver.OnEnlist.RegisterResponse (OnEnlist);
			workSiteWriter.CommandReceiver.OnUnEnlist.RegisterResponse (OnUnEnlist);
			workSiteWriter.CommandReceiver.OnFireWorker.RegisterResponse (OnFireWorker);

			owned = GetComponent<OwnedController> ();
			building = GetComponent<BuildingController> ();
			interiorPositions = GetComponentsInChildren<InteriorPosition> ();
		}

		// Update is called once per frame
		void OnDisable () {
			workSiteWriter.CommandReceiver.OnEnlist.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnUnEnlist.DeregisterResponse ();
			workSiteWriter.CommandReceiver.OnFireWorker.DeregisterResponse (); 
		}

		private EnlistResponse OnEnlist(EnlistRequest request, ICommandCallerInfo callerinfo) {
			// return no
			if (workers.Count >= workSiteWriter.Data.maxWorkers)
				return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d (building.door.position.x, building.door.position.y, building.door.position.z), true, buildingWriter.Data.district.Value, new Option<Vector3d> ());
			
			// add to list
			workers.Add (request.worker);
			workSiteWriter.Send (new WorkSite.Update ()
				.SetWorkers (workers)
			);

				// register job with district
			if (buildingWriter.Data.district.HasValue)
				SpatialOS.Commands.SendCommand (workSiteWriter, District.Commands.SetJob.Descriptor, new SetJobRequest(request.worker,new JobInfo(gameObject.EntityId(),workSiteWriter.Data.type),new Option<EntityId>()), buildingWriter.Data.district.Value);

			// return no interior pos
			if (interiorPositions.Length < workers.Count) {
				return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d (building.door.position.x, building.door.position.y, building.door.position.z), false, buildingWriter.Data.district.Value, new Option<Vector3d> ());
			} else {
				// return with next interior pos
				Vector3 v = interiorPositions [workers.Count - 1].transform.position;
				Option<Vector3d> v3d = new Option<Vector3d> (new Vector3d(v.x,v.y,v.z));
				return new EnlistResponse (workSiteWriter.Data.type, new Improbable.Vector3d (building.door.position.x, building.door.position.y, building.door.position.z), false, buildingWriter.Data.district.Value, v3d);
			}

		}

		private UnEnlistResponse OnUnEnlist(UnEnlistRequest request, ICommandCallerInfo callerinfo) {
			workers.RemoveAll (x => x.Id == request.worker.Id);
			workSiteWriter.Send (new WorkSite.Update ()
				.SetWorkers (workers)
			);

				// register unemployed with district
			if (buildingWriter.Data.district.HasValue)
				SpatialOS.Commands.SendCommand (workSiteWriter, District.Commands.SetJob.Descriptor, new SetJobRequest(request.worker,new Option<JobInfo>(),new Option<EntityId>(gameObject.EntityId())), buildingWriter.Data.district.Value);

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

			if (workers.Count >= 1) {
				SpatialOS.Commands.SendCommand (workSiteWriter, Character.Commands.Fire.Descriptor, new Nothing (), workers [workers.Count - 1]);
				return new FireWorkerResponse (true);
			} else {
				return new FireWorkerResponse (false);
			}
		}

		public void FireAll() {
			foreach (EntityId i in workers) {
				SpatialOS.Commands.SendCommand (workSiteWriter, Character.Commands.Fire.Descriptor, new Nothing (), i);
			}
		}

	}

}