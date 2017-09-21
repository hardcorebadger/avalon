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

		public static Dictionary<string, ConstructionInfo> options;
		public GameObject ghost;

		public static BuildingManager instance;
		public static bool isBuilding = false;
		private static string currentConstructionGhost;

		public void OnEnable() {
			instance = this;
			options = new Dictionary<string, ConstructionInfo> ();
			options.Add ("house-3d", new ConstructionInfo(1,1));
			options.Add ("forester", new ConstructionInfo(3,1));
			options.Add ("quarry", new ConstructionInfo(2,2));
			options.Add ("farm", new ConstructionInfo(1,1));
		}

		public void OnBuildButton() {
			UIManager.OpenToolbarWindow ("Build", GetOptionsList(), StartBuilding);
		}

		public static void StartBuilding(string option) {
			isBuilding = true;
			currentConstructionGhost = option;
			GameObject o = Instantiate (instance.ghost);
			o.GetComponent<ConstructionGhost> ().SetSize (options [option].xWidth, options [option].zWidth);
		}

		public static void StopBuilding() {
			isBuilding = false;
		}

		public static void Construct(Vector3 pos) {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),currentConstructionGhost), PlayerController.instance.gameObject.EntityId());
		}

		private List<string> GetOptionsList() {
			List<string> s = new List<string> ();
			foreach (string o in options.Keys) {
				s.Add (o);
			}
			return s;
		}

		[System.Serializable]
		public struct ConstructionInfo {
			public int xWidth, zWidth;
			public ConstructionInfo(int x, int z) {
				xWidth = x;
				zWidth = z;
			}
		}

	}


}