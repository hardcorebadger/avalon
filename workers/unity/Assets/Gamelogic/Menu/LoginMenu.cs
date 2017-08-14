using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

namespace Assets.Gamelogic.Core {

	public class LoginMenu : MonoBehaviour {

		public InputField username;
		public InputField password;
		private bool loaded = false;
		string savedUsername, savedToken;
		private string passwordFiller = "*************";
		public PlayerData loggedInPlayer;
		public PlayerDataComponent dataObject;

		// Use this for initialization
		void Start () {
			dataObject = FindObjectOfType<PlayerDataComponent> ();

			if (PlayerPrefs.HasKey ("username")) {
				savedUsername = PlayerPrefs.GetString ("username");
				savedToken = PlayerPrefs.GetString ("token");
				username.text = savedUsername;
				password.text = passwordFiller;
				loaded = true;
			}
		}


		
		// Update is called once per frame
		void Update () {
			if (loaded && (username.text != savedUsername || password.text != passwordFiller)) {
				loaded = false;
				savedToken = "";
				savedUsername = "";
				password.text = "";
			}
		}

		public void OnLogin() {
			Debug.Log (username.text + " " + password.text);

			string encrypted = savedToken;
			if (!loaded)
				encrypted = ToSHA (password.text);
			if (username.text.Length <= 0) {
//				errorText.text = "You must enter a username!";
				return;
			}
			if (password.text.Length <= 0) {
//				errorText.text = "You must enter a password!";
				return;
			}

			WWWForm form = new WWWForm ();
			form.AddField ("username", username.text);
			if (!loaded)
				form.AddField ("password", encrypted);
			else 
				form.AddField ("token", encrypted);
//			form.AddField ("version", manager.version);

			WWW w = new WWW ("http://cdn.lilsumn.com/login.php", form);    
			StartCoroutine (LoginForm (w));

		}

		private IEnumerator LoginForm(WWW _w) {
			yield return _w; 

			if (_w.error == null) {
				Debug.Log (_w.text);
				if (_w.text.Contains ("!!BAD!!LOGIN!!")) {
					Debug.Log ("Bad Login!");
					DeleteUser ();
				} else {
					PlayerData player = JsonUtility.FromJson<PlayerData> (_w.text);
					Debug.Log (player.ToString ());
					if (player.status != 200) {
						//failed
						//					errorText.text = player.error;

						Debug.Log ("Bad Login!");
						DeleteUser ();
					} else {
						//connect

						dataObject.data = player;
						SaveUser (player);
						loggedInPlayer = player;
									SceneManager.LoadScene ("UnityClient");

						//					SceneManager.LoadScene ("main");
					}
				}
			} else {
				Debug.Log(_w.error);
//				errorText.text = "An Unknown Error Occured!";
				DeleteUser ();
				//php error
			}
		}

		private string ToSHA(string password) {
			SHA1 sha = System.Security.Cryptography.SHA1.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
			byte[] hash = sha.ComputeHash(inputBytes);

			string t = "";
			for (int i = 0; i < hash.Length; i++)
				t += string.Format("{0:x2}", hash[i]);
			return t;
		}

		private void SaveUser(PlayerData p) {
			PlayerPrefs.SetString ("username", p.username);
			PlayerPrefs.SetString ("token", p.token);
			PlayerPrefs.Save ();
		}

		private void DeleteUser() {
			if (PlayerPrefs.HasKey("username"))
				PlayerPrefs.DeleteKey ("username");
			if (PlayerPrefs.HasKey("token"))
				PlayerPrefs.DeleteKey ("token");
			PlayerPrefs.Save ();
		}

		[Serializable]
		public class PlayerData {
			public int id;
			public string username;
			public string email;
			public string password;
			public string token;
			public int red;
			public int green;
			public int blue;
			public float alpha;

			public int status;
			public int x;
			public int y;
			public string error;
			public override string ToString () {
				return string.Format ("[PlayerData: playerId={0}, username={1}, email={2}, password={3}, status={4}, error={5}, red={6}, token={7}]", id, username, email, password, status, error, red, token);
			}
		}



	}

}
