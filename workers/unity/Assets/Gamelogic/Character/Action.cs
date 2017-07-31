using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class Action {

		public delegate void OnCompleteDelegate(Action a);

		public CharacterController owner;
		public OnCompleteDelegate onComplete;
		public Action chained;

		/*
			 * 
			 * Public Interface
			 * 
			 */ 

		public Action(CharacterController o, OnCompleteDelegate oc) {
			owner = o;
			onComplete = oc;
		}

		public virtual void Update () {
			kill();
		}

		public virtual void kill() {
			owner.StopAction (this);

			if (chained != null)
				owner.StartAction (chained);

			if (onComplete != null)
				onComplete(this);

		}


		public virtual Action chain (Action a) {
			chained = a;
			return this;
		}

	}

}