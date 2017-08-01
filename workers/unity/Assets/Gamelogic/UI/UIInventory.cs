using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIInventory : MonoBehaviour {

		public GameObject content;
		public GameObject linePrefab;
		public Text weightLabel;

		public void Load(Dictionary<int,int> items, int maxWeight) {
			// add max weight
			int weight = 0;
			foreach (int id in items.Keys) {
				GameObject line = Instantiate (linePrefab, content.transform);
				int w = Item.GetWeight (id) * items [id];
				weight += w;
				line.GetComponent<UIInventoryLine> ().SetInfo (
					Item.GetName (id),
					items [id],
					w
				);
			}
			weightLabel.text = weight + "/" + maxWeight + " lbs";
				
		}
	}

}