using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class Hoverable : MonoBehaviour {

		public bool hovered = false;
		private GameObject hoverContainer;
		public Vector3 offset;

		public void Update() {
			if (hoverContainer != null) {
				hoverContainer.transform.position = Camera.main.WorldToScreenPoint (transform.position+offset);
			}
		}

		public void SetHovered(bool b) {
			
			if (b && !hovered)
				StartHover ();
			else if (!b && hovered)
				EndHover ();

			hovered = b;
		}

		private void StartHover() {
			hoverContainer = Instantiate (UIManager.instance.hoverContainer, UIManager.instance.transform);

			foreach (MonoBehaviour m in GetComponents<MonoBehaviour>()) {
				if (UIManager.instance.hoverWidgetsOptions.ContainsKey (m.GetType()))
					AddWidget (UIManager.instance.hoverWidgetsOptions [m.GetType()]);
			}
		}

		private void EndHover() {
			Destroy (hoverContainer);
			hoverContainer = null;
		}

		private void AddWidget(UIHoverWidget w) {
			Instantiate (w, hoverContainer.transform).GetComponent<UIHoverWidget> ().Load (gameObject);
		}

	}

}