using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSettings : MonoBehaviour {

	public BlockSettings[] blockSettings;
	public static TerrainSettings instance;

	private static Dictionary<int, BlockSettings> blockSettingsMap;

	// Use this for initialization
	void OnEnable () {
		instance = this;
		blockSettingsMap = new Dictionary<int, BlockSettings> ();
		for (int i = 0; i < blockSettings.Length; i++) {
			blockSettingsMap.Add (blockSettings [i].id, blockSettings [i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static Color GetFaceColor(int blockId) {
		return blockSettingsMap [blockId].faceColor;
	}

	public static Color GetEdgeColor(int blockId) {
		return blockSettingsMap [blockId].edgeColor;
	}

	[System.Serializable]
	public struct BlockSettings {
		public int id;
		public Color faceColor;
		public Color edgeColor;
	}
}
