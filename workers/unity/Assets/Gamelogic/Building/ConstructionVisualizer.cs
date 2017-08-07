using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core
{

	public class ConstructionVisualizer : MonoBehaviour {

		[Require] private Construction.Reader constructionReader;

		public Dictionary<int,ConstructionController.Requirement> requirements;

		// Use this for initialization
		void OnEnable () {
			if (constructionReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			requirements = new Dictionary<int,ConstructionController.Requirement> ();
			UnwrapComponentRequirements ();
			constructionReader.ComponentUpdated.Add (OnConstructionUpdated);
		}

		// Update is called once per frame
		void OnDisable () {

		}

		private void OnConstructionUpdated(Construction.Update update) {
			if (update.requirements.HasValue) {
				requirements.Clear ();
				UnwrapComponentRequirements ();
			}
		}

		private void UnwrapComponentRequirements() {
			foreach (int key in constructionReader.Data.requirements.Keys) {
				ConstructionRequirement val = constructionReader.Data.requirements[key];
				requirements.Add (key, new ConstructionController.Requirement(val.amount, val.required));
			}
		}

		public void Log() {
			foreach (int key in requirements.Keys) {
				ConstructionController.Requirement val = requirements[key];
				Debug.Log(Item.GetName (key) + ": " + val.amount + " / " + val.required);
			}
		}
	}

}