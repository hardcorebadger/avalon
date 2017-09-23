using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldGeneratorTest1 : MonoBehaviour {

	[System.Serializable]
	public struct PerlinSettings {
		public float weight;
		public float freq;
		public float bimodalCutoff;
		public float bimodalRounding;
	}

	public int size = 1024;
	public float scale = 0.6f;
	public float seed = 420f;
	public float mountainSeed = 40f;
	public float amplitude = 40f;

	public PerlinSettings generalContinents;
	public PerlinSettings generalHills;
	public PerlinSettings generalRoughness;

	public PerlinSettings mountainMap;
	public PerlinSettings mountains;
	public PerlinSettings mountainRoughness;


	private float[,] heightmap;

	public void RandomWorldSeed() {
		seed = Random.Range (-1000000,1000000);
		Regenerate ();
	}

	public void RandomMountainSeed() {
		mountainSeed = Random.Range (-1000000,1000000);
		Regenerate ();
	}

	public void Randomize() {
		seed = Random.Range (-1000000,1000000);
		mountainSeed = Random.Range (-1000000,1000000);
		Regenerate ();
	}

	public void Regenerate() {
		GenerateWorld ();
		if (GetComponent<Terrain> () != null)
			GetComponent<Terrain> ().terrainData = GetTerrainData (GetComponent<Terrain> ().terrainData, amplitude);
		if (GetComponent<RawImage> () != null)
			GetComponent<RawImage> ().texture = GetTexture ();
	}

	public void GenerateWorld() {
		heightmap = new float[size,size];
		for (int z = 0; z < size; z++) {
			for (int x = 0; x < size; x++) {

				// ----- Basic Terrain ----- //

				// Initial 3-iteration perlin noise
				float basicTerrain = MixPerlins(new PerlinSettings[]{generalContinents,generalHills,generalRoughness}, x, z, seed);


				float mountainMixer = BimodalPerlin (mountainMap, x, z, mountainSeed);

				basicTerrain += mountainMixer * MixPerlins(new PerlinSettings[]{mountains,mountainRoughness}, x, z, mountainSeed);

				basicTerrain /= 2f;

				// round off the edges and set the heightmap!
				heightmap[x,z] = EdgeDrop(basicTerrain, (float)x/(float)size, (float)z/(float)size);
			}
		}
	}

	private float BimodalPerlin(PerlinSettings s, int x, int z, float curSeed) {
		float f = Mathf.PerlinNoise((curSeed+x)*(s.freq*scale),(curSeed+z)*(s.freq*scale));
		return Bimodal (f, s.bimodalCutoff, s.bimodalRounding);
	}

	private float MixPerlins(PerlinSettings[] s, int x, int z, float curSeed) {
		float f = 0f;
		foreach (PerlinSettings perlin in s) {
			f += BimodalPerlin (perlin, x, z, curSeed) * perlin.weight;
		}
		return f;
	}

	private float Amplify(float f, float cutoff) {
		if (f < cutoff)
			return 0f;
		float a = (f - cutoff) / (1 - cutoff);
		return f * a;
	}

	private float Bimodal(float f, float cutoff, float rounding) {
		if (f < cutoff - rounding)
			return 0f;
		else if (f > cutoff)
			return f;
		else {
			float a = f - (cutoff - rounding);
			float rate = a / rounding;
			return f * rate;
		}
	}

	private float EdgeDrop(float preDrop, float x, float z) {
		// gets value between 0 and 0.5 defining how far it is from the edge
		if (x > 0.5f)
			x = 1f - x;
		if (z > 0.5f)
			z = 1f - z;
		// this equals 1 at the edge drop point and 0 at 0, with a gradient (input value is the average edginess)
		float ratio = Mathf.Min(x,z)/0.5f;
		// mixer with ratio value on original and inverse to 0
		return preDrop * ratio;
	}

	public Texture2D GetTexture() {
		Texture2D t = new Texture2D (heightmap.GetLength(0),heightmap.GetLength(1));
		for (int z = 0; z < heightmap.GetLength(1); z++) {
			for (int x = 0; x < heightmap.GetLength(0); x++) {
				t.SetPixel(x,z,GetColor(heightmap[x,z]));
			}
		}
		t.Apply ();
		return t;
	}

	public TerrainData GetTerrainData(TerrainData terrainData, float amp) {
		terrainData.size = new Vector3 (heightmap.GetLength (0), amp, heightmap.GetLength (1));
		terrainData.SetHeights (0, 0, heightmap);
		return terrainData;
	}

	private Color GetColor(float height) {
		return new Color (height, height, height);
	}

}
