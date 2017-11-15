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
		public float coOpRadius = 50f;
		private GameObject preview;

		public UIPreviewWidget[] previewWidgetList;
		public Dictionary<Type,UIPreviewWidget> previewWidgetsOptions;

		public UIHoverWidget[] hoverWidgetList;
		public UIHoverWidget hoverTagWidget;
		public Dictionary<Type,UIHoverWidget> hoverWidgetsOptions;

		public UIChat chat;

		public GameObject hoverContainer;

		void OnEnable () {
			instance = this;
			previewWidgetsOptions = new Dictionary<Type,UIPreviewWidget> ();
			foreach (UIPreviewWidget w in previewWidgetList) {
				previewWidgetsOptions.Add (Type.GetType ("Assets.Gamelogic.Core."+w.type), w);
			}

			hoverWidgetsOptions = new Dictionary<Type,UIHoverWidget> ();
			foreach (UIHoverWidget w in hoverWidgetList) {
				hoverWidgetsOptions.Add (Type.GetType ("Assets.Gamelogic.Core."+w.type), w);
			}
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

		public static void DisplayMessage(string message, int sender) { 
				
			if (sender == -1)
				instance.chat.displayMessage (message);
			else
				instance.chat.displayMessage ("<color=#"+Bootstrap.players [sender].htmlColor+">" + Bootstrap.players [sender].username + "</color>: " + message);


		}
			
		public static void DisplayNotification(string message) { 

			instance.chat.displayNotification (message);
	

		}


	}

}