using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGeneratorTest))]
public class WorldGeneratorTestEditor : UnityEditor.Editor {

	public override void OnInspectorGUI() {

		DrawDefaultInspector();

		WorldGeneratorTest generator = (WorldGeneratorTest)target;

		if (GUILayout.Button ("Random Seed")) {
			generator.RandomizeSeed ();
		}

		if (GUILayout.Button ("Generate")) {
			generator.Regenerate ();
		}
	}

}
