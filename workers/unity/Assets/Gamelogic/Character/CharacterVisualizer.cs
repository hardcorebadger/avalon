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
		public CharacterState state;

		void OnEnable() {
			if (characterReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			transform.position = positionReader.Data.coords.ToVector3();
			state = characterReader.Data.state;
			transform.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);

			positionReader.ComponentUpdated.Add(OnPositionUpdated);
			rotationReader.ComponentUpdated.Add(OnRotationUpdated);
			characterReader.ComponentUpdated.Add(OnCharacterUpdated);
		}

		void OnDisable() {
			positionReader.ComponentUpdated.Remove(OnPositionUpdated);
			rotationReader.ComponentUpdated.Remove(OnRotationUpdated);
			characterReader.ComponentUpdated.Remove(OnCharacterUpdated);
		}

		void OnPositionUpdated(Position.Update update) {
			if (!positionReader.HasAuthority && update.coords.HasValue)
				transform.position = update.coords.Value.ToVector3();
		}

		void OnRotationUpdated(Rotation.Update update) {
			if (!rotationReader.HasAuthority && update.rotation.HasValue)
				transform.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);
		}

		void OnCharacterUpdated(Character.Update update) {
			if (!characterReader.HasAuthority && update.state.HasValue)
				state = update.state.Value;
		}

		public bool CanControl() {
			return ownerId == Bootstrap.playerId;
		}

	}

}