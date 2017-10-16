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

		private OwnedVisualizer owned;
		public Dictionary<int,ConstructionController.Requirement> requirements;
		private System.Collections.Generic.List<OnUIChange> listeners;

		public float completion = 0.0f;

		// Use this for initialization
		void OnEnable () {
			if (constructionReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			owned = GetComponent<OwnedVisualizer> ();
			requirements = new Dictionary<int,ConstructionController.Requirement> ();
			UnwrapComponentRequirements ();
			constructionReader.ComponentUpdated.Add (OnConstructionUpdated);
			listeners = new System.Collections.Generic.List<OnUIChange> ();

		}

		// Update is called once per frame
		void OnDisable () {

		}

		private void OnConstructionUpdated(Construction.Update update) {
			if (update.requirements.HasValue) {
				requirements.Clear ();
				UnwrapComponentRequirements ();
				int total = requirements.Count;
				float percentage = 0;
				foreach(var item in requirements) {
					percentage += ((float)item.Value.amount / (float)item.Value.required);
				}
				completion = percentage / total;
			}
			foreach (OnUIChange c in listeners) {
				c ();
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

		public bool CanControl() {
			return owned.GetOwnerId() == Bootstrap.playerId;
		}

		public void RegisterUIListener(OnUIChange c) {

			listeners.Add(c);
		}

		public void DeRegisterUIListener(OnUIChange c) {
			listeners.Remove(c);

		}

		public float getProgress() {
			return completion;
		}

	}

}