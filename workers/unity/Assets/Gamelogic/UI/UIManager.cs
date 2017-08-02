using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class UIManager : MonoBehaviour {

		public static UIManager instance;

		public GameObject previewPrefab;
		public GameObject commandPickerPrefab;
		private GameObject preview;

		void OnEnable () {
			instance = this;
		}

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

		public static void OpenCommandPicker(List<string> options) {
			Instantiate (instance.commandPickerPrefab, instance.transform).GetComponent<UICommandPicker> ().Load (options);
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