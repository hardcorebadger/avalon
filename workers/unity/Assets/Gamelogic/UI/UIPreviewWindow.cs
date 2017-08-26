using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Assets.Gamelogic.Core {

	public class UIPreviewWindow : MonoBehaviour {

		public GameObject dotContainer;
		public GameObject widgetContainer;
		public GameObject dotPrefab;
		public Color activeDot;
		public Color inactiveDot;
		public Text title;

		private GameObject target;
		private Dictionary<GameObject,UIPreviewWidget> dots;

		public void Load(GameObject g) {
			target = g;

			foreach (MonoBehaviour m in target.GetComponents<MonoBehaviour>()) {
				if (UIManager.instance.previewWidgetsOptions.ContainsKey (m.GetType()))
					AddWidget (UIManager.instance.previewWidgetsOptions [m.GetType()]);
			}

			// shit-tarded way of getting the first value
			foreach (GameObject d in dots.Keys) {
				OpenWidget (d);
				break;
			}
		}

		private void OpenWidget(GameObject dot) {
			foreach (GameObject d in dots.Keys) {
				d.GetComponent<Image> ().color = inactiveDot;
			}
			dot.GetComponent<Image> ().color = activeDot;
			foreach (Transform t in widgetContainer.transform) {
				Destroy (t.gameObject);
			}
			Instantiate (dots [dot], widgetContainer.transform).GetComponent<UIPreviewWidget> ().Load (this, target);
		}

		private void AddWidget(UIPreviewWidget w) {
			// create a dot which when clicked opens the widget
			GameObject dot = Instantiate(dotPrefab,dotContainer.transform);
			dot.GetComponent<Button> ().onClick.AddListener (delegate{OpenWidget (dot);});
			dots.Add (dot,w);
		}

		private void OnEnable() {
			dots = new Dictionary<GameObject,UIPreviewWidget> ();
		}

		private void Update() {
			if (Input.GetKeyDown (KeyCode.Escape))
				Destroy (gameObject);
		}

	}

}