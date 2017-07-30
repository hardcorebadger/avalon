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

	public class InventoryController : MonoBehaviour {

		[Require] private Inventory.Writer inventoryWriter;
		private Dictionary<int,int> items;
		private int maxWeight = 0;

		// Use this for initialization
		void OnEnable () {
			items = new Dictionary<int,int> ();
			UnwrapComponentInventory ();
			maxWeight = inventoryWriter.Data.maxWeight;
		}

		private void UnwrapComponentInventory() {
			foreach (int key in inventoryWriter.Data.inventory.Keys) {
				int val = inventoryWriter.Data.inventory[key];
				items.Add (key, val);
			}
		}

		private Improbable.Collections.Map<int,int> WrapComponentInventory() {
			Improbable.Collections.Map<int,int> wrapped = new Improbable.Collections.Map<int,int> ();
			foreach (int key in items.Keys) {
				int val = items[key];
				wrapped.Add (key, val);
			}
			return wrapped;
		}

		private void SendInventoryUpdate() {
			inventoryWriter.Send (new Inventory.Update ()
				.SetInventory (WrapComponentInventory())
			);
		}

		public void Log() {
			foreach (int key in items.Keys) {
				int val = items[key];
				Debug.Log (Item.GetName (key) + " " + val + " " + (Item.GetWeight (key) * val) + "lbs");
			}
		}

		public bool Insert(int id, int amount) {
			int weight = Item.GetWeight (id) * amount;
			if (weight + GetWeight () > maxWeight)
				return false;

			int val = 0;
			items.TryGetValue (id, out val);
			val += amount;
			items [id] = val;

			SendInventoryUpdate ();
			return true;
		}

		public void Clear() {
			items.Clear ();
			SendInventoryUpdate ();
		}

		public int Count(int i) {
			int amount = 0;
			items.TryGetValue (i, out amount);
			return amount;
		}

		public bool Drop(int i, int n) {
			int amount = 0;
			items.TryGetValue (i, out amount);
			amount -= n;
			if (amount < 0)
				return false;
			
			if (amount == 0)
				items.Remove (i);
			else
				items [i] = amount;

			SendInventoryUpdate ();
			return true;
		}

		public int GetWeight() {
			int weight = 0;
			foreach (int id in items.Keys) {
				weight += Item.GetWeight (id) * items[id];
			}
			return weight;
		}
	}

}