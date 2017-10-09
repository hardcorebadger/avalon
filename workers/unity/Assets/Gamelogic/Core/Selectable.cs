using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class Selectable : MonoBehaviour {

//		public Color tagColor;
//		public string typeName;
//
//		private cakeslice.Outline outline;
//		private GameObject tagObject;
//
//		// Use this for initialization
//		void Start () {
//			outline = GetComponent<cakeslice.Outline>();
//			outline.color = 2;
//			outline.enabled = false;
//		}
//
//		void OnMouseEnter () {
//			GetComponent<Hoverable> ().SetHovered (true);
//			SetHighlighted (true);
//			tagObject = Instantiate (SelectionManager.instance.tagPrefab,SelectionManager.instance.canvas.transform);
//			tagObject.GetComponent<Image> ().color = tagColor;
//			tagObject.GetComponent<HoverTag> ().SetText (typeName);
//			tagObject.GetComponent<RectTransform> ().position = Camera.main.WorldToScreenPoint (transform.position);
//		}
//
//		void OnMouseExit () {
//			GetComponent<Hoverable> ().SetHovered (false);
//			if (!SelectionManager.instance.IsSelected (this)) {
//				SetHighlighted (false);
//			}
//			Destroy (tagObject);
//		}
//
//		public bool IsSelectable() {
//			return true;
//		}
//			
//		public void SetHighlighted(bool h) {
//			if (h)
//				SetOutlineColor (OutlineColor.Blue);
//			else
//				SetOutlineColor (OutlineColor.None);
//		}
//
//		private void SetOutlineColor(OutlineColor c) {
//			if (outline != null) {
//				if (c == OutlineColor.Blue) {
//					outline.enabled = true;
//					outline.color = 0;
//				} else if (c == OutlineColor.Red) {
//					outline.enabled = true;
//					outline.color = 1;
//				} else if (c == OutlineColor.None)
//					outline.enabled = false;
//			}
//		}
	}


//	public enum OutlineColor {
//		None,
//		Blue,
//		Red,
//	}

}
