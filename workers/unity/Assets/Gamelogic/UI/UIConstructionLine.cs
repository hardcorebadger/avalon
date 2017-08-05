using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConstructionLine : MonoBehaviour {

	public Text item;
	public Text req;

	public void SetInfo(string i, int a, int r) {
		item.text = i;
		req.text = a + " / " + r;
	}
}
