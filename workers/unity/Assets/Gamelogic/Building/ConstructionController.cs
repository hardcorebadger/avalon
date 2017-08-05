using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {
	
	public class ConstructionController : MonoBehaviour {

		[Require] private Construction.Writer constructionWriter;

		private Dictionary<int,Requirement> requirements;

		// Use this for initialization
		void OnEnable () {
			
		}
		
		// Update is called once per frame
		void OnDisable () {
			
		}

		public struct Requirement {
			int amount;
			int required;
		}
	}
		
}