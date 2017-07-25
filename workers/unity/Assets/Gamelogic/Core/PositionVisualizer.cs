using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class PositionVisualizer : MonoBehaviour {

		[Require] private Position.Reader positionReader;

		void OnEnable() {
			transform.position = positionReader.Data.coords.ToUnityVector();

			positionReader.ComponentUpdated.Add(OnPositionUpdated);
		}

		void OnDisable() {
			positionReader.ComponentUpdated.Remove(OnPositionUpdated);
		}

		void OnPositionUpdated(Position.Update update) {
			if (!positionReader.HasAuthority && update.coords.HasValue)
				transform.position = update.coords.Value.ToUnityVector();
		}
	}

}