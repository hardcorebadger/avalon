using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class Hoverable : MonoBehaviour {

		public HoverState hoverState = HoverState.None;
		private GameObject hoverContainer;
		public Vector3 offset;

		public Material hoverMaterial;
		private Material defaultMaterial;

		public void Update() {
			if (hoverContainer != null) {
				hoverContainer.transform.position = Camera.main.WorldToScreenPoint (transform.position+offset);
			}
		}

		public void SetHovered(HoverState s) {

			if (s == HoverState.None) {
				DestroyHoverTag ();
				DestroyHoverEffects ();
			} else if (s == HoverState.Hovered) {
				CreateHoverTag ();
				CreateHoverEffects ();
			} else if (s == HoverState.Selected) {
				DestroyHoverEffects ();
				CreateHoverTag ();
			}

			hoverState = s;
		}

		private void CreateHoverEffects() {
			SpriteRenderer sr = GetComponent<SpriteRenderer> ();
			if (sr != null) {
				sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, 0.5f);
			}
			MeshRenderer mr = GetComponentInChildren<MeshRenderer> ();
			if (mr != null) {
				defaultMaterial = mr.material;
				mr.material = hoverMaterial;
			}
		}

		private void DestroyHoverEffects() {
			SpriteRenderer sr = GetComponent<SpriteRenderer> ();
			if (sr != null) {
				sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, 1f);
			}
			MeshRenderer mr = GetComponentInChildren<MeshRenderer> ();
			if (mr != null) {
				mr.material = defaultMaterial;
			}
		}

		private void CreateHoverTag() {
			if (hoverContainer != null)
				return;
			
			hoverContainer = Instantiate (UIManager.instance.hoverContainer, UIManager.instance.transform);

			foreach (MonoBehaviour m in GetComponents<MonoBehaviour>()) {
				if (UIManager.instance.hoverWidgetsOptions.ContainsKey (m.GetType()))
					AddWidget (UIManager.instance.hoverWidgetsOptions [m.GetType()]);
			}
		}

		private void DestroyHoverTag() {
			if (hoverContainer != null)
				Destroy (hoverContainer);
			hoverContainer = null;
		}

		private void AddWidget(UIHoverWidget w) {
			Instantiate (w, hoverContainer.transform).GetComponent<UIHoverWidget> ().Load (gameObject);
		}

		public enum HoverState {
			Hovered,
			Selected,
			None
		}

	}

}