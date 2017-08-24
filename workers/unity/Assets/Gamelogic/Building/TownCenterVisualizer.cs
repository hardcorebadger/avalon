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

	public class TownCenterVisualizer : MonoBehaviour {

		public float radius = 5f;
		public GameObject townRadiusMarker;

		[Require] private TownCenter.Reader townCenterReader;

		public void CreateRadiusMarker() {
			GameObject g = Instantiate (townRadiusMarker, transform.position, Quaternion.identity);
			g.transform.localScale = new Vector3 (townCenterReader.Data.radius, townCenterReader.Data.radius);
			g.GetComponent<TownRadiusMarker> ().townCenter = gameObject;
		}
			
	}

}
