using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIInventory : UIPreviewWidget {

		public GameObject content;
		public GameObject linePrefab;
		public Text total;

		public override void Load(UIPreviewWindow window, GameObject target) {
			base.Load (window, target);
			InventoryVisualizer inv = target.GetComponent<InventoryVisualizer> ();
			Load (inv.items, inv.maxItems);
			total.text = inv.maxItems + "";
		}

		public void Load(Dictionary<int,int> items, int maxItems) {
			foreach (int id in items.Keys) {
				GameObject line = Instantiate (linePrefab, content.transform);
				line.GetComponent<UIInventoryLine> ().SetInfo (
					id,
					items [id],
					maxItems
				);
			}
		}

	}

}