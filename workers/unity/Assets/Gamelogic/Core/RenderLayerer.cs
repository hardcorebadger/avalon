using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenderLayerer : MonoBehaviour {

	private List<SpriteRenderer> sprites;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		sprites = FindObjectsOfType<SpriteRenderer> ().ToList();
		sprites = sprites.OrderByDescending(
			t => t.transform.position.y
		).ToList();

		int layerOrder = 0;
		foreach (SpriteRenderer sprite in sprites) {
			sprite.sortingOrder = layerOrder;
			layerOrder++;
		}
	}
}
