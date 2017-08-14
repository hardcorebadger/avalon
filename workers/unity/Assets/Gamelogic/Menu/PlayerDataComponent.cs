using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class PlayerDataComponent : MonoBehaviour {

		public LoginMenu.PlayerData data;

		// Use this for initialization
		void Start () {
			
		}

		void Awake() {
			DontDestroyOnLoad (this);
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}

}