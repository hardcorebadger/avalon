using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {
	
	public class UICommandPicker : MonoBehaviour {

		public GameObject content;
		public GameObject tableButtonPrefab;

		public void Load(List<string> options) {
			foreach (string o in options) {
				GameObject button = Instantiate (tableButtonPrefab, content.transform);
				button.GetComponentInChildren<Text> ().text = o;
				button.GetComponent<Button>().onClick.AddListener(delegate() { OnButtonPress(o); });
			}
		}

		private void OnButtonPress(string option) {
			CommandCenter.OnCommandSelected (option);
			Destroy (gameObject);
		}

		public void OnExit() {
			CommandCenter.OnCommandCancelled ();
			Destroy (gameObject);
		}

	}

}
