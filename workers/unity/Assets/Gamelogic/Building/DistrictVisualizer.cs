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

		Map<EntityId, JobInfoOption> characters;

		private void OnEnable() {
			if (districtReader.HasAuthority) {
				enabled = false;
				return;
			}
			districtReader.ShowCedeTriggered.Add (OnCede);
			districtReader.ComponentUpdated.Add (OnDistrictUpdate);
			characters = districtReader.Data.characterMap;
		}

		private void OnDisable() {
			districtReader.ShowCedeTriggered.Remove (OnCede);
		}

		private void OnCede(Nothing _) {
			GetComponent<AudioSource> ().Play ();
		}

		private void OnDistrictUpdate(District.Update u) {
			if (u.characterMap.HasValue)
				characters = u.characterMap.Value;
		}

		public List<EntityId> GetIdleCharacters() {
			List<EntityId> chars = new List<EntityId> ();
			foreach (EntityId id in characters.Keys) {
				if (!characters [id].jobInfo.HasValue)
					chars.Add (id);
			}
			return chars;
		}
	}

}