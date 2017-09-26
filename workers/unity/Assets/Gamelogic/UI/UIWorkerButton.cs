using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIWorkerButton : MonoBehaviour {
		public void Disable() {
			GetComponent<Button> ().interactable = false;
			GetComponent<Image> ().color = Color.gray;
		}
	}

}