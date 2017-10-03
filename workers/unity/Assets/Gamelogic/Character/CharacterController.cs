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

	public class CharacterController : MonoBehaviour {

		[Require] public Character.Writer characterWriter;
		[Require] public Position.Writer positionWriter;
		[Require] public Rotation.Writer rotationWriter;

		[HideInInspector]
		public Rigidbody rigidBody;

		public float speed = 5f;
		public float range = 5f;
		public float maxRotation = 60f;
		public float interpolation = 1f;
		public float width = 1f;
		public float approachRadius = 4f;
		public float arrivalRadius = 2f;
		public Quaternion facing = Quaternion.identity;
		public Animator anim;


		private Action currentAction;
		private float velocity;
		public CharacterState state;
		private int itemInHand = -1;
		public float health;

		private void OnEnable() {
			anim = GetComponent<Animator> ();

			characterWriter.CommandReceiver.OnPositionTarget.RegisterResponse(OnPositionTarget);
			characterWriter.CommandReceiver.OnEntityTarget.RegisterResponse(OnEntityTarget);
			characterWriter.CommandReceiver.OnRadiusTarget.RegisterResponse(OnRadiusTarget);

			characterWriter.CommandReceiver.OnFire.RegisterResponse(OnFire);
			characterWriter.CommandReceiver.OnReceiveHit.RegisterResponse(OnReceiveHit);

			transform.position = positionWriter.Data.coords.ToVector3();
			transform.eulerAngles = new Vector3 (0, 0, rotationWriter.Data.rotation);
			state = characterWriter.Data.state;
			itemInHand = characterWriter.Data.itemInHand;
			health = characterWriter.Data.health;
			StartCoroutine ("UpdateTransform");

			rigidBody = GetComponent<Rigidbody> ();
		
			currentAction = new ActionBlank (this);
		}

		private void OnDisable() {
			characterWriter.CommandReceiver.OnPositionTarget.DeregisterResponse();
			characterWriter.CommandReceiver.OnEntityTarget.DeregisterResponse();
			characterWriter.CommandReceiver.OnRadiusTarget.DeregisterResponse();

			characterWriter.CommandReceiver.OnFire.DeregisterResponse();
		}

		IEnumerator UpdateTransform() {
			while (true) {
				yield return new WaitForSeconds (0.1f);
				positionWriter.Send (new Position.Update ().SetCoords (transform.position.ToCoordinates ()));
				rotationWriter.Send (new Rotation.Update ().SetRotation(facing.eulerAngles.y));
				characterWriter.Send (new Character.Update ().SetVelocity (velocity));
			}
		}

		private void Update() {
			// if the controlling action completes, stop doing it

			if (health <= 0F) {
				SpatialOS.Commands.DeleteEntity(characterWriter, gameObject.EntityId());
			}

			ActionCode code = currentAction.Update ();
			if (code == ActionCode.Success || code == ActionCode.Failure)
				currentAction = new ActionBlank (this);
		}

		private Nothing OnPositionTarget(PositionTargetRequest request, ICommandCallerInfo callerinfo) {
			if (request.command == "goto")
				SetAction (new ActionSeek (this, new Vector3 ((float)request.targetPosition.x, (float)request.targetPosition.y, (float)request.targetPosition.z)));
			return new Nothing ();
		}

		private Nothing OnEntityTarget(EntityTargetRequest request, ICommandCallerInfo callerinfo) {
			if (request.command == "gather") 
				SetAction (new ActionGather (this, request.target));
			else if (request.command == "work")
				SetAction (new ActionWork (this, request.target));
			else if (request.command == "attack")
				SetAction (new ActionAttack (this, request.target));
			return new Nothing ();
		}

		private Nothing OnRadiusTarget(RadiusTargetRequest request, ICommandCallerInfo callerinfo) {
//			if (request.command == "gather") {
//				var query = Query.And (Query.HasComponent (Gatherable.ComponentId), Query.InSphere (request.targetPosition.x, request.targetPosition.y, request.targetPosition.z, request.size)).ReturnOnlyEntityIds ();
//				SpatialOS.Commands.SendQuery(characterWriter, query)
//					.OnSuccess(result => {
//						if (result.EntityCount < 1) {
//							return;
//						}
//						Improbable.Collections.Map<EntityId, Entity> resultMap = result.Entities;
//						EntityId[] ids = new EntityId[resultMap.Count];
//						int index = 0;
//						foreach (EntityId i in resultMap.Keys) {
//							ids[index] = i;
//							index++;
//						}
//						SetAction(new ActionDistributedGather(this, request.groupInfo.groupId, request.groupInfo.groupSize, ids));
//					})
//					.OnFailure(errorDetails => Debug.Log("Query failed with error: " + errorDetails));
//			}
			return new Nothing ();
		}

		private Nothing OnFire(Nothing request, ICommandCallerInfo callerinfo) {
			SetAction (new ActionBlank (this));
			return new Nothing ();
		}

		private Nothing OnReceiveHit(ReceiveHitRequest request, ICommandCallerInfo callerinfo) {

//			Debug.LogWarning ("Received Hit!");
			health -= 10F;
			characterWriter.Send (new Character.Update ()
				.SetHealth (health)
			);

			return new Nothing ();
		}


		public void SetAction(Action a) {
			if (currentAction != null)
				currentAction.OnKill ();
			SetVelocity (0f);
			rigidBody.angularVelocity = Vector3.zero;
			SetState (CharacterState.DEFAULT);
			currentAction = a;
		}

		public void SetState(CharacterState s) {
			state = s;
			characterWriter.Send (new Character.Update ()
				.SetState (s)
			);
		}

		public void SetVelocity(float f) {
			velocity = f;
			// preserve gravitational force
			rigidBody.velocity = new Vector3(0f, rigidBody.velocity.y, 0f) + (facing * new Vector3 (0, 0, velocity));
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

		public void DropItem() {
			itemInHand = -1;
			characterWriter.Send (new Character.Update ()
				.SetItemInHand (itemInHand)
			);
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
			if (characterWriter.HasAuthority &&  currentAction != null) {
				currentAction.OnDealHit ();
			}
		}

	}

}
