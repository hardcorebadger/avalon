using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryLine : MonoBehaviour {

	public Text item;
	public Text amount;
	public Text weight;

	public void SetInfo(string i, int a, int w) {
		item.text = i;
		amount.text = "" + a;
		weight.text = "" + w;
	}

}
