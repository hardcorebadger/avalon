using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {
		
	public class UINavResourceStat : MonoBehaviour {

		private Text amount;
		private Text trend;

		void OnEnable() {
			GetTextComponents ();
		}

		void GetTextComponents() {
			amount = transform.Find ("amount").GetComponent<Text> ();
			trend = transform.Find ("trend").GetComponent<Text> ();
		}

		public void SetValues(int a, int t) {
			if (amount == null)
				GetTextComponents ();
			
			if (t > 0) {
				trend.color = new Color(0.4118f,0.7608f,0.5765f);
				trend.text = "+" + t;
			} else if (t < 0) {
				trend.color = new Color(0.9176f,0.2824f,0.4235f);
				trend.text = t + "";
			} else {
				trend.color = new Color(0.9f, 0.9f, 0.9f);
				trend.text = t + "";
			}
				
			amount.text = a + "";
		}
		
	}

}