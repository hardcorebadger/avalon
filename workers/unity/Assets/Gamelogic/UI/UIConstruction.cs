using UnityEngine;
using UnityEngine.UI;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {

	public class UIConstruction : UIPreviewWidget {

		public GameObject content;
		public GameObject linePrefab;
		public Text total;

		public override void Load(UIPreviewWindow window, GameObject target) {
			base.Load (window, target);
			ConstructionVisualizer cons = target.GetComponent<ConstructionVisualizer> ();
			Load (cons.requirements);
		}

		public void Load(Map<int,ConstructionRequirement> items) {
			int totalNum = 0;
				
			foreach (int id in items.Keys) {
				GameObject line = Instantiate (linePrefab, content.transform);
				line.GetComponent<UIConstructionLine> ().SetInfo (
					id,
					items [id]
				);
				totalNum += items [id].required;
			}
			total.text = totalNum + " ";

		}

	}

}