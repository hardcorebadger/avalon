using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {

	public class BuildingVisualizer : MonoBehaviour {

		[Require] private Building.Reader buildingReader;

		private OwnedVisualizer owned;

		public Option<EntityId> district;

		public int tileMargin = 1;
		public int xWidth = 1;
		public int zWidth = 1;
		public Transform itemGetParticles;
		public GameObject flag;

		// Use this for initialization
		void OnEnable () {
			if (buildingReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			owned = GetComponent<OwnedVisualizer> ();
			tileMargin = buildingReader.Data.tileMargin;
			xWidth = buildingReader.Data.xWidth;
			zWidth = buildingReader.Data.zWidth;
			district = buildingReader.Data.district;
			buildingReader.ComponentUpdated.Add (OnBuildingUpdate);
			buildingReader.ShowItemGetTriggered.Add (ShowItemGetTriggered);
			if (flag != null) {
				flag.GetComponent<SpriteRenderer> ().color = owned.GetOwnerColor ();
			}
		}

		private void OnBuildingUpdate(Building.Update u) {
			if (u.district.HasValue) {
				district = u.district.Value;
			}
		}

		private void ShowItemGetTriggered(ShowItemGetEvent e) {
			PushItemGetNotification (e.id);
		}

		// Update is called once per frame
		void OnDisable () {

		}

		public bool CanControl() {
			return owned.GetOwnerId() == Bootstrap.playerId;
		}

		public void PushItemGetNotification(int id) {
			// optional feature
			if (itemGetParticles == null)
				return;

			itemGetParticles.Find ("item-" + id).GetComponent<ParticleSystem> ().Play ();

		}
	
	}

}