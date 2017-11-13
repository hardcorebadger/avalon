using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class UIConstructionLine : MonoBehaviour {

		public Image icon;
		public Slider fill;
		public Text fillLabel;

		public void SetInfo(int i, ConstructionRequirement c) {
			icon.sprite = Item.GetIcon (i);
			fill.maxValue = c.required;
			fill.value = c.amount;
			fillLabel.text = c.amount + "/" + c.required;
		}

	}

}