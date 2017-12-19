using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class BuildingManager : MonoBehaviour {

		public static Map<string, ConstructionInfo> options;
		public GameObject ghost;
		public GameObject tile;

		public static BuildingManager instance;
		public static bool isBuilding = false;
		private static string currentConstructionGhost;
		private static GameObject currentConstructionGhostObject;

		private static List<GameObject> currentTiles;

		public void OnEnable() {
			instance = this;
			options = new Map<string, ConstructionInfo> ();
			options.Add ("house-3d", new ConstructionInfo(1,1, true, true));
			options.Add ("forester", new ConstructionInfo(2,1, true, true));
			options.Add ("quarry", new ConstructionInfo(2,2, true, true));
			options.Add ("farm", new ConstructionInfo(2,2, true, true));
			options.Add ("stockpile", new ConstructionInfo(3,1, true, true));
			options.Add ("settlement", new ConstructionInfo(3,3, false, false));
			options.Add ("road", new ConstructionInfo(1,1, true, true));
			options.Add ("wall", new ConstructionInfo(1,1, true, true));
			options.Add ("tower", new ConstructionInfo(1,1, true, true));
		}

		public void OnBuildButton() {
			UIManager.OpenToolbarWindow ("Build", GetOptionsList(), StartBuilding);
		}

		public static void StartBuilding(string option) {
			if (isBuilding) {
				Destroy (currentConstructionGhostObject);
				StopBuilding ();
				instance.StartCoroutine (RestartHelper (option));
				return;
			}
			isBuilding = true;
			currentConstructionGhost = option;
			CreateTiles ();
			currentConstructionGhostObject = Instantiate (instance.ghost);
			currentConstructionGhostObject.GetComponent<ConstructionGhost> ().SetInfo (options [option]);
		}

		public static void RefreshBuilding() {
			ClearTiles ();
			instance.StartCoroutine (RefreshBuildingHelper());
		}

		private static System.Collections.IEnumerator RestartHelper(string option) {
			yield return null;
			StartBuilding (option);
		}

		private static System.Collections.IEnumerator RefreshBuildingHelper() {
			yield return null;
			CreateTiles ();
		}

		public static void StopBuilding() {
			isBuilding = false;
			ClearTiles ();
		}

		public static void Construct(Vector3 pos, Collider[] overlap, EntityId id) {
			List<EntityId> toDestroy = new List<EntityId> ();
			foreach (Collider c in overlap) {
				if (c.gameObject.tag == "vegitation")
					toDestroy.Add(c.gameObject.EntityId());
			}
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),currentConstructionGhost, new Improbable.Collections.Option<EntityId>(id), toDestroy), PlayerController.instance.gameObject.EntityId())
				.OnSuccess(result => OnConstructionSuccess());
		}

		public static void Construct(Vector3 pos, Collider[] overlap) {
			List<EntityId> toDestroy = new List<EntityId> ();
			foreach (Collider c in overlap) {
				if (c.gameObject.tag == "vegitation")
					toDestroy.Add(c.gameObject.EntityId());
			}
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, PlayerOnline.Commands.Construct.Descriptor, new ConstructionRequest(new Vector3d(pos.x,pos.y,pos.z),currentConstructionGhost, new Improbable.Collections.Option<EntityId>(), toDestroy), PlayerController.instance.gameObject.EntityId());
		}

		private static void OnConstructionSuccess() {
			if (isBuilding)
				RefreshBuilding ();
		}

		private List<string> GetOptionsList() {
			List<string> s = new List<string> ();
			foreach (string o in options.Keys) {
				s.Add (o);
			}
			return s;
		}

		private static void CreateTiles() {
			currentTiles = new List<GameObject>();
			BuildingVisualizer[] buildings = FindObjectsOfType<BuildingVisualizer> ();
			foreach (BuildingVisualizer building in buildings) {
				if (!building.district.HasValue) {
					continue;
				}
				for (int z = -1 * building.tileMargin; z < building.zWidth + building.tileMargin; z++) {
					for (int x = -1 * building.tileMargin; x < building.xWidth + building.tileMargin; x++) {
						// relative position to block locked bottom tile on building
						Vector3 pos = building.transform.position + new Vector3(x * 8, 0f, z * 8);
						GameObject g = GameObject.Instantiate (instance.tile, pos, Quaternion.identity);
						g.GetComponent<ConstructionTile> ().SetDistrict (building.district.Value);
						currentTiles.Add (g);
					}
				}
			}
		}

		public static void DestroyTile(GameObject g) {
			Destroy (g);
			currentTiles.Remove (g);
		}

		private static void ClearTiles() {
			foreach (GameObject c in currentTiles) {
				Destroy (c);
			}
			currentTiles.Clear ();
		}

		[System.Serializable]
		public struct ConstructionInfo {
			public int xWidth, zWidth;
			public bool requiresDistrict;
			public bool multiPlacement;
			public ConstructionInfo(int x, int z, bool d, bool m) {
				xWidth = x;
				zWidth = z;
				requiresDistrict = d;
				multiPlacement = m;
			}
		}

	}


}