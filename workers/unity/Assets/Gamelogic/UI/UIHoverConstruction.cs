using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIHoverConstruction : UIHoverWidget {

		protected ConstructionVisualizer construction;
		public Slider slider;
		public OnUIChange callback;

		public override void Load(GameObject target) {
			base.Load (target);

			construction = targetObject.GetComponent<ConstructionVisualizer> ();
			slider.maxValue = 100f;
			slider.minValue = 0;
			slider.value = construction.getProgress () * 100;
			callback = OnChange;
			construction.RegisterUIListener (callback);
		}

		public void OnDisable() {
			construction.DeRegisterUIListener(callback);
		}

		public void OnChange() {
			slider.value = construction.getProgress () * 100;

		}


	}

}
