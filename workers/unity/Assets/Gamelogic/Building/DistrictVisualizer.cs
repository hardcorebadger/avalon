using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using System.Linq;

namespace Assets.Gamelogic.Core {
	
	public class DistrictVisualizer : MonoBehaviour {

		[Require] private District.Reader districtReader;

		private void OnEnable() {
			districtReader.ShowCedeTriggered.Add (OnCede);
		}

		private void OnDisable() {
			districtReader.ShowCedeTriggered.Remove (OnCede);
		}

		private void OnCede(Nothing _) {
			GetComponent<AudioSource> ().Play ();
		}
	}

}