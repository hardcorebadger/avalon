﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class Selectable : MonoBehaviour {

		public Color tagColor;
		public string typeName;

		private cakeslice.Outline outline;
		private GameObject tagObject;
		private bool engaged;

		// Use this for initialization
		void Start () {
			outline = GetComponent<cakeslice.Outline>();
			outline.color = 2;
		}

		void OnMouseEnter () {
			SetHighlighted (true);
			tagObject = Instantiate (SelectionManager.instance.tagPrefab,SelectionManager.instance.canvas.transform);
			tagObject.GetComponent<Image> ().color = tagColor;
			tagObject.GetComponent<HoverTag> ().SetText (typeName);
			tagObject.GetComponent<RectTransform> ().position = Camera.main.WorldToScreenPoint (transform.position);
		}

		void OnMouseExit () {
			if (!SelectionManager.instance.IsSelected (this)) {
				SetHighlighted (false);
			}
			Destroy (tagObject);
		}

		public bool IsSelectable() {
			return !engaged;
		}

		public void SetEngaged(bool e) {
			engaged = e;
			SetOutlineColor (OutlineColor.Red);
		}

		public bool IsEngaged() {
			return engaged;
		}

		public void SetHighlighted(bool h) {
			if (engaged)
				return;
			
			if (h)
				SetOutlineColor (OutlineColor.Blue);
			else
				SetOutlineColor (OutlineColor.None);
		}

		private void SetOutlineColor(OutlineColor c) {
			if (c == OutlineColor.Blue)
				outline.color = 0;
			else if (c == OutlineColor.Red)
				outline.color = 1;
			else if (c == OutlineColor.None)
				outline.color = 2;
		}
	}


	public enum OutlineColor {
		None,
		Blue,
		Red,
	}

}
