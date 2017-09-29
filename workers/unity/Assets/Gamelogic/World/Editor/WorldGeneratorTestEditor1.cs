using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGeneratorTest1))]
public class WorldGeneratorTestEditor1 : UnityEditor.Editor {

	public override void OnInspectorGUI() {

		DrawDefaultInspector();

		WorldGeneratorTest1 generator = (WorldGeneratorTest1)target;

		if (GUILayout.Button ("Random Seed")) {
			generator.RandomWorldSeed ();
		}

		if (GUILayout.Button ("Random Mountains")) {
			foreach (ChunkTest g in FindObjectsOfType<ChunkTest>())
				DestroyImmediate (g.gameObject);
			generator.RandomMountainSeed();
		}

		if (GUILayout.Button ("Random")) {
			foreach (ChunkTest g in FindObjectsOfType<ChunkTest>())
				DestroyImmediate (g.gameObject);
			generator.Randomize();
		}

		if (GUILayout.Button ("Generate")) {
			foreach (ChunkTest g in FindObjectsOfType<ChunkTest>())
				DestroyImmediate (g.gameObject);
			generator.Regenerate ();
		}

		if (GUILayout.Button ("Clear Chunks")) {
			foreach (ChunkTest g in FindObjectsOfType<ChunkTest>())
				DestroyImmediate (g.gameObject);
		}
	}

}
