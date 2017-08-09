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
		private Rigidbody2D rigidBody;
		public CharacterState state;
		public bool deee = false;
		public float ipAllowance = 0.1f;

		void OnEnable() {
			if (characterReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			rigidBody = GetComponent<Rigidbody2D> ();

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
			if (!positionReader.HasAuthority && update.coords.HasValue) {
				if (Vector3.Distance(update.coords.Value.ToVector3 (),transform.position) > ipAllowance)
					transform.position = update.coords.Value.ToVector3 ();
			}
		}

		void OnRotationUpdated(Rotation.Update update) {
			if (!rotationReader.HasAuthority && update.rotation.HasValue)
				transform.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);
		}

		void OnCharacterUpdated(Character.Update update) {
			if (!characterReader.HasAuthority && update.state.HasValue)
				state = update.state.Value;
			if (!characterReader.HasAuthority && update.velocity.HasValue) {
				if (deee)
					Debug.Log ("ye");
				rigidBody.velocity = transform.TransformDirection (new Vector2 (0, update.velocity.Value));
			}
		}

		public bool CanControl() {
			return ownerId == Bootstrap.playerId;
		}

	}

}