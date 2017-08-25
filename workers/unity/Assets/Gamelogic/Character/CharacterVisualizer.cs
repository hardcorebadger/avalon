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

		private Rigidbody2D rigidBody;
		private SpriteRenderer sprite;
		public CharacterState state;
		public float ipAllowance = 0.1f;
		private GameObject currentParticle;
		public GameObject choppingParticle; 
		public GameObject buildingParticle;
		private Quaternion facing;

		void OnEnable() {
			if (characterReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			rigidBody = GetComponent<Rigidbody2D> ();
			sprite = GetComponent<SpriteRenderer> ();
			PlayerColor c = Bootstrap.players [characterReader.Data.playerId].color;
			sprite.color = new Color(c.red, c.green, c.blue, 1f); 
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
				facing.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);
		}

		void OnCharacterUpdated(Character.Update update) {
			if (!characterReader.HasAuthority && update.state.HasValue) {
				UpdateState ( update.state.Value);
			}
			if (!characterReader.HasAuthority && update.velocity.HasValue) {
				rigidBody.velocity = facing * new Vector2 (0, update.velocity.Value);
			}
		}

		public bool CanControl() {
			return characterReader.Data.playerId == Bootstrap.playerId;
		}

		private void UpdateState(CharacterState s) {
			state = s;
			if (currentParticle != null) {
				Destroy (currentParticle);
				currentParticle = null;
			}
			switch (state) {
			case CharacterState.DEFAULT:
				Destroy (currentParticle);
				currentParticle = null;
				break;
			case CharacterState.CHOPPING:
				currentParticle = Instantiate (choppingParticle, transform);
				currentParticle.transform.localPosition += transform.forward*0.1f;
				currentParticle.transform.eulerAngles = new Vector3(-90,0,0);
				break;
			case CharacterState.BUILDING:
				currentParticle = Instantiate (buildingParticle, transform);
				currentParticle.transform.localPosition += transform.forward*0.1f;
				currentParticle.transform.eulerAngles = new Vector3(-90,0,0);
				break;
			default:
				break;
			}
				
		}

	}

}