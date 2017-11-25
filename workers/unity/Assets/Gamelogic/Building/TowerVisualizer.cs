using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Worker;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class TowerVisualizer : MonoBehaviour {

		[Require] private Tower.Reader towerReader;
		public GameObject[] archeryParticles;

		void OnEnable() {
			towerReader.ComponentUpdated.Add (OnTowerUpdated);
		}

		void OnDisable() {
			towerReader.ComponentUpdated.Remove (OnTowerUpdated);
		}

		private void OnTowerUpdated(Tower.Update u) {
			if (u.archeryState.HasValue) {
				SetArchery (u.archeryState.Value == 1);
			}
		}

		private void SetArchery(bool b) {
			foreach (GameObject g in archeryParticles) {
				g.SetActive (b);
			}
		}

	}

}
