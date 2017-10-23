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
		public GameObject tile;

		public static BuildingManager instance;
		public static bool isBuilding = false;
		private static string currentConstructionGhost;

		public void OnEnable() {
			instance = this;
			options = new Dictionary<string, ConstructionInfo> ();
			options.Add ("house-3d", new ConstructionInfo(1,1, true));
			options.Add ("forester", new ConstructionInfo(3,1, true));
			options.Add ("quarry", new ConstructionInfo(2,2, true));
			options.Add ("farm", new ConstructionInfo(2,2, true));
			options.Add ("stockpile", new ConstructionInfo(3,1, true));
			options.Add ("settlement", new ConstructionInfo(4,4, false));
			options.Add ("road", new ConstructionInfo(1,1, true));

		}

		public void OnBuildButton() {
			UIManager.OpenToolbarWindow ("Build", GetOptionsList(), StartBuilding);
		}

		public static void StartBuilding(string option) {
			isBuilding = true;
			currentConstructionGhost = option;
			CreateTiles ();
			GameObject o = Instantiate (instance.ghost);
			o.GetComponent<ConstructionGhost> ().SetInfo (options [option]);
		}

		public static void StopBuilding() {
			isBuilding = false;
			ClearTiles ();
		}

		public static void Construct(Vector3 pos, EntityId id) {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),currentConstructionGhost, new Improbable.Collections.Option<EntityId>(id)), PlayerController.instance.gameObject.EntityId());
		}

		public static void Construct(Vector3 pos) {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),currentConstructionGhost, new Improbable.Collections.Option<EntityId>()), PlayerController.instance.gameObject.EntityId());
		}

		private List<string> GetOptionsList() {
			List<string> s = new List<string> ();
			foreach (string o in options.Keys) {
				s.Add (o);
			}
			return s;
		}

		private static void CreateTiles() {
			BuildingVisualizer[] buildings = FindObjectsOfType<BuildingVisualizer> ();
			foreach (BuildingVisualizer building in buildings) {
				if (!building.district.HasValue) {
					Debug.Log ("this");
					continue;
				}
				for (int z = -1 * building.tileMargin; z < building.zWidth + building.tileMargin; z++) {
					for (int x = -1 * building.tileMargin; x < building.xWidth + building.tileMargin; x++) {
						// relative position to block locked bottom tile on building
						Vector3 pos = building.transform.position + new Vector3(x * 8, 0f, z * 8);
						GameObject g = GameObject.Instantiate (instance.tile, pos, Quaternion.identity);
						g.GetComponent<ConstructionTile> ().SetDistrict (building.district.Value);
					}
				}
			}
		}

		private static void ClearTiles() {
			ConstructionTile[] con = FindObjectsOfType<ConstructionTile> ();
			foreach (ConstructionTile c in con) {
				Destroy (c.gameObject);
			}
		}

		[System.Serializable]
		public struct ConstructionInfo {
			public int xWidth, zWidth;
			public bool requiresDistrict;
			public ConstructionInfo(int x, int z, bool d) {
				xWidth = x;
				zWidth = z;
				requiresDistrict = d;
			}
		}

	}


}