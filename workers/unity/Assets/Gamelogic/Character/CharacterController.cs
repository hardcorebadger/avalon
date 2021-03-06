﻿using Improbable.Collections;
using System.Collections;
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


namespace Assets.Gamelogic.Core {

	public class CharacterController : MonoBehaviour {

		[Require] public Character.Writer characterWriter;
		[Require] public Position.Writer positionWriter;
		[Require] public Rotation.Writer rotationWriter;

		public float speed = 5f;
		public float range = 5f;
		public float maxRotation = 60f;
		public float interpolation = 1f;
		public float width = 1f;
		public float approachRadius = 4f;
		public float arrivalRadius = 2f;
		public Quaternion facing = Quaternion.identity;
		public Animator anim;
		public OwnedController owned;
		public bool indoors = false;
		public Option<Vector3> doorPosition;
		private Option<EntityId> workSite;


		private ActionQueue actionQueue;
		private AIAction currentAction;
		private float velocity;
		public CharacterState state;
		private int itemInHand = -1;
		public float health;
		public float hunger;
		public Option<EntityId> district;

		private bool tryingToEat;

		// Initializers //

		private void OnEnable() {
			anim = GetComponent<Animator> ();
			owned = GetComponent<OwnedController> ();

			characterWriter.CommandReceiver.OnPositionTarget.RegisterResponse(OnPositionTarget);
			characterWriter.CommandReceiver.OnEntityTarget.RegisterResponse(OnEntityTarget);

			characterWriter.CommandReceiver.OnFire.RegisterResponse(OnFire);
			characterWriter.CommandReceiver.OnReceiveHit.RegisterResponse(OnReceiveHit);
			characterWriter.CommandReceiver.OnHostileAlert.RegisterResponse(OnHostileAlert);
			characterWriter.CommandReceiver.OnSetDistrict.RegisterResponse(OnSetDistrict);
			transform.position = positionWriter.Data.coords.ToVector3();
			transform.eulerAngles = new Vector3 (0, 0, rotationWriter.Data.rotation);
			state = characterWriter.Data.state;
			itemInHand = characterWriter.Data.itemInHand;
			health = characterWriter.Data.health;
			hunger = characterWriter.Data.hunger; 
			StartCoroutine (UpdateTransform());
			StartCoroutine (UpdateVitals ());

			district = characterWriter.Data.district;
		
			indoors = characterWriter.Data.isIndoors;

			actionQueue = new ActionQueue ();
		}

		private void OnDisable() {
			characterWriter.CommandReceiver.OnPositionTarget.DeregisterResponse();
			characterWriter.CommandReceiver.OnEntityTarget.DeregisterResponse();

			characterWriter.CommandReceiver.OnFire.DeregisterResponse();
			characterWriter.CommandReceiver.OnReceiveHit.DeregisterResponse();
			characterWriter.CommandReceiver.OnHostileAlert.DeregisterResponse();
			characterWriter.CommandReceiver.OnSetDistrict.DeregisterResponse();

		}

		// Updates //

		private void Update() {
			if (health <= 0F)
				DestroyCharacter ();

			transform.position += facing * new Vector3 (0, 0, velocity) * Time.deltaTime;

//			if (transform.position.y < 0.5f || transform.position.y > 1.5f)
//				transform.position = new Vector3 (transform.position.x, 1f, transform.position.z);
			
			UpdateAI ();
		}

		private IEnumerator UpdateVitals() {
			while (enabled) {
				yield return new WaitForSeconds (60f);

				hunger += GameSettings.hungerPerHour/60f;
				if (hunger >= 100f) {
					hunger = 100f;
					Hurt (10f);
				}

				if (hunger >= 10f && !tryingToEat) {
					tryingToEat = true;
					QueueAction (1, new AIActionEat (this));
				}
				
				characterWriter.Send (new Character.Update ().SetHunger (hunger));
			}
		}

		private void UpdateAI() {
			if (currentAction == null)
				return;
			
//			if (currentAction == null) {
//				if (district.HasValue)
//					currentAction = new AIActionWander (this, 60f, district.Value, 60f);
//				else
//					currentAction = new AIActionWander (this, 60f, transform.position, 60f);
//				return;
//			}

			if (AIAction.OnTermination (currentAction.Update ())) {
//				Debug.LogWarning ("terminate " + currentAction.name);
				AIAction newAction = actionQueue.Dequeue ();
				if (indoors && !(newAction is AIActionJob)) {
					SetIndoors (false, new Option<Vector3>());
				}
				currentAction = newAction;
				if (currentAction == null)
					UpdateAI ();
//				else
//					Debug.LogWarning ("new: " + currentAction.name);
			}
		}

		private IEnumerator UpdateTransform() {
			while (true) {
				yield return new WaitForSeconds (0.1f);
				positionWriter.Send (new Position.Update ().SetCoords (transform.position.ToCoordinates ()));
				rotationWriter.Send (new Rotation.Update ().SetRotation(facing.eulerAngles.y));
				characterWriter.Send (new Character.Update ().SetVelocity (velocity));
			}
		}

