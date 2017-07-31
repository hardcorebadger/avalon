using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class UIManager : MonoBehaviour {

		public GameObject previewPrefab;
		private GameObject preview;

		void Update () {
			if (Input.GetKeyDown(KeyCode.Space)) {
				preview = Instantiate (previewPrefab, transform);
				int max;
				Dictionary<int,int> items = AggregateInventories (out max);
				preview.GetComponent<UIInventory> ().Load (items, max);
			}
			if (Input.GetKeyUp(KeyCode.Space)) {
				Destroy (preview);
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