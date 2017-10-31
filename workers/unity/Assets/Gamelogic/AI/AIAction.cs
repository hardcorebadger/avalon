using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public abstract class AIAction {

		public CharacterController agent;
		public string name;

		protected int state = 0;
		protected int shouldRespond = 100;

		public AIAction(CharacterController o, string n) {
			agent = o;
			name = n;
		}

		public abstract int Update();

		public virtual void OnKill () {}

		public virtual void OnDealHit() {}

		// response codes
		// 100 = working
		// 200 = success
		// 400 = user error
		// 500 = server error

		public static bool OnTermination(int responseCode) {
			return responseCode >= 200;
		}

		public static bool OnSuccess(int responseCode) {
			return responseCode >= 200 && responseCode < 300;
		}

		public static bool OnUserError(int responseCode) {
			return responseCode >= 400 && responseCode < 500;
		}

		public static bool OnServerError(int responseCode) {
			return responseCode >= 500 && responseCode < 600;
		}
			
	}

}