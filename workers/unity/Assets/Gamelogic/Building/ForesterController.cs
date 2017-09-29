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
		private int incomingLogs = 0;
		private int currentLogs = 0;
		private bool treeDensitySatisfied = false;

		void OnEnable () {
			foresterWriter.CommandReceiver.OnGetJob.RegisterResponse (OnGetJob);
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

		private ForesterJobResponse OnGetJob(Nothing n, ICommandCallerInfo callerinfo) {
			// basically "if you need to replant or the thing is full so be proactive why dont ya"
			if ((localTrees.Count < minTrees || currentLogs+incomingLogs >= inventoryReader.Data.max)/* && !treeDensitySatisfied //need a way to make the workers go idle for this */) {
				return new ForesterJobResponse (new Improbable.Collections.Option<EntityId> ());
			} else {
				incomingLogs++;
				EntityId id = localTrees [0];
				localTrees.RemoveAt (0);
				return new ForesterJobResponse (new Improbable.Collections.Option<EntityId>(id));
			}
		}

		private void OnInventoryUpdate(Inventory.Update u) {
			if (u.inventory.HasValue) {
				// one of the incomming logs has arrived
				if (u.inventory.Value.Count - currentLogs > 0)
					incomingLogs--;
				// else basically someone removed logs to bring somewhere else

				currentLogs = u.inventory.Value.Count;
			}
		}

	}

}