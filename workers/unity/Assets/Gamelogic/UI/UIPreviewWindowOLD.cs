using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {


	public class UIPreviewWindowOLD : MonoBehaviour {

		public UIInventory inventoryPrefab;
		public UIConstruction constructionPrefab;
		public Text title;

		private GameObject content;

		public void SetTitle(string t) {
			title.text = t;
		}

		public void LoadConstruction(ConstructionVisualizer construction) {
			ClearContent ();
			SetTitle ("Construction");

			content = Instantiate (constructionPrefab.gameObject, transform);
			content.GetComponent<UIConstruction> ().Load (construction);
		}

		public void LoadInventoryFromSelection() {
			int maxWeight;
			Dictionary<int,int> items = AggregateInventories(out maxWeight);

			ClearContent ();
			SetTitle ("Inventory");

			content = Instantiate (inventoryPrefab.gameObject, transform);
			content.GetComponent<UIInventory> ().Load (items, maxWeight);
		}

		public void ClearContent() {
			if (content != null) {
				Destroy (content);
				content = null;
			}
		}

		private Dictionary<int,int> AggregateInventories(out int maxWeight) {
			maxWeight = 0;
			Dictionary<int,int> result = new Dictionary<int, int> ();
			foreach (Selectable s in SelectionManager.instance.selected) {
				InventoryVisualizer inv = s.GetComponent<InventoryVisualizer> ();
				if (inv != null) {
					inv.AppendInventory (ref maxWeight, ref result);
				}
			}
			return result;
		}

	}

}