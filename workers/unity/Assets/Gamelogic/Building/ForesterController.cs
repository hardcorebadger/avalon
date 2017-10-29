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
		[Require] private Inventory.Reader inventoryReader;
		public float localTreeRefreshRate = 60f;
		// doesnt work yet
		public int maxTrees = 100;
		public int minTrees = 10;
		private float timer = -1f;
		private List<EntityId> localTrees;
		private int currentLogs = 0;
		private bool treeDensitySatisfied = false;
		private bool notAcceptingItems = false;

		void OnEnable () {
			foresterWriter.CommandReceiver.OnGetJob.RegisterResponse (OnGetJob);
			foresterWriter.CommandReceiver.OnCompleteJob.RegisterResponse (OnCompleteJob);
			inventoryReader.ComponentUpdated.Add (OnInventoryUpdate);
			RefreshLocalTrees ();
			currentLogs = InventoryController.GetTotal (inventoryReader.Data);
		}
		
		void OnDisable () {
			foresterWriter.CommandReceiver.OnGetJob.DeregisterResponse ();
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
			var entityQuery = Query.And (Query.HasComponent<Gatherable> (), Query.InSphere (transform.position.x, transform.position.y,transform.position.z,100)).ReturnComponents (Gatherable.ComponentId);
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

			treeDensitySatisfied = localTrees.Count > maxTrees;
		}

		private void OnFailedEntityQuery (ICommandErrorDetails _) {
			Debug.LogWarning ("forester failed refresh, this may cause performance issues, but i'm gunna go ahead and retry...");
		}

		private ForesterJobAssignment OnGetJob(Nothing n, ICommandCallerInfo _) {
			// basically "if you need to replant or the thing is full so be proactive why dont ya"
			if (localTrees.Count < minTrees || currentLogs >= inventoryReader.Data.max) {
				if (!treeDensitySatisfied)
					return new ForesterJobAssignment (new Option<EntityId> (), new Option<Vector3d> (GetNewTreePlantPosition ()));
				else {
					Debug.LogWarning (localTrees.Count + " " + minTrees + " " + maxTrees + " " + treeDensitySatisfied);
					return new ForesterJobAssignment (new Option<EntityId> (), new Option<Vector3d> ());
				}
			} else {
				EntityId id = localTrees [0];
				localTrees.RemoveAt (0);
				return new ForesterJobAssignment (new Option<EntityId>(id), new Option<Vector3d> ());
			}
		}

		private Nothing OnCompleteJob(ForesterJobResult r, ICommandCallerInfo _) {
			return new Nothing ();
		}

		private void OnInventoryUpdate(Inventory.Update u) {
			if (u.inventory.HasValue) {

				if (!u.inventory.Value.ContainsKey (0)) {
					currentLogs = 0;
					return;
				}
				currentLogs = u.inventory.Value[0];
			}
		}

		private Vector3d GetNewTreePlantPosition() {
			Vector3 v = transform.position + new Vector3 (DonutRandom(), 0, DonutRandom());
			return new Vector3d (v.x, v.y, v.z);
		}

		private float DonutRandom() {
			float f = Random.Range (7f, 100f);
			if (Random.Range(0,2) == 0)
				f *= -1;
			return f;
		}

	}

}