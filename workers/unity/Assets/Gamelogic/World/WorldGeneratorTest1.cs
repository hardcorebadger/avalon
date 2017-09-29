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

	[System.Serializable]
	public struct GradientPoint {
		public float point;
		public Color color;
	}

	public int size = 1024;
	public float scale = 0.6f;
	public float seed = 420f;
	public float mountainSeed = 40f;
	public float amplitude = 40f;
	public float edgeDropCutoff = 0.3f;

	public PerlinSettings generalContinents;
	public PerlinSettings generalHills;
	public PerlinSettings generalRoughness;

	public PerlinSettings mountainMap;
	public PerlinSettings mountains;
	public PerlinSettings mountainRoughness;

	public float tempurature = 1f;
	public float waterLevel = 0.3f;
	public Color waterColor;
	public GradientPoint[] gradient;

	public int maxBlockHeight = 100;
	public float blockSize = 5f;
	public int chunkSize = 10;
	public GameObject chunkPrefab;


	public float[,] heightmap;
	public int[,] blockmap;

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
		if (GetComponent<MeshRenderer> () != null)
			BuildBlockTerrain ();
		if (chunkPrefab != null)
			BuildChunks ();
	}

	public void GenerateWorld() {
		heightmap = new float[size,size];
		blockmap = new int[size,size];
		for (int z = 0; z < size; z++) {
			for (int x = 0; x < size; x++) {

				// ----- Basic Terrain ----- //

				// Initial 3-iteration perlin noise
				float basicTerrain = MixPerlins(new PerlinSettings[]{generalContinents,generalHills,generalRoughness}, x, z, seed);

				float mountainMixer = BimodalPerlin (mountainMap, x, z, mountainSeed);

				basicTerrain += mountainMixer * MixPerlins(new PerlinSettings[]{mountains,mountainRoughness}, x, z, mountainSeed);

				basicTerrain /= ((mountains.weight + mountainRoughness.weight)+1f);

				// round off the edges and set the heightmap!
				heightmap[x,z] = EdgeDrop(basicTerrain, (float)x/(float)size, (float)z/(float)size);
				blockmap[x,z] = (int)(heightmap [x, z] * maxBlockHeight);

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

		if (ratio > edgeDropCutoff)
			return Mathf.Max(preDrop, waterLevel);
		else
			return Mathf.Max(preDrop * (ratio/edgeDropCutoff), waterLevel);
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
		if (height <= waterLevel)
			return waterColor;
		
		height -= tempurature;
		for (int i = 0; i < gradient.GetLength(0); i++) {
			if (gradient [i].point > height) {
				if (i == 0)
					return gradient [i].color;
				else {
					float v = height - gradient [i-1].point;
					float mix = v / (gradient [i].point - gradient [i-1].point);
					return Mix (gradient [i-1].color, gradient [i].color, mix);
				}
			}
		}
		if (gradient.GetLength (0) > 0)
			return gradient [gradient.GetLength (0)-1].color;
		else
			return new Color (height, height, height);
	}

	private Color Mix(Color c1, Color c2, float point) {
		return (c2 - c1) * point + c1;
	}

	public void BuildBlockTerrain() {

		Mesh mesh = new Mesh();

		List<Vector3> vertList = new List<Vector3>();
		List<Vector2> uvList = new List<Vector2>();
		List<int> triList = new List<int> ();
		List<Color> colorList = new List<Color> ();

		int i = 0;
		for (int zi = 0; zi < blockmap.GetLength(1); zi++) {
			for (int xi = 0; xi <  blockmap.GetLength(0); xi++) {
				if (xi + 1 <  blockmap.GetLength(0) && zi + 1 <  blockmap.GetLength(1) && xi > 0 && zi > 0) {
					addFace(xi, zi, ref vertList, ref uvList, ref triList, ref colorList, ref i, GetColor(heightmap[xi,zi]));

					int forward = blockmap [xi, zi] - blockmap [xi, zi + 1];
					if (forward > 0)
						addForwardEdge(forward, xi, zi, ref vertList, ref uvList, ref triList, ref colorList, ref i, GetColor(heightmap[xi,zi]));

					int backward = blockmap [xi, zi] - blockmap [xi, zi - 1];
					if (backward > 0)
						addBackwardEdge(backward, xi, zi, ref vertList, ref uvList, ref triList, ref colorList, ref i, GetColor(heightmap[xi,zi]));

					int left = blockmap [xi, zi] - blockmap [xi-1, zi];
					if (left > 0)
						addLeftEdge(left, xi, zi, ref vertList, ref uvList, ref triList, ref colorList, ref i, GetColor(heightmap[xi,zi]));

					int right = blockmap [xi, zi] - blockmap [xi+1, zi];
					if (right > 0)
						addRightEdge(right, xi, zi, ref vertList, ref uvList, ref triList, ref colorList, ref i, GetColor(heightmap[xi,zi]));
				}
			}
		}

		mesh.vertices = vertList.ToArray();
		mesh.uv = uvList.ToArray();
		mesh.triangles = triList.ToArray();
		mesh.colors = colorList.ToArray();
		mesh.RecalculateNormals();

		setMesh(mesh);

	}

	private void addForwardEdge(int length, int x, int z, ref List<Vector3> vertList, ref List<Vector2> uvList, ref List<int> triList, ref List<Color> colorList,  ref int i, Color color) {
		Vector3 v1 = ToPosition (x+1, z+1, blockmap[x,z]);
		Vector3 v2 = ToPosition (x, z+1, blockmap[x,z]);
		Vector3 v3 = ToPosition (x, z+1, blockmap[x,z]-length);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);

		v1 = ToPosition (x+1, z+1, blockmap[x,z]);
		v2 = ToPosition (x, z+1, blockmap[x,z]-length);
		v3 = ToPosition (x+1, z+1, blockmap[x,z]-length);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
	}

	private void addBackwardEdge(int length, int x, int z, ref List<Vector3> vertList, ref List<Vector2> uvList, ref List<int> triList, ref List<Color> colorList,  ref int i, Color color) {
		Vector3 v1 = ToPosition (x, z, blockmap [x, z] - length);
		Vector3 v2 = ToPosition (x, z, blockmap[x,z]);
		Vector3 v3 = ToPosition (x+1, z, blockmap[x,z]);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);

		v1 = ToPosition (x+1, z, blockmap[x,z]-length);
		v2 = ToPosition (x, z, blockmap[x,z]-length);
		v3 = ToPosition (x+1, z, blockmap[x,z]);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
	}

	private void addLeftEdge(int length, int x, int z, ref List<Vector3> vertList, ref List<Vector2> uvList, ref List<int> triList, ref List<Color> colorList,  ref int i, Color color) {
		Vector3 v1 = ToPosition (x, z+1, blockmap[x,z]);
		Vector3 v2 = ToPosition (x, z, blockmap[x,z]);
		Vector3 v3 = ToPosition (x, z, blockmap [x, z] - length);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);

		v1 = ToPosition (x, z+1, blockmap[x,z]);
		v2 = ToPosition (x, z, blockmap[x,z]-length);
		v3 = ToPosition (x, z+1, blockmap[x,z]-length);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
	}

	private void addRightEdge(int length, int x, int z, ref List<Vector3> vertList, ref List<Vector2> uvList, ref List<int> triList, ref List<Color> colorList,  ref int i, Color color) {
		Vector3 v1 = ToPosition (x+1, z, blockmap [x, z] - length);
		Vector3 v2 = ToPosition (x+1, z, blockmap[x,z]);
		Vector3 v3 = ToPosition (x+1, z+1, blockmap[x,z]);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);

		v1 = ToPosition (x+1, z+1, blockmap[x,z]-length);
		v2 = ToPosition (x+1, z, blockmap[x,z]-length);
		v3 = ToPosition (x+1, z+1, blockmap[x,z]);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
	}

	private void addFace(int x, int z, ref List<Vector3> vertList, ref List<Vector2> uvList, ref List<int> triList, ref List<Color> colorList,  ref int i, Color color) {
		Vector3 v1 = ToPosition (x, z, blockmap[x,z]);
		Vector3 v2 = ToPosition (x, z+1, blockmap[x,z]);
		Vector3 v3 = ToPosition (x+1, z, blockmap[x,z]);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);

		v1 = ToPosition (x, z+1, blockmap[x,z]);
		v2 = ToPosition (x+1, z+1, blockmap[x,z]);
		v3 = ToPosition (x+1, z, blockmap[x,z]);

		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
	}

	private void addTri(Vector3 v1, Vector3 v2, Vector3 v3, ref List<Vector3> vertList, ref List<Vector2> uvList, ref List<int> triList, ref List<Color> colorList,  ref int i, Color color) {
		addVertex(v1, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v2, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
		addVertex(v3, ref vertList, ref uvList, ref triList, ref colorList, ref i, color);
	}

	private void addVertex(Vector3 pos, ref List<Vector3> vert, ref List<Vector2> uv, ref List<int> tri, ref List<Color> color,  ref int i, Color c) {
		vert.Add (pos);
		uv.Add (new Vector2 (0, 0));
		tri.Add (i);
		color.Add (c);
		i++;
	}

	private void setMesh(Mesh m) {
		GetComponent<MeshFilter> ().mesh = m;
		GetComponent<MeshCollider> ().sharedMesh = m;
	}

	public struct HeightmapIndex {
		public int x;
		public int z;
		public HeightmapIndex(int xi, int zi) {
			x = xi;
			z = zi;
		}
	};

	public Vector3 ToPosition(int x, int z, float blockHeight) {
		Vector3 v = Vector3.zero;
		v.x = x * blockSize;
		v.z = z * blockSize;
		v.y = blockHeight * blockSize;
		return v;
	}

	public void LoopDebug(string s, float f) {
		if (Random.Range ((int)0, (int)(heightmap.GetLength (0) * heightmap.GetLength (1) / f)) == 0) {
			Debug.Log (s);
		}
	}

	public void BuildChunks() {
		for (int z = 0; z < size/chunkSize; z++) {
			for (int x = 0; x < size/chunkSize; x++) {
				GameObject c = Instantiate (chunkPrefab);
				c.GetComponent<ChunkTest> ().Setup (this, x*chunkSize, z*chunkSize);
				c.GetComponent<ChunkTest> ().BuildBlockTerrain ();
			}
		}
	}

}
