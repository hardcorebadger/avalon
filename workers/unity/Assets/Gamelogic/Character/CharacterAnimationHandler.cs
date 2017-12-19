using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {
	
	public class CharacterAnimationHandler : MonoBehaviour {
		public void OnFootstep() {
			GetComponentInParent<CharacterVisualizer> ().OnFootstep ();
		}
		public void OnDealHit() {
			GetComponentInParent<CharacterVisualizer> ().OnDealHit ();
		}
	}

}