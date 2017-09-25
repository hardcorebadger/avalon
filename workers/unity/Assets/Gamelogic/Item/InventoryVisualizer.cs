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

	public class InventoryVisualizer : MonoBehaviour {

		[Require] private Inventory.Reader inventoryReader;
		public Dictionary<int,int> items;
		public int maxItems = 0;

		// Use this for initialization
		void OnEnable () {
			if (inventoryReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			items = new Dictionary<int,int> ();
			UnwrapComponentInventory ();
			maxItems = inventoryReader.Data.max;

			inventoryReader.ComponentUpdated.Add (OnInventoryUpdated);
		}

		private void OnInventoryUpdated(Inventory.Update update) {
			if (update.inventory.HasValue)
				UnwrapComponentInventory ();
			if (update.max.HasValue)
				maxItems = update.max.Value;
		}

		private void UnwrapComponentInventory() {
			items.Clear ();
			foreach (int key in inventoryReader.Data.inventory.Keys) {
				int val = -1;
				inventoryReader.Data.inventory.TryGetValue (key, out val);
				items.Add (key, val);
			}
		}

		public void Log() {
			foreach (int key in items.Keys) {
				int val = items[key];
				Debug.Log (Item.GetName (key) + " " + val);
			}
		}
	}

}