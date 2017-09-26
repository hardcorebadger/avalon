using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Gamelogic.Core {

	public class UIWorkerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

		public GameObject xImage;
		public Color enabledColor;

		private GameObject currentX;
		private Color disabledColor;

		private void OnEnable() {
			disabledColor = GetComponent<Image> ().color;
		}

		public void Enable() {
			GetComponent<Button> ().interactable = true;
			GetComponent<Image> ().color = enabledColor;
		}

		public void Disable() {
			GetComponent<Button> ().interactable = false;
			GetComponent<Image> ().color = disabledColor;
			if (currentX != null) {
				Destroy (currentX);
				currentX = null;
			}
		}

		public void OnPointerEnter(PointerEventData eventData) {
			if (GetComponent<Button> ().interactable) {
				currentX = Instantiate (xImage, transform);
			}
		}

		public void OnPointerExit(PointerEventData eventData) {
			if (currentX != null) {
				Destroy (currentX);
				currentX = null;
			}
		}

	}

}