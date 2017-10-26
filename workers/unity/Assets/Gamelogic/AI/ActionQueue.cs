using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Gamelogic.Core {

	public class ActionQueue {

		SortedDictionary<int, List<AIAction>> priorityQueue;

		public ActionQueue() {
			priorityQueue = new SortedDictionary<int, List<AIAction>> ();
		}

		public void Enqueue(int priority, AIAction action) {
			if (!priorityQueue.ContainsKey (priority))
				priorityQueue.Add (priority, new List<AIAction> ());
			priorityQueue[priority].Add(action);
		}

		public AIAction Dequeue() {
			if (priorityQueue.Count < 1)
				return null;
			AIAction a = priorityQueue.First ().Value [0];
			priorityQueue.First ().Value.RemoveAt (0);
			if (priorityQueue.First ().Value.Count < 1)
				priorityQueue.Remove (priorityQueue.First ().Key);
			return a;
		}

		public bool IsEmpty() {
			return priorityQueue.Count < 1;
		}

		public void CancelAllJobActions() {
			foreach (int k in priorityQueue.Keys) {
				for (int i = 0; i < priorityQueue [k].Count; i++) {
					if (priorityQueue [k] [i] is AIActionJob)
						priorityQueue [k].RemoveAt (i);
				}
			}
		}
	}

}