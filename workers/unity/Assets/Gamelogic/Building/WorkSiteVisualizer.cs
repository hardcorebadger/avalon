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

		public int workers = 0;
		public int maxWorkers = 0;

		public GameObject[] characterVisualizers;
		private int currentCharViz = 0;

		void OnEnable () {
			if (workSiteReader.HasAuthority)
				return;
			workSiteReader.ComponentUpdated.Add(OnWorkSiteUpdated);
			workers = workSiteReader.Data.workers.Count + workSiteReader.Data.inside.Count;
			maxWorkers = workSiteReader.Data.maxWorkers;
			Color c = GetOwnerColor();
			foreach (GameObject g in characterVisualizers) {
				g.GetComponent<SpriteRenderer> ().color = c;
			}
			RefreshCharacterViz ();
		}

		public Color GetOwnerColor() {
			PlayerColor c = Bootstrap.players [ownedReader.Data.owner];
			return new Color (c.red, c.green, c.blue);
		}

		private void RefreshCharacterViz() {
			foreach (GameObject g in characterVisualizers) {
				g.SetActive (false);
			}
			currentCharViz = 0;
			for (int i = 0; i < workSiteReader.Data.inside.Count; i++) {
				if (characterVisualizers.Length > i) {
					characterVisualizers [currentCharViz].SetActive (true);
					currentCharViz++;
				}
			}
		}

		void OnDisable () {
			workSiteReader.ComponentUpdated.Remove(OnWorkSiteUpdated);
		}
		
		void OnWorkSiteUpdated (WorkSite.Update update) {
			if (workSiteReader.HasAuthority)
				return;
			if (update.workers.HasValue && update.inside.HasValue) {
				workers = update.workers.Value.Count + update.inside.Value.Count;
				RefreshCharacterViz ();
			} else 
			if (update.workers.HasValue) {
				workers = update.workers.Value.Count + workSiteReader.Data.inside.Count;
				RefreshCharacterViz ();
			} else
			if (update.inside.HasValue) {
				workers = update.inside.Value.Count + workSiteReader.Data.workers.Count;
				RefreshCharacterViz ();
			}
			if (update.maxWorkers.HasValue) {
				maxWorkers = update.maxWorkers.Value; 
			}
		}
	}

}