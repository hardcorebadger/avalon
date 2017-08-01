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
using Improbable.Unity.Core.EntityQueries;
using Improbable.Worker;

namespace Assets.Gamelogic.Core {

	public class CharacterController : MonoBehaviour {

		[Require] private Character.Writer characterWriter;
		[Require] private Position.Writer positionWriter;
		[Require] private Rotation.Writer rotationWriter;

		[HideInInspector]
		public Rigidbody2D rigidBody;

		public float speed = 5f;
		public float range = 5f;
		public float maxRotation = 60f;
		public float interpolation = 1f;
		public float width = 1f;

		private InventoryController inventory;
		private Action currentAction;

		private void OnEnable() {
			characterWriter.CommandReceiver.OnPositionTarget.RegisterResponse(OnPositionTarget);
			characterWriter.CommandReceiver.OnEntityTarget.RegisterResponse(OnEntityTarget);
			characterWriter.CommandReceiver.OnRadiusTarget.RegisterResponse(OnRadiusTarget);

			transform.position = positionWriter.Data.coords.ToUnityVector();
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
				yield return new WaitForSeconds (1 / 9);
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
				SetAction(new ActionSeek(this, new Vector3((float)request.targetPosition.x, (float)request.targetPosition.z, 0f)));
			return new Nothing ();
		}

		private Nothing OnEntityTarget(EntityTargetRequest request, ICommandCallerInfo callerinfo) {
//			if (request.command == "gather") {
			// THIS IS WHERE YOU
			// 1. query to get position
			// 2. start action seek to the position, chain action gather
//			}
			return new Nothing ();
		}

		private Nothing OnRadiusTarget(RadiusTargetRequest request, ICommandCallerInfo callerinfo) {
			if (request.command == "gather") {
				int count = 0;
				var query = Query.And(Query.HasComponent(Position.ComponentId),Query.InSphere (request.targetPosition.x, request.targetPosition.y, request.targetPosition.z, request.size)).ReturnComponents(Position.ComponentId);
				SpatialOS.Commands.SendQuery(characterWriter, query)
					.OnSuccess(result => {
						Debug.Log("Found " + result.EntityCount + " nearby entities with a inventory component");
						if (result.EntityCount < 1) {
							return;
						}
						Improbable.Collections.Map<EntityId, Entity> resultMap = result.Entities;
						foreach (Entity e in resultMap.Values) {
							Improbable.Collections.Option<IComponentData<Position>> p = e.Get<Position>();
							Vector3 pos = p.Value.Get().Value.coords.ToVector3();
							count++;
						}
						Debug.Log(count);
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
