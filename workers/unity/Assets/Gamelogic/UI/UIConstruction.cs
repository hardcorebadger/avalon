using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIConstruction : MonoBehaviour {

		public GameObject content;
		public GameObject linePrefab;
		public Text weightLabel;

		public void Load(ConstructionVisualizer construction) {
			// add max weight
			foreach (int id in construction.requirements.Keys) {
				GameObject line = Instantiate (linePrefab, content.transform);
				ConstructionController.Requirement r = construction.requirements [id];
				line.GetComponent<UIConstructionLine> ().SetInfo (
					Item.GetName (id),
					r.amount,
					r.required
				);
			}
		}
	}

}