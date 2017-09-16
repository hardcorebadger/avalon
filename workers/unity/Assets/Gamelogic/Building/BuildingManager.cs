using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class BuildingManager : MonoBehaviour {

		public static List<string> options;
		public GameObject ghost;

		public static BuildingManager instance;
		public static bool isBuilding = false;
		private static string currentConstructionGhost;

		public void OnEnable() {
			instance = this;
			options = new List<string> ();
			options.Add ("house-3d");
		}

		public void OnBuildButton() {
			UIManager.OpenToolbarWindow ("Build", options, StartBuilding);
		}

		public static void StartBuilding(string option) {
			isBuilding = true;
			currentConstructionGhost = option;
			Instantiate (instance.ghost);
		}

		public static void StopBuilding() {
			isBuilding = false;
		}

		public static void Construct(Vector3 pos) {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),currentConstructionGhost), PlayerController.instance.gameObject.EntityId());
		}

	}

	[System.Serializable]
	public struct ConstructionOption {
		public string name;
		public GameObject constructionPrefab;
	}

}