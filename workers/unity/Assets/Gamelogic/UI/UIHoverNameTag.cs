using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Gamelogic.Core {

	public class UIHoverNameTag : UIHoverWidget {

		public Text label;

		public override void Load(GameObject target) {
			base.Load (target);
			label.text = target.GetComponent<Hoverable> ().hoverLabel;
		}

		public void SetLabel(string s) {
			label.text = s;
		}


	}

}