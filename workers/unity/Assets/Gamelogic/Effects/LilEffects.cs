using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core
{

	public class LilEffects : MonoBehaviour {

		private Animator anim;

		void OnEnable() {
			anim = GetComponent<Animator> ();
		}

		void OnMouseDown() { 
			anim.SetTrigger ("click");
		}

	}

}