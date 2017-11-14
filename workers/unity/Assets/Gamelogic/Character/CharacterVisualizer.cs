using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Collections;

namespace Assets.Gamelogic.Core {
	public delegate void OnUIChange();

	public class CharacterVisualizer : MonoBehaviour {
		
		[Require] private Character.Reader characterReader;
		[Require] private Position.Reader positionReader;
		[Require] private Rotation.Reader rotationReader;

		private Rigidbody rigidBody;
		private SpriteRenderer sprite;
		public CharacterState state;
		public float ipAllowance = 0.1f;
		private GameObject currentParticle;
		public GameObject choppingParticle; 
		public GameObject buildingParticle;
		private Quaternion facing;
		private Animator anim;
		private AudioSource audioSrc;
		public AudioClip[] footstepSounds;
		public AudioClip[] acceptSounds;
		public AudioClip[] cheeringSounds;
		public SpriteRenderer itemSprite;

		private System.Collections.Generic.List<OnUIChange> listeners;

		void OnEnable() {
			if (characterReader.HasAuthority) {
				this.enabled = false;
				return;
			}
			rigidBody = GetComponent<Rigidbody> ();
			sprite = GetComponent<SpriteRenderer> ();
			anim = GetComponent<Animator> ();
			audioSrc = GetComponent<AudioSource> ();
			PlayerColor c = new PlayerColor(Bootstrap.players [characterReader.Data.playerId].red, Bootstrap.players [characterReader.Data.playerId].green, Bootstrap.players [characterReader.Data.playerId].blue);
			sprite.color = new Color(c.red, c.green, c.blue, 1f); 
			transform.position = positionReader.Data.coords.ToVector3();
			state = characterReader.Data.state;
			facing = Quaternion.identity;
			facing.eulerAngles = new Vector3 (0, 0, rotationReader.Data.rotation);
			SetIndoors (characterReader.Data.isIndoors);
			SetItemInHand (characterReader.Data.itemInHand);

			positionReader.ComponentUpdated.Add(OnPositionUpdated);
			rotationReader.ComponentUpdated.Add(OnRotationUpdated);
			characterReader.ComponentUpdated.Add(OnCharacterUpdated);
			characterReader.ShowHitTriggered.Add (OnShowHit);
			characterReader.ShowHurtTriggered.Add (OnShowHurt);

			listeners = new System.Collections.Generic.List<OnUIChange> ();

			SelectionManager.instance.OnCharacterEnabled (gameObject);
		}

		void OnDisable() {
			if (characterReader.HasAuthority) {
				return;
			}
			positionReader.ComponentUpdated.Remove(OnPositionUpdated);
			rotationReader.ComponentUpdated.Remove(OnRotationUpdated);
			characterReader.ComponentUpdated.Remove(OnCharacterUpdated);
			SelectionManager.instance.OnCharacterDisabled (gameObject);
		}

		void OnPositionUpdated(Position.Update update) {
			if (!positionReader.HasAuthority && update.coords.HasValue) {
				if (Vector3.Distance (update.coords.Value.ToVector3 (), transform.position) > ipAllowance) {
					transform.position = update.coords.Value.ToVector3 ();
				}
			}
		}

		void OnRotationUpdated(Rotation.Update update) {
			if (!rotationReader.HasAuthority && update.rotation.HasValue)
				facing.eulerAngles = new Vector3 (0, rotationReader.Data.rotation, 0);
		}

		void OnCharacterUpdated(Character.Update update) {
			if (!characterReader.HasAuthority && update.state.HasValue) {
				UpdateState ( update.state.Value);
			}
			if (!characterReader.HasAuthority && update.velocity.HasValue) {
				if (update.velocity.Value > 0)
					anim.SetBool ("walking", true);
				else
					anim.SetBool ("walking", false);
				rigidBody.velocity = new Vector3(0f, rigidBody.velocity.y, 0f) + facing * new Vector3 (0, 0, update.velocity.Value);

			}
			if (!characterReader.HasAuthority && update.itemInHand.HasValue) {
				SetItemInHand (update.itemInHand.Value);
			}
			if (!characterReader.HasAuthority && update.isIndoors.HasValue) {
				SetIndoors (update.isIndoors.Value);
			}
			foreach (OnUIChange c in listeners) {
				c ();
			}
		}

		private void SetItemInHand(int i) {
			if (i == -1)
				itemSprite.enabled = false;
			else {
				itemSprite.sprite = Item.GetIcon (i);
				itemSprite.enabled = true;
			}
		}

		private void SetIndoors(bool b) {
			GetComponent<Collider> ().enabled = !b;
			GetComponent<Rigidbody> ().isKinematic = b;
		}

		private void OnShowHit(Nothing n) {

			if (characterReader != null && anim != null) {
				
				if (!characterReader.HasAuthority)
					anim.SetTrigger ("attack");

			}
			
		}

		private void OnShowHurt(Nothing n) {
			if (!characterReader.HasAuthority)
				anim.SetTrigger ("hurt");
		}

		public float GetHealth() {
			return characterReader.Data.health;
		}

		public float GetHunger() {
			return characterReader.Data.hunger;
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

		public void RegisterUIListener(OnUIChange c) {
		
			listeners.Add(c);
		}

		public void DeRegisterUIListener(OnUIChange c) {
			listeners.Remove(c);

		}

		public void OnFootstep() {
			audioSrc.PlayOneShot (footstepSounds [Random.Range (0, footstepSounds.Length - 1)],0.3f);
		}

		public void OnAcceptCommand() {
			audioSrc.PlayOneShot (acceptSounds [Random.Range (0, acceptSounds.Length - 1)]);
		}

	}

}