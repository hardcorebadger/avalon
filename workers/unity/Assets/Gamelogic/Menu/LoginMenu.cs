using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.Gamelogic.Core {

	public class LoginMenu : MonoBehaviour {

		public InputField username;
		public InputField password;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void OnLogin() {
			Debug.Log (username.text + " " + password.text);
			SceneManager.LoadScene ("UnityClient");
		}

	}

}
