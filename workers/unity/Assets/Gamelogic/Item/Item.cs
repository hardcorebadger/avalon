using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;

namespace Assets.Gamelogic.Core {

	[System.Serializable]
	public class Item {

		public int id;
		public string name;
		public Sprite icon;

		private static Dictionary<int,Item> items;

		public Item(int i, string n, Sprite ic) {
			id = i;
			name = n;
			icon = ic;
		}

		public static string GetName(int id) {
			return items [id].name;
		}

		public static Sprite GetIcon(int id) {
			return items [id].icon;
		}

		public static void InitializeItems(Item[] i) {
			items = new Dictionary<int,Item> ();
			foreach (Item item in i) {
				items.Add (item.id, item);
			}
		}

	}

}