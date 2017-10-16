using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIHoverCharacter : UIHoverWidget {

		protected CharacterVisualizer character;
		public Slider slider;
		public OnUIChange callback;

		public override void Load(GameObject target) {
			base.Load (target);

			character = targetObject.GetComponent<CharacterVisualizer> ();
			slider.maxValue = 100f;
			slider.minValue = 0;
			slider.value = character.getHealth ();
			callback = OnChange;
			character.RegisterUIListener (callback);
		}

		public void OnDisable() {
			character.DeRegisterUIListener(callback);
		}

		public void OnChange() {
			slider.value = character.getHealth ();

		}

	
	}

}