		// Action Queuing //

		public void QueueAction(int priority, AIAction a) {
			if (currentAction == null) {
				currentAction = a;
			} else
				actionQueue.Enqueue (priority, a);
		}

		public void QueueActionImmediate(AIAction a) {
			if (a.directCommand)
				QuitJob ();
			if (currentAction != null) {
//				Debug.LogWarning ("QAI Kill: " + currentAction.name);
				currentAction.OnKill ();
				currentAction = a;
			} else {
				currentAction = a;
			}
		}

		// Spatial Hooks //

		private Nothing OnPositionTarget(PositionTargetRequest request, ICommandCallerInfo callerinfo) {
//			Debug.LogWarning ("============");
			if (request.command == "goto")
				QueueActionImmediate (new AIActionGoTo (this, new Vector3 ((float)request.targetPosition.x, (float)request.targetPosition.y, (float)request.targetPosition.z)).DirectCommand());
			return new Nothing ();
		}

		private Nothing OnEntityTarget(EntityTargetRequest request, ICommandCallerInfo callerinfo) {
//			Debug.LogWarning ("============");
			if (request.command == "gather")
				QueueActionImmediate (new AITaskGoAndGather (this, request.target).DirectCommand());
			else if (request.command == "work")
				QueueActionImmediate (new AIActionWork (this, request.target).DirectCommand());
			else if (request.command == "attack")
				QueueActionImmediate (new AIActionAttack (this, request.target).DirectCommand());
			else if (request.command == "damage")
				QueueActionImmediate (new AIActionDamage (this, request.target).DirectCommand());
			else if (request.command == "construction")
				QueueActionImmediate (new AITaskConstruction (this, request.target, characterWriter.Data.district).DirectCommand());
			return new Nothing ();
		}

		private Nothing OnFire(Nothing request, ICommandCallerInfo callerinfo) {
			QuitJob ();
			return new Nothing ();
		}

		public void Hurt(float f) {
			health -= f;
			characterWriter.Send (new Character.Update ()
				.SetHealth (health)
				.AddShowHurt(new Nothing())
			);
		}

		private Nothing OnReceiveHit(ReceiveHitRequest request, ICommandCallerInfo callerinfo) {
			Hurt (Random.Range (3.0f, 6.0f));
			if (currentAction == null || !(currentAction is AIActionAttack || currentAction is AIActionDamage || currentAction.directCommand)) {
				QueueActionImmediate (new AIActionAttack (this, request.source));
			}

			//TODO add to hostile

//			Collider[] cols = Physics.OverlapSphere (transform.position, 50);
//			System.Collections.Generic.List<CharacterController> enemies = new System.Collections.Generic.List<CharacterController>();
//			System.Collections.Generic.List<CharacterController> friends = new System.Collections.Generic.List<CharacterController>();
//
//			for (int x = 0; x < cols.Length; x++) {
//				GameObject g = cols [x].gameObject;
//
//				// this is yourself
//				if (g.EntityId ().Id == gameObject.EntityId ().Id)
//					continue;
//				
//				CharacterController c = g.GetComponent<CharacterController> ();
//				if (c != null) {
//					if (c.characterWriter.Data.playerId == characterWriter.Data.playerId) {
//						//my character found 
//						friends.Add(c);
//					} else if (c.characterWriter.Data.playerId == request.playerId) {
//						//other HOSTILE character found
//						enemies.Add(c);
//					} else {
//						//other NEUTRAL/HOSTILE character found
//					}
//				}
//
//			}
//			int i = -1; 
//			for (int y = 0; y < friends.Count; y++) {
//				i++;
//				SpatialOS.Commands.SendCommand (characterWriter, Character.Commands.HostileAlert.Descriptor, new HostileAlertRequest(enemies[i].characterWriter.EntityId), friends[y].characterWriter.EntityId);
//				if (i >= (enemies.Count - 1)) {
//					i = -1;
//				}
//
//			}

			return new Nothing ();
		}

		private Nothing OnHostileAlert(HostileAlertRequest request, ICommandCallerInfo callerinfo) {
			
			if (currentAction == null || !(currentAction is AIActionAttack || currentAction is AIActionDamage || currentAction.directCommand)) {
				QueueActionImmediate(new AIActionAttack (this, request.target));
			}

			return new Nothing ();
		}

		private Nothing OnSetDistrict(SetCharacterDistrictRequest request, ICommandCallerInfo callerinfo) {
			district = request.districtId;
			characterWriter.Send (new Character.Update ()
				.SetDistrict (district)
			);


			return new Nothing ();
		}

