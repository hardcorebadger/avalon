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

		public ConstructionOption[] options;
		public static Dictionary<string, GameObject> constructionOptions;

		public static BuildingManager instance;
		private static string currentConstructionGhost;

		public void OnEnable() {
			constructionOptions = new Dictionary<string, GameObject> ();
			foreach (ConstructionOption c in options) {
				constructionOptions.Add (c.name, c.constructionPrefab);
			}
		}

		public static void Initialize() {
			
		}

		public void OnBuildButton() {
			UIManager.OpenToolbarWindow ("Build", OptionsList(), StartBuilding);
		}

		public static void StartBuilding(string option) {
			currentConstructionGhost = option;
			Instantiate (constructionOptions [option]);
		}

		public static void Construct(Vector3 pos) {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),constructionOptions[currentConstructionGhost].name), PlayerController.instance.gameObject.EntityId());
		}

		private static List<string> OptionsList() {
			List<string> l = new List<string> ();
			foreach (string s in constructionOptions.Keys) {
				l.Add (s);
			}
			return l;
		}

	}

	[System.Serializable]
	public struct ConstructionOption {
		public string name;
		public GameObject constructionPrefab;
	}

}