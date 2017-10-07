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

	public class BuildingVisualizer : MonoBehaviour {

		[Require] private Building.Reader buildingReader;

		private OwnedVisualizer owned;

		// Use this for initialization
		void OnEnable () {
			if (buildingReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			owned = GetComponent<OwnedVisualizer> ();
		}

		// Update is called once per frame
		void OnDisable () {

		}

		public bool CanControl() {
			return owned.GetOwnerId() == Bootstrap.playerId;
		}
	
	}

}