		private void OnDeregisteredSelfDistrict(Nothing n) {

			SpatialOS.Commands.SendCommand (
				characterWriter, 
				PlayerOnline.Commands.DeregisterCharacter.Descriptor, 
				new CharacterPlayerDeregisterRequest (gameObject.EntityId()), 
				owned.getOwnerObject()
			).OnSuccess (OnDeregisteredSelf);

		}

		private void OnDeregisteredSelf(Nothing n) {
			// finally delete yourself

			SpatialOS.WorkerCommands.DeleteEntity (gameObject.EntityId ());
		}

		// Public Interface Functions //

		public void SetState(CharacterState s) {
			state = s;
			characterWriter.Send (new Character.Update ()
				.SetState (s)
			);
		}

		public void SetVelocity(float f) {
			velocity = f;
			// preserve gravitational force
		}

		public Vector3 GetFacingDirection() {
			return facing*Vector3.forward;
		}

		public bool HasApplicableItem(ConstructionData c) {
			if (!c.requirements.ContainsKey(itemInHand))
				return false;
			if (c.requirements [itemInHand].required - c.requirements [itemInHand].amount > 0)
				return true;

			return false;
		}

		public void Eat(float amount) {
			hunger -= amount;
			if (hunger <= 0)
				hunger = 0;
			
			characterWriter.Send (new Character.Update ()
				.SetHunger (hunger)
			);

			tryingToEat = false;
		}

		public void EatFailed() {
			// trying to eat still, try again in a minute
			StartCoroutine (EatRequeue ());
		}

		private IEnumerator EatRequeue() {
			yield return new WaitForSeconds (60f);
			QueueAction (1, new AIActionEat (this));
		}

		public void DropItem() {
			itemInHand = -1;
			characterWriter.Send (new Character.Update ()
				.SetItemInHand (itemInHand)
			);
		}

		public int GetItemInHand() {
			return itemInHand;
		}

		public bool EmptyHanded() {
			return (itemInHand == -1);
		}

		public bool SetInHandItem(int id) {
			if (itemInHand != -1)
				return false;

			itemInHand = id;
			characterWriter.Send (new Character.Update ()
				.SetItemInHand (itemInHand)
			);

			return true;
		}

		public void SetDebugMetadata(int i) {
			characterWriter.Send (new Character.Update ()
				.SetDebugMetadata (i)
			);
		}

		public void OnDealHit() {
			if (characterWriter != null && characterWriter.HasAuthority && currentAction != null) {
				currentAction.OnDealHit ();
			}
//			else if (!characterWriter.HasAuthority) {
//				GetComponent<CharacterVisualizer> ().OnDealHit ();
//			}
		}

		public void DestroyCharacter() {
			if (district.HasValue) {
				// deregiste the construction site

				Improbable.Collections.List<EntityId> l = new Improbable.Collections.List<EntityId> ();
				l.Add (gameObject.EntityId());
				SpatialOS.Commands.SendCommand (
					characterWriter, 
					District.Commands.DeregisterCharacter.Descriptor, 
					new CharacterDeregistrationRequest (l), 
				district.Value
				).OnSuccess (OnDeregisteredSelfDistrict);
			} else {
				// settlement construction is not registered, so no deregistration
				OnDeregisteredSelfDistrict (new Nothing ());
			}
		}

		public void SetIndoors(bool b, Option<Vector3> door) {

			indoors = b;
			characterWriter.Send (new Character.Update ()
				.SetIsIndoors (indoors)
			);
			if (indoors) {
				doorPosition = door;
				GetComponentInChildren<Collider> ().enabled = false;
			} else {
				GetComponentInChildren<Collider> ().enabled = true;
				transform.position = doorPosition.Value;
			}
		}

		public void Hit(EntityId other) {
			anim.SetTrigger ("attack");
			characterWriter.Send (new Character.Update ()
				.AddShowHit(new Nothing())
			);
		}

		public void SetWorkSite(EntityId i) {
			workSite = new Option<EntityId>(i);
			characterWriter.Send (new Character.Update ()
				.SetWorkSite(workSite)
			);
		}

		public void ClearWorkSite() {
			workSite = new Option<EntityId>();
			characterWriter.Send (new Character.Update ()
				.SetWorkSite(workSite)
			);
		}

		public void QuitJob() {
			QuitJob (true);
		}
			
		public void QuitJob(bool clearActions) {				
			if (clearActions) {
				if (currentAction is AIActionJob) {
//					Debug.LogWarning ("Job Kill");
					currentAction.OnKill ();
					actionQueue.CancelAllJobActions ();
					currentAction = actionQueue.Dequeue ();
				} else {
					actionQueue.CancelAllJobActions ();
				}
			}

			if (workSite.HasValue) {
				SpatialOS.Commands.SendCommand (characterWriter, WorkSite.Commands.UnEnlist.Descriptor, new UnEnlistRequest (gameObject.EntityId()), workSite.Value);
				ClearWorkSite ();
			}
		}

	}

}
