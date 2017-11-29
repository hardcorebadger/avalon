using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core
{

	public class LilEffects : MonoBehaviour {

		public AudioClip[] clickSounds;

		private Animator anim;
		private AudioSource audioSrc;

		void OnEnable() {
			anim = GetComponent<Animator> ();
			audioSrc = GetComponent<AudioSource> ();
		}

		void PlayMouseDownEffect() {
			if (anim != null)
			anim.SetTrigger ("click");
			if (audioSrc != null && clickSounds.Length > 0)
				audioSrc.PlayOneShot(clickSounds[Random.Range(0,clickSounds.Length-1)]);
		}

		void OnMouseOver () {
			if(Input.GetMouseButtonDown(1)) {
				PlayMouseDownEffect ();
			}
		}

		void OnMouseDown () {
			PlayMouseDownEffect ();
		}
			

	}

}