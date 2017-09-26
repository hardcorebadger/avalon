using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIWorkers : UIPreviewWidget {

		public GameObject content;
		public GameObject buttonPrefab;

		GameObject[] buttons;
		int currentMax = -1;

		public override void Load(UIPreviewWindow window, GameObject target) {
			base.Load (window, target);
			WorkSiteVisualizer workSite = target.GetComponent<WorkSiteVisualizer> ();
			buttons = new GameObject[workSite.maxWorkers];
			currentMax = workSite.workers - 1;
			for (int i = 0; i < workSite.maxWorkers; i++) {
				GameObject btn = Instantiate (buttonPrefab, content.transform);
				btn.GetComponent<Button> ().onClick.AddListener (delegate{ButtonPressed (btn);});
				buttons [i] = btn;
				if (i >= workSite.workers)
					btn.GetComponent<UIWorkerButton> ().Disable ();
			}
		}

		public void ButtonPressed(GameObject btn) {
			buttons [currentMax].GetComponent<UIWorkerButton> ().Disable ();

			currentMax--;
		}

	}

}