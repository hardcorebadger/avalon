using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class CharacterVisualizer : MonoBehaviour {
		
		[Require] private Character.Reader characterReader;
		[Require] private Position.Reader positionReader;
		[Require] private Rotation.Reader rotationReader;

		private int ownerId = 1;


		void OnEnable() {
			transform.position = positionReader.Data.coords.ToVector3();
			transform.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);

			positionReader.ComponentUpdated.Add(OnPositionUpdated);
			rotationReader.ComponentUpdated.Add(OnRotationUpdated);
		}

		void OnDisable() {
			positionReader.ComponentUpdated.Remove(OnPositionUpdated);
			rotationReader.ComponentUpdated.Remove(OnRotationUpdated);
		}

		void OnPositionUpdated(Position.Update update) {
			if (!positionReader.HasAuthority && update.coords.HasValue)
				transform.position = update.coords.Value.ToVector3();
		}

		void OnRotationUpdated(Rotation.Update update) {
			if (!rotationReader.HasAuthority && update.rotation.HasValue)
				transform.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);
		}

		public bool CanControl() {
			return ownerId == Bootstrap.playerId;
		}

	}

}