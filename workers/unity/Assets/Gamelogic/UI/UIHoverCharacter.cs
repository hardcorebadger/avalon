using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Core {

	public class UIHoverCharacter : UIHoverWidget {

		protected CharacterVisualizer character;
		public Slider healthSlider;
		public Slider hungerSlider;
		public OnUIChange callback;

		public override void Load(GameObject target) {
			base.Load (target);

			character = targetObject.GetComponent<CharacterVisualizer> ();
			healthSlider.maxValue = 100f;
			healthSlider.minValue = 0;
			healthSlider.value = character.GetHealth ();
			hungerSlider.maxValue = 100f;
			hungerSlider.minValue = 0;
			hungerSlider.value = 100f-character.GetHunger ();
			callback = OnChange;
			character.RegisterUIListener (callback);
		}

		public void OnDisable() {
			character.DeRegisterUIListener(callback);
		}

		public void OnChange() {
			healthSlider.value = character.GetHealth ();
			hungerSlider.value = 100f-character.GetHunger ();
		}

	
	}

}
