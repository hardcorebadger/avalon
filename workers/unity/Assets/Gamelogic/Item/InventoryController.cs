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

			inventoryWriter.CommandReceiver.OnGive.RegisterResponse(OnGive);
			inventoryWriter.CommandReceiver.OnGiveMultiple.RegisterResponse(OnGiveMultiple);

			inventoryWriter.CommandReceiver.OnTake.RegisterResponse(OnTake);
			inventoryWriter.CommandReceiver.OnTakeMultiple.RegisterResponse(OnTakeMultiple);

			items = new Dictionary<int,int> ();
			UnwrapComponentInventory ();
			maxWeight = inventoryWriter.Data.maxWeight;
		}

		void OnDisable() {
			inventoryWriter.CommandReceiver.OnGive.DeregisterResponse();
			inventoryWriter.CommandReceiver.OnGiveMultiple.DeregisterResponse();
			inventoryWriter.CommandReceiver.OnTake.DeregisterResponse();
			inventoryWriter.CommandReceiver.OnTakeMultiple.DeregisterResponse();
		}

		private GiveResponse OnGive(ItemStack itemStack, ICommandCallerInfo callerinfo) {
			return new GiveResponse (Insert(itemStack.id,itemStack.amount));
		}

		private GiveResponse OnGiveMultiple(ItemStackList itemStackList, ICommandCallerInfo callerinfo) {
			return new GiveResponse (Insert(ToDictionary(itemStackList)));
		}

		private TakeResponse OnTake(ItemStack itemStack, ICommandCallerInfo callerinfo) {
			return new TakeResponse (Drop(itemStack.id,itemStack.amount));
		}

		private TakeResponse OnTakeMultiple(ItemStackList itemStackList, ICommandCallerInfo callerinfo) {
			return new TakeResponse (Drop(ToDictionary(itemStackList)));
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
				Debug.Log(Item.GetName (key) + " " + val + " " + (Item.GetWeight (key) * val) + "lbs");
			}
		}

		public bool Insert(Dictionary<int,int> insert) {
			int w = 0;
			foreach (int id in insert.Keys) {
				w += Item.GetWeight (id) * insert [id];
			}
			if (GetWeight () + w > maxWeight)
				return false;
			foreach (int id in insert.Keys) {
				Insert (id, insert [id]);
			}
			return true;
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

		public int GetAvailableWeight() {
			return maxWeight - GetWeight ();
		}

		public bool Drop(Dictionary<int,int> drops) {
			foreach (int id in drops.Keys) {
				int amount = 0;
				items.TryGetValue (id, out amount);
				amount -= drops[id];
				if (amount < 0)
					return false;
			}
			foreach (int id in drops.Keys) {
				Drop(id,drops[id]);
			}
			return true;
		}

		public bool CanHold(int id, int amount) {
			return (GetWeight () + Item.GetWeight (id) * amount <= maxWeight);
		}

		public ItemStackList GetItemStackList() {
			return ToItemStackList(items);
		}

		public Dictionary<int,int> GetConstructionOverlap(ConstructionData construction) {
			Dictionary<int,int> o = new Dictionary<int,int> ();
			foreach (int i in construction.requirements.Keys) {
				ConstructionRequirement r = construction.requirements [i];
				if (Count (i) > 0 && r.required - r.amount > 0) {
					if (r.required - r.amount < Count (i))
						o.Add (i, r.required - r.amount);
					else
						o.Add (i, Count (i));
				}
			}
			return o;
		}

		public bool CanStoreSomething(StorageData s, InventoryData sInv) {
			foreach (int id in items.Keys) {
				if (s.types.Contains (Item.GetType (id))) {
					if (CanHold (sInv, id, 1))
						return true;
				}
			}
			return false;
		}

		public ItemStackList GetStorableItems(StorageData s, InventoryData sInv) {
			Improbable.Collections.Map<int, int> l = new Improbable.Collections.Map<int, int> ();
			foreach (int id in items.Keys) {
				if (s.types.Contains (Item.GetType (id))) {
					int a = TestFill (sInv, id, items [id]);
					if (a > 0)
						l.Add (id, a);
				}
			}
			return new ItemStackList (l);
		}

		public static ItemStackList ToItemStackList(Dictionary<int,int> d) {
			ItemStackList l = new ItemStackList (new Improbable.Collections.Map<int, int>());
			foreach (int id in d.Keys) {
				l.inventory.Add (id, d [id]);
			}
			return l;
		}

		public static Dictionary<int,int> ToDictionary(ItemStackList d) {
			Dictionary<int,int> o = new Dictionary<int,int> ();
			foreach (int i in d.inventory.Keys) {
				o.Add (i, d.inventory [i]);
			}
			return o;
		}

		public static Dictionary<int,int> GetOverlap(Dictionary<int,int> inv1, Dictionary<int,int> inv2) {
			Dictionary<int,int> o = new Dictionary<int,int> ();
			foreach (int i in inv1.Keys) {
				if (inv2.ContainsValue(i) && inv2[i] > 0)
					o.Add(i, Mathf.Min(inv1[i],inv2[i]));
			}
			return o;
		}

		public static Dictionary<int,int> GetCappedOverlap(Improbable.Collections.Map<int,int> inv1, Dictionary<int,int> inv2, int weight) {
			Dictionary<int,int> o = new Dictionary<int,int> ();
			foreach (int i in inv1.Keys) {
				if (inv2.ContainsKey (i) && inv2 [i] > 0) {
					int amount = Mathf.Min (inv1 [i], inv2 [i]);
					int cap = weight / Item.GetWeight (i);
					if (cap > 0 && amount > cap) {
						o.Add (i, weight / Item.GetWeight (i));
					} else if (cap > 0) {
						o.Add (i, amount);
					} else if (cap == 0) {
					}
				}
			}
			return o;
		}

		public static int TestFill(InventoryData inv, int id, int amount) {
			int available = inv.maxWeight - GetWeight (inv);
			int max = available / Item.GetWeight (id);
			amount = Mathf.Min (max, amount);
			int cur = 0;
			inv.inventory.TryGetValue (id, out cur);
			inv.inventory[id] = cur + amount;
			return amount;
		}

		public static bool CanHold(InventoryData inv, int id, int amount) {
			return (Item.GetWeight (id) * amount + GetWeight (inv) <= inv.maxWeight);
		}

		public static int GetWeight(InventoryData inv) {
			int w = 0;
			foreach (int i in inv.inventory.Keys) {
				w += Item.GetWeight (i) * inv.inventory [i];
			}
			return w;
		}
	}

}