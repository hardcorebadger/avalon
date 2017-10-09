using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class UIHoverWidget : MonoBehaviour {
		
		public string type;
		protected GameObject targetObject;

		public virtual void Load(GameObject target) {
			targetObject = target;
		}

	}

}
