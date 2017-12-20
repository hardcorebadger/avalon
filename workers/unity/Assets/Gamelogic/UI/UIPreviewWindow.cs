using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Assets.Gamelogic.Core {

	public class UIPreviewWindow : MonoBehaviour {

		public GameObject widgetContainer;
		public Text title;

		private GameObject target;

		public void Load(GameObject g) {
			target = g;

			foreach (MonoBehaviour m in target.GetComponents<MonoBehaviour>()) {
				if (UIManager.instance.previewWidgetsOptions.ContainsKey (m.GetType()))
					AddWidget (UIManager.instance.previewWidgetsOptions [m.GetType()]);
			}
			StartCoroutine (Redraw());
		}

		// I know you're thinking - woah - wtf is this about - well turns out responsive 
		// design is easier in HTML than in Unity - so just forget about it.
		private IEnumerator Redraw() {
			yield return null;
			LayoutRebuilder.MarkLayoutForRebuild (widgetContainer.transform as RectTransform);
		}

		private void AddWidget(UIPreviewWidget w) {
			Instantiate (w, widgetContainer.transform).GetComponent<UIPreviewWidget> ().Load (this, target);
		}

		private void Update() {
			if (Input.GetKeyDown (KeyCode.Escape))
				Close ();
		}

		public void Close() {
			Destroy (gameObject);
		}

	}

}