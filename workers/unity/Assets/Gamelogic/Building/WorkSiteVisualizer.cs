using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class WorkSiteVisualizer : MonoBehaviour {

		[Require] private WorkSite.Reader workSiteReader;
		[Require] private Owned.Reader ownedReader;

		public int maxWorkers = 0;
		public Color ownerColor;

		private List<EntityId> workers;

		void OnEnable () {
			if (workSiteReader.HasAuthority)
				return;
			workSiteReader.ComponentUpdated.Add(OnWorkSiteUpdated);
			maxWorkers = workSiteReader.Data.maxWorkers;
			workers = workSiteReader.Data.workers;

			ownerColor = GetOwnerColor ();
		}

		public Color GetOwnerColor() {
			PlayerColor c = Bootstrap.players [ownedReader.Data.owner];
			return new Color (c.red, c.green, c.blue);
		}

		void OnDisable () {
			workSiteReader.ComponentUpdated.Remove(OnWorkSiteUpdated);
		}
		
		void OnWorkSiteUpdated (WorkSite.Update update) {
			if (workSiteReader.HasAuthority)
				return;
			if (update.workers.HasValue) {
				workers = update.workers.Value;
			}
			if (update.maxWorkers.HasValue) {
				maxWorkers = update.maxWorkers.Value; 
			}
		}

		public int GetWorkerCount() {
			return workers.Count;
		}

		public List<EntityId> GetWorkers() {
			return new List<EntityId>(workers);
		}
	}

}