using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Assets.Gamelogic.Core {

	public class UIManager : MonoBehaviour {

		public static UIManager instance;

		public GameObject previewPrefab;
		public GameObject commandPickerPrefab;
		public GameObject toolbarWindowPrefab;
		private GameObject preview;

		public UIPreviewWidget[] previewWidgetList;
		public Dictionary<Type,UIPreviewWidget> previewWidgetsOptions;

		void OnEnable () {
			instance = this;
			previewWidgetsOptions = new Dictionary<Type,UIPreviewWidget> ();
			foreach (UIPreviewWidget w in previewWidgetList) {
				previewWidgetsOptions.Add (Type.GetType ("Assets.Gamelogic.Core."+w.type), w);
			}
		}

		void Update () {
			//depricated

//			if (Input.GetKeyDown(KeyCode.Space)) {
//				InterpretPreview ();
//			}
//			if (Input.GetKeyUp(KeyCode.Space)) {
//				if (preview != null) {
//					Destroy (preview);
//					preview = null;
//				}
//			}
		}

		public static void OpenCommandPicker(List<string> options) {
			Instantiate (instance.commandPickerPrefab, instance.transform).GetComponent<UICommandPicker> ().Load (options);
		}

		public static void OpenToolbarWindow(string title, List<string> options, UIToolbarWindow.OnSelectedDelegate d) {
			Instantiate (instance.toolbarWindowPrefab, instance.transform).GetComponent<UIToolbarWindow> ().Load (title, options, d);
		}

		public static void OpenPreview(GameObject o) {
			Instantiate (instance.previewPrefab, instance.transform).GetComponent<UIPreviewWindow> ().Load (o);
		}

		//depricated
		private void InterpretPreview() {
			if (SelectionManager.instance.selected.Count == 0)
				return;

			preview = Instantiate (previewPrefab, transform);

			foreach (Selectable s in SelectionManager.instance.selected) {
				if (s.GetComponent<ConstructionVisualizer> () != null) {
					// any building gets priority
					preview.GetComponent<UIPreviewWindowOLD> ().LoadConstruction(s.GetComponent<ConstructionVisualizer> ());
					return;
				}
			}

			// assume inventory
			preview.GetComponent<UIPreviewWindowOLD> ().LoadInventoryFromSelection ();
		}
	}

}