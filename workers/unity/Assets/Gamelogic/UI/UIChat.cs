using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Linq;

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
			t.text = "";

			//remove unwanted formatters from unity rich text (Size (can spam very large text) and Bold (ugly))
			s = s.Replace ("</size>", "");
			s = s.Replace ("<size=", "");
			s = s.Replace ("<b>", "");
			s = s.Replace ("</b>", "");

			t.text = s;
			input.transform.SetAsLastSibling ();
		}

		public void displayNotification(string s) {
			GameObject g = Instantiate (textPrefab.gameObject);
			Text t = g.GetComponent<Text> ();
			t.transform.SetParent (transform, false);
			t.text = "";
			//regex takes out all matches for a number within double brackets [[x]]
			Regex reg = new Regex (@"\[(\[(\d+)\])\]");
			Match match = reg.Match(s);
			while (match.Success) {
				string groupString = (match.Groups[0].Value); //[[x]]
				string justNumbers = new string(groupString.Where(char.IsDigit).ToArray()); //x
				int playerId = int.Parse (justNumbers);//int32 x
				s = s.Replace (groupString, "<color=#" + Bootstrap.players [playerId].htmlColor + ">" + Bootstrap.players [playerId].username + "</color>");
				match = match.NextMatch();

			}
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