using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {
	public class GatherableController : MonoBehaviour {

		[Require] private Gatherable.Writer gatherableWriter;
		// Use this for initialization
		void Start () {
		
			InventoryController c = GetComponent<InventoryController> ();
			c.Insert (1, 5);
		
		}	
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}
