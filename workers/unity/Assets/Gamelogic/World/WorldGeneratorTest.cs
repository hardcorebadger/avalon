using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldGeneratorTest : MonoBehaviour {

	public int size = 256;
	public float scale = 1f;
	public float seed = 420f;
	public float amplitude = 50f;

	public float lgWeight = 0.89f;
	public float mdWeight = 0.1f;
	public float smWeight = 0.01f;

	public float lgFreq = 0.89f;
	public float mdFreq = 0.1f;
	public float smFreq = 0.01f;

	private float[,] heightmap;

	public void RandomizeSeed() {
		seed = Random.Range (-1000000,1000000);
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
				float lg = Mathf.PerlinNoise((seed+x)*(lgFreq*scale),(seed+z)*(lgFreq*scale));
				float md = Mathf.PerlinNoise((seed+x)*(mdFreq*scale),(seed+z)*(mdFreq*scale));
				float sm = Mathf.PerlinNoise((seed+x)*(smFreq*scale),(seed+z)*(smFreq*scale));
				float preDrop = lg * lgWeight + md * mdWeight + sm * smWeight;
				heightmap[x,z] = EdgeDrop(preDrop, (float)x/(float)size, (float)z/(float)size);
			}
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
		terrainData.heightmapResolution = 1;
		terrainData.size = new Vector3 (heightmap.GetLength (0), amp, heightmap.GetLength (1));
		terrainData.SetHeights (0, 0, heightmap);
		return terrainData;
	}

	private Color GetColor(float height) {
		return new Color (height, height, height);
	}

}
