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
		[Require] private Inventory.Reader inventoryReader;

		public delegate void DistrictUpdateDelegate();

		Map<EntityId, JobInfoOption> characters;
		List<DistrictUpdateDelegate> updateListeners; 

		private void OnEnable() {
			if (districtReader.HasAuthority) {
				enabled = false;
				return;
			}
			updateListeners = new List<DistrictUpdateDelegate> ();
			districtReader.ShowCedeTriggered.Add (OnCede);
			districtReader.ComponentUpdated.Add (OnDistrictUpdate);
			inventoryReader.ComponentUpdated.Add (OnInventoryUpdate);
			characters = districtReader.Data.characterMap;
			FindObjectOfType<UINavbar> ().OnDistrictEnabled (this);

		}

		private void OnDisable() {
			if (districtReader.HasAuthority)
				return;
			districtReader.ShowCedeTriggered.Remove (OnCede);
			FindObjectOfType<UINavbar> ().OnDistrictDisabled (this);
		}

		private void OnCede(Nothing _) {
			GetComponent<AudioSource> ().Play ();
		}

		private void OnDistrictUpdate(District.Update u) {
			if (u.characterMap.HasValue)
				characters = u.characterMap.Value;

			foreach (DistrictUpdateDelegate d in updateListeners)
				d ();
		}

		private void OnInventoryUpdate(Inventory.Update u) {
			foreach (DistrictUpdateDelegate d in updateListeners)
				d ();
		}

		public void AddListener(DistrictUpdateDelegate d) {
			updateListeners.Add (d);
		}

		public void RemoveListener(DistrictUpdateDelegate d) {
			updateListeners.Remove (d);
		}

		public List<EntityId> GetIdleCharacters() {
			List<EntityId> chars = new List<EntityId> ();
			foreach (EntityId id in characters.Keys) {
				if (!characters [id].jobInfo.HasValue)
					chars.Add (id);
			}
			return chars;
		}

		public int GetItemAmount(int id) {
			return GetComponent<InventoryVisualizer>().GetAmount(id);
		}

		public int GetItemTrend(int id) {
			if (!districtReader.Data.itemTrends.ContainsKey(id))
				return 0;
			return districtReader.Data.itemTrends [id].currentTrend;
		}

		public int GetCharacterAmount() {
			return characters.Count;
		}

		public int GetBedAmount() {
			return districtReader.Data.beds;
		}
	}

}