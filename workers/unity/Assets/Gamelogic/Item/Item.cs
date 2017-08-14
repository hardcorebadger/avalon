using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;

namespace Assets.Gamelogic.Core {

	public class Item {

		public int id;
		public int weight;
		public string name;
		public ResourceType type;

		private static Dictionary<int,Item> items;

		public Item(int i, int w, string n, ResourceType t) {
			id = i;
			weight = w;
			name = n;
			type = t;
		}

		public static string GetName(int id) {
			return items [id].name;
		}

		public static int GetWeight(int id) {
			return items [id].weight;
		}

		public static ResourceType GetType(int id) {
			return items [id].type;
		}

		public static void InitializeItems() {
			items = new Dictionary<int,Item> ();
			items.Add(0,new Item(0,1,"Berries", ResourceType.RESOURCE_FOOD));
			items.Add(1,new Item(1,100,"Log", ResourceType.RESOURCE_TIMBER));
			items.Add(2,new Item(2,1,"Sticks", ResourceType.RESOURCE_MISC));
			items.Add(3,new Item(3,2,"Dirt", ResourceType.RESOURCE_MISC));
		}

	}

}