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
			generator.RandomMountainSeed();
		}

		if (GUILayout.Button ("Random")) {
			generator.Randomize();
		}

		if (GUILayout.Button ("Generate")) {
			generator.Regenerate ();
		}
	}

}
