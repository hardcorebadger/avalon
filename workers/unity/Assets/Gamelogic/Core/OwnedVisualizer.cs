using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {
	

	public class OwnedVisualizer : MonoBehaviour {

		[Require] private Owned.Reader ownedReader;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public int GetOwnerId() {
			return ownedReader.Data.owner;
		}


		public Color GetOwnerColor() {
			return new Color (Bootstrap.players [ownedReader.Data.owner].red, Bootstrap.players [ownedReader.Data.owner].green, Bootstrap.players [ownedReader.Data.owner].blue);
		}

	}

}