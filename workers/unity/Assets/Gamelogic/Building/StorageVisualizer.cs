using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class StorageVisualizer : MonoBehaviour {

		[Require] private Storage.Reader storageReader;

		private Improbable.Collections.List<ResourceType> types;

		// Use this for initialization
		void OnEnable () {
			storageReader.ComponentUpdated.Add (OnStorageUpdated);
		}

		// Update is called once per frame
		void OnDisable () {
			storageReader.ComponentUpdated.Remove (OnStorageUpdated);
		}

		private void OnStorageUpdated(Storage.Update update) {
			if (update.types.HasValue)
				types = update.types.Value;
		}
	}

}