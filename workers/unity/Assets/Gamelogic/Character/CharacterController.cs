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
		public Rigidbody2D rigidBody;

		public float speed = 5f;
		public float range = 5f;
		public float maxRotation = 60f;
		public float interpolation = 1f;
		public float width = 1f;
		public float approachRadius = 4f;
		public float arrivalRadius = 2f;

		public InventoryController inventory;
		private Action currentAction;

		private void OnEnable() {
			characterWriter.CommandReceiver.OnPositionTarget.RegisterResponse(OnPositionTarget);
			characterWriter.CommandReceiver.OnEntityTarget.RegisterResponse(OnEntityTarget);
			characterWriter.CommandReceiver.OnRadiusTarget.RegisterResponse(OnRadiusTarget);

			transform.position = positionWriter.Data.coords.ToVector3();
			StartCoroutine ("UpdateTransform");

			rigidBody = GetComponent<Rigidbody2D> ();
			inventory = GetComponent<InventoryController> ();

			currentAction = new ActionBlank (this);
		}

		private void OnDisable() {
			characterWriter.CommandReceiver.OnPositionTarget.DeregisterResponse();
			characterWriter.CommandReceiver.OnEntityTarget.DeregisterResponse();
			characterWriter.CommandReceiver.OnRadiusTarget.DeregisterResponse();
		}

		IEnumerator UpdateTransform() {
			while (true) {
				yield return new WaitForSeconds (0.1f);
				positionWriter.Send (new Position.Update ().SetCoords (transform.position.ToCoordinates ()));
				rotationWriter.Send (new Rotation.Update ().SetRotation(transform.eulerAngles.z));
			}
		}

		private void Update() {
			// if the controlling action completes, stop doing it
			if (currentAction.Update () == ActionCode.Success || currentAction.Update () == ActionCode.Failure)
				currentAction = new ActionBlank (this);
		}

		private Nothing OnPositionTarget(PositionTargetRequest request, ICommandCallerInfo callerinfo) {
			if (request.command == "goto")
				SetAction (new ActionSeek (this, new Vector3 ((float)request.targetPosition.x, (float)request.targetPosition.z, 0f)));
			else if (request.command == "stash")
				SetAction (new ActionStash (this));
			return new Nothing ();
		}

		private Nothing OnEntityTarget(EntityTargetRequest request, ICommandCallerInfo callerinfo) {
			if (request.command == "gather") {
				SetAction (new ActionGather (this, request.target));
			}
			return new Nothing ();
		}

		private Nothing OnRadiusTarget(RadiusTargetRequest request, ICommandCallerInfo callerinfo) {
			if (request.command == "gather") {
				var query = Query.And (Query.HasComponent (Gatherable.ComponentId), Query.InSphere (request.targetPosition.x, request.targetPosition.y, request.targetPosition.z, request.size)).ReturnOnlyEntityIds ();
				SpatialOS.Commands.SendQuery(characterWriter, query)
					.OnSuccess(result => {
						if (result.EntityCount < 1) {
							return;
						}
						Improbable.Collections.Map<EntityId, Entity> resultMap = result.Entities;
						EntityId[] ids = new EntityId[resultMap.Count];
						int index = 0;
						foreach (EntityId i in resultMap.Keys) {
							ids[index] = i;
							index++;
						}
						SetAction(new ActionGatherRadial(this, ids));
					})
					.OnFailure(errorDetails => Debug.Log("Query failed with error: " + errorDetails));
			}
			return new Nothing ();
		}

		public void SetAction(Action a) {
			currentAction = a;
		}

	}

}
