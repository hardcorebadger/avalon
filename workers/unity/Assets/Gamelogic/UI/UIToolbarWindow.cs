using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIToolbarWindow : MonoBehaviour {

		public delegate void OnSelectedDelegate(string option);

		public GameObject content;
		public GameObject tableButtonPrefab;
		public Text title;
		private OnSelectedDelegate deli;

		public void Load(string t, List<string> options, OnSelectedDelegate d) {
			title.text = t;
			deli = d;
			foreach (string o in options) {
				GameObject button = Instantiate (tableButtonPrefab, content.transform);
				button.GetComponentInChildren<Text> ().text = o;
				button.GetComponent<Button>().onClick.AddListener(delegate() { OnButtonPress(o); });
			}
		}

		private void OnButtonPress(string option) {
			deli (option);
			Destroy (gameObject);
		}

		public void OnExit() {
			Destroy (gameObject);
		}
	}

}