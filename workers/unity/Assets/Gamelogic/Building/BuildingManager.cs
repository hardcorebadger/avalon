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
		public static bool isBuilding = false;
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
			isBuilding = true;
			currentConstructionGhost = option;
			GameObject g = Instantiate (constructionOptions [option]);
			if (option == "Town Center") {
				g.GetComponent<ConstructionGhost> ().townOnly = false;
				g.GetComponent<ConstructionGhost> ().isTownCenter = true;
			}
			CreateTownRadialOverlays ();
		}

		public static void StopBuilding() {
			isBuilding = false;
			DestroyTownRadialOverlays ();
		}

		public static void Construct(Vector3 pos, GameObject townRadius) {

			if (townRadius != null)
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.ConstructTown.Descriptor, new ConstructionTownRequest(new Vector3d(pos.x,pos.y,pos.z),constructionOptions[currentConstructionGhost].name, townRadius.GetComponent<TownRadiusMarker>().townCenter.EntityId()), PlayerController.instance.gameObject.EntityId());
			else
				SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),constructionOptions[currentConstructionGhost].name), PlayerController.instance.gameObject.EntityId());

		}

		private static List<string> OptionsList() {
			List<string> l = new List<string> ();
			foreach (string s in constructionOptions.Keys) {
				l.Add (s);
			}
			return l;
		}

		private static void CreateTownRadialOverlays() {
			//find all town centers on your worker
			TownCenterVisualizer[] townCenters = FindObjectsOfType<TownCenterVisualizer>();
			foreach (TownCenterVisualizer t in townCenters) {
				//tell each to create a radius object 
				t.CreateRadiusMarker();
			}

		}

		private static void DestroyTownRadialOverlays() {
			//find all town center radius objects
			TownRadiusMarker[] radiuses = FindObjectsOfType<TownRadiusMarker>();
			foreach (TownRadiusMarker r in radiuses) {
				// destroy them
				Destroy(r.gameObject);
			}

		}

	}

	[System.Serializable]
	public struct ConstructionOption {
		public string name;
		public GameObject constructionPrefab;
	}

}