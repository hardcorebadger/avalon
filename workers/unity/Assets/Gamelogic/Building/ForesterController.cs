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

	public class ForesterController : MonoBehaviour {

		[Require] private Forester.Writer foresterWriter;

		public float localTreeRefreshRate = 30f;
		// doesnt work yet
		public int maxTrees = 20;
		public int minTrees = 10;
		public float radius = 40f;
		private float timer = -1f;
		private List<EntityId> localTrees;
		private int plantedSinceLastRefresh = 0;

		void OnEnable () {
			foresterWriter.CommandReceiver.OnGetJob.RegisterResponse (OnGetJob);
			foresterWriter.CommandReceiver.OnCompleteJob.RegisterResponse (OnCompleteJob);
			RefreshLocalTrees ();
		}
		
		void OnDisable () {
			foresterWriter.CommandReceiver.OnGetJob.DeregisterResponse ();
			foresterWriter.CommandReceiver.OnCompleteJob.DeregisterResponse ();
		}

		void Update() {
			if (timer < 0f) {
				RefreshLocalTrees ();
				timer = 0f + Time.deltaTime;
			} else {
				timer += Time.deltaTime;
				if (timer >= localTreeRefreshRate)
					timer = -1f;
			}
		}

		private void RefreshLocalTrees () {
			var entityQuery = Query.And (Query.HasComponent<Gatherable> (), Query.InSphere (transform.position.x, transform.position.y,transform.position.z,radius)).ReturnComponents (Gatherable.ComponentId);
			SpatialOS.WorkerCommands.SendQuery (entityQuery)
				.OnSuccess (OnSuccessfulTreeQuery)
				.OnFailure (OnFailedEntityQuery);
		}

		private void OnSuccessfulTreeQuery (EntityQueryResult queryResult) {
			localTrees = new List<EntityId> ();
			Map<EntityId, Entity> resultMap = queryResult.Entities;
			foreach (EntityId id in resultMap.Keys) {
				Entity e = resultMap [id];
				Improbable.Collections.Option<IComponentData<Gatherable>> g = e.Get<Gatherable> ();
				GatherableData gatherable = g.Value.Get ().Value;

				if (gatherable.workType == WorkType.WORK_LOGGING)
					localTrees.Add (id);

			}
			plantedSinceLastRefresh = 0;
		}

		private void OnFailedEntityQuery (ICommandErrorDetails _) {
			Debug.LogWarning ("forester failed refresh, this may cause performance issues, but i'm gunna go ahead and retry...");
		}

		private ForesterJobAssignment OnGetJob(Nothing n, ICommandCallerInfo _) {
			// basically "if you need to replant or the thing is full so be proactive why dont ya"
			if (localTrees.Count + plantedSinceLastRefresh < minTrees) {
				plantedSinceLastRefresh++;
				return new ForesterJobAssignment (new Option<EntityId> (), new Option<Vector3d> (GetNewTreePlantPosition ()));
			} else {
				EntityId id = localTrees [0];
				localTrees.RemoveAt (0);
				return new ForesterJobAssignment (new Option<EntityId>(id), new Option<Vector3d> ());
			}
		}

		private Nothing OnCompleteJob(ForesterJobResult r, ICommandCallerInfo _) {
			if (r.assignment.chop.HasValue && AIAction.OnSuccess(r.result))
				GetComponent<BuildingController> ().PushItemGetNotification (0);
			return new Nothing ();
		}

		private Vector3d GetNewTreePlantPosition() {
			Vector3 v = transform.position + new Vector3 (DonutRandom(8f) + 4f, 0, DonutRandom(4f));
			return new Vector3d (v.x, v.y, v.z);
		}

		private float DonutRandom(float min) {
			float f = Random.Range (min, radius);
			if (Random.Range(0,2) == 0)
				f *= -1;
			return f;
		}

	}

}