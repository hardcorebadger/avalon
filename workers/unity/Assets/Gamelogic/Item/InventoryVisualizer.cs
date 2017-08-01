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
		public int maxWeight = 0;

		// Use this for initialization
		void OnEnable () {
			if (inventoryReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			items = new Dictionary<int,int> ();
			UnwrapComponentInventory ();
			maxWeight = inventoryReader.Data.maxWeight;

			inventoryReader.ComponentUpdated.Add (OnInventoryUpdated);
		}

		private void OnInventoryUpdated(Inventory.Update update) {
			if (update.inventory.HasValue)
				UnwrapComponentInventory ();
			if (update.maxWeight.HasValue)
				maxWeight = update.maxWeight.Value;
		}

		private void UnwrapComponentInventory() {
			items.Clear ();
			foreach (int key in inventoryReader.Data.inventory.Keys) {
				int val = -1;
				inventoryReader.Data.inventory.TryGetValue (key, out val);
				items.Add (key, val);
			}
		}

		public void AppendInventory(ref int max, ref Dictionary<int,int> result) {
			foreach (int id in items.Keys) {
				int amount = 0;
				result.TryGetValue (id, out amount);
				result [id] = amount + items [id];
			}
			max  += maxWeight;
		}

		public void Log() {
			foreach (int key in items.Keys) {
				int val = items[key];
				Debug.Log (Item.GetName (key) + " " + val + " " + (Item.GetWeight (key) * val) + "lbs");
			}
		}
	}

}