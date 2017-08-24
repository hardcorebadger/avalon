using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class TownRadiusMarker : MonoBehaviour {

		public GameObject townCenter;
		public Color activeColor;
		public Color restrictedColor;

		private Color inactiveColor;
		private SpriteRenderer sprite;

		// Use this for initialization
		void Start () {
			sprite = GetComponent<SpriteRenderer> ();
			inactiveColor = new Color(sprite.color.r,sprite.color.g,sprite.color.b,sprite.color.a) ;
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void SetMarker(int i) {
			if (i == 1) {
				sprite.color = activeColor;
			} else if (i == 0) {
				sprite.color = inactiveColor;
			} else if (i == -1) {
				sprite.color = restrictedColor;
			}
		}
	}

}