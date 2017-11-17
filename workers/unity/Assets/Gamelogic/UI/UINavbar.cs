using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UINavbar : MonoBehaviour {

		public UINavResourceStat resourceStat1;
		public UINavResourceStat resourceStat2;
		public UINavResourceStat resourceStat3;
		public Text characterStat;
		public Image characterImage;
		 
		private List<DistrictVisualizer> prePlayerInitializedDistricts;
		private List<DistrictVisualizer> loadedDistricts;
		private DistrictVisualizer currentDistrict;

		private void OnEnable() {
			loadedDistricts = new List<DistrictVisualizer> ();
			prePlayerInitializedDistricts = new List<DistrictVisualizer> ();
			Refresh ();
		}

		public void OnPlayerInitialized() {
			foreach (DistrictVisualizer d in prePlayerInitializedDistricts) {
				OnDistrictEnabled (d);
			}
			prePlayerInitializedDistricts.Clear ();
		}

		public void OnDistrictEnabled(DistrictVisualizer d) {
			if (Bootstrap.playerObject == null) {
				prePlayerInitializedDistricts.Add (d);
			} else {
				loadedDistricts.Add (d);
				RefreshClosestDistrict ();
			}
		}

		public void OnDistrictDisabled(DistrictVisualizer d) {
			loadedDistricts.Remove (d);
			RefreshClosestDistrict ();
		}

		private void RefreshClosestDistrict () {
			if (loadedDistricts.Count < 1) {
				currentDistrict = null;
			} else {
				DistrictVisualizer closestDistrict = loadedDistricts [0];
				float min = DistanceTo (loadedDistricts [0]);
	
				foreach (DistrictVisualizer d in loadedDistricts) {
					float dist = DistanceTo (d);
					if (dist < min) {
						min = dist;
						closestDistrict = d;
					}
				}
				SetDistrict (closestDistrict);
			}

			Refresh ();
		}

		private void SetDistrict(DistrictVisualizer d) {
			if (currentDistrict != null)
				currentDistrict.RemoveListener (Refresh);
			currentDistrict = d;
			currentDistrict.AddListener (Refresh);
		}

		private void Refresh () {
			if (currentDistrict == null) {
				ClearStats ();
			} else {
				resourceStat1.SetValues (currentDistrict.GetItemAmount(0), currentDistrict.GetItemTrend(0));
				resourceStat2.SetValues (currentDistrict.GetItemAmount(1), currentDistrict.GetItemTrend(1));
				resourceStat3.SetValues (currentDistrict.GetItemAmount(2), currentDistrict.GetItemTrend(2));
				characterStat.text = currentDistrict.GetCharacterAmount () + " / " + currentDistrict.GetBedAmount ();
				characterImage.color = currentDistrict.GetComponent<OwnedVisualizer>().GetOwnerColor();
			}
		}

		private void ClearStats() {
			resourceStat1.SetValues (0, 0);
			resourceStat2.SetValues (0, 0);
			resourceStat3.SetValues (0, 0);
			characterStat.text = "-";
		}

		private float DistanceTo(DistrictVisualizer d) {
			return Vector3.Distance (Bootstrap.playerObject.transform.position, d.transform.position); 
		}

		// set character color correctly
		// load the closest district
		// keep live updates

	}

}