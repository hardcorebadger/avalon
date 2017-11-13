using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Gamelogic.Core
{

	public class ConstructionVisualizer : MonoBehaviour {

		[Require] private Construction.Reader constructionReader;

		private OwnedVisualizer owned;
		public Map<int,ConstructionRequirement> requirements;
		public GameObject fireParticles;
		private System.Collections.Generic.List<OnUIChange> listeners;
		private bool wasDestroyed = false;

		public float completion = 0.0f;

		// Use this for initialization
		void OnEnable () {
			if (constructionReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			owned = GetComponent<OwnedVisualizer> ();
			requirements = constructionReader.Data.requirements;
			constructionReader.ComponentUpdated.Add (OnConstructionUpdated);
			listeners = new System.Collections.Generic.List<OnUIChange> ();
			wasDestroyed = constructionReader.Data.wasDestroyed;
			if (wasDestroyed) {
				CreateFire ();
			}
		}

		// Update is called once per frame
		void OnDisable () {

		}

		private void OnConstructionUpdated(Construction.Update update) {
			if (update.requirements.HasValue) {
				requirements = update.requirements.Value;
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

		public void Log() {
			foreach (int key in requirements.Keys) {
				ConstructionRequirement val = requirements[key];
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

		private void CreateFire() {
			Transform t = transform.Find ("fire");
			foreach (Transform t1 in t) {
				Instantiate (fireParticles, t1);
			}
		}

	}

}