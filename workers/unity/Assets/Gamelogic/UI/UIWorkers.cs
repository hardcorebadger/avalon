using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Entity.Component;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;

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
				if (i < workSite.workers)
					btn.GetComponent<UIWorkerButton> ().Enable ();
			}
		}

		public void ButtonPressed(GameObject btn) {
			buttons [currentMax].GetComponent<UIWorkerButton> ().Disable ();
			currentMax--;
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, WorkSite.Commands.FireWorker.Descriptor, new FireWorkerRequest (), targetObject.EntityId())
				.OnSuccess(response => OnFireResult (response))
				.OnFailure(response => OnFailure());

		}

		private void OnFireResult(FireWorkerResponse r) {
			if (!r.success) {
				currentMax++;
				buttons [currentMax].GetComponent<UIWorkerButton> ().Enable ();
			}
		}

		private void OnFailure() {
			currentMax++;
			buttons [currentMax].GetComponent<UIWorkerButton> ().Enable ();
		}

	}

}