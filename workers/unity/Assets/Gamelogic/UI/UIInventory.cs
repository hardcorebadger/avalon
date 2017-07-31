using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIInventory : MonoBehaviour {

		public GameObject content;
		public GameObject linePrefab;

		public void Load(Dictionary<int,int> items, int maxWeight) {
			// add max weight
			foreach (int id in items.Keys) {
				GameObject line = Instantiate (linePrefab, content.transform);
				line.GetComponent<UIInventoryLine> ().SetInfo (
					Item.GetName (id),
					items [id],
					Item.GetWeight (id) * items [id]
				);
			}
		}
	}

}