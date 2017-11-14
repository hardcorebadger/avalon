using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Gamelogic.Core {

	public class UIChat : MonoBehaviour {

		public InputField input;
		public Text textPrefab;
		public bool isActive = false;

		public void setInputEnabled(bool e) {
			if (!e)
				input.text = "";

			isActive = e;
			input.gameObject.SetActive(e);
			if (e) {
				input.transform.SetAsLastSibling ();			
				input.ActivateInputField ();
			}
		}

		public void displayMessage(string s) {
			GameObject g = Instantiate (textPrefab.gameObject);
			Text t = g.GetComponent<Text> ();
			t.transform.SetParent (transform, false);
			t.text = s;
			input.transform.SetAsLastSibling ();
		}

		public void onInputSubmitted() {
			CommandCenter.SendChat (input.text);
	
			displayMessage("<color=#"+Bootstrap.players[Bootstrap.playerId].htmlColor+">" + Bootstrap.players[Bootstrap.playerId].username + "</color>: " + input.text);
			setInputEnabled(false);
		}

		private void Start() {
			setInputEnabled (false);
		}
	}

}