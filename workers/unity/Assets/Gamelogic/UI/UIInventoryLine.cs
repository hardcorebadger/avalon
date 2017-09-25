using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIInventoryLine : MonoBehaviour {

		public Image icon;
		public Slider fill;
		public Text fillLabel;

		public void SetInfo(int i, int a, int t) {
			icon.sprite = Item.GetIcon (i);
			fill.maxValue = t;
			fill.value = a;
			fillLabel.text = a + "/" + t;
		}

	}

}