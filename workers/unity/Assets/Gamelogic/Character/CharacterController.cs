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

		private List<Action> queuedActions;
		private List<Action> currentActions;
		private List<Action> completedActions;

		private InventoryController inventory;

		private void OnEnable() {
			characterWriter.CommandReceiver.OnPositionTarget.RegisterResponse(OnPositionTarget);
			characterWriter.CommandReceiver.OnEntityTarget.RegisterResponse(OnEntityTarget);
			characterWriter.CommandReceiver.OnRadiusTarget.RegisterResponse(OnRadiusTarget);
			transform.position = positionWriter.Data.coords.ToUnityVector();
			StartCoroutine ("UpdateTransform");

			queuedActions = new List<Action> ();
			currentActions = new List<Action> ();
			completedActions = new List<Action> ();

			rigidBody = GetComponent<Rigidbody2D> ();
			inventory = GetComponent<InventoryController> ();
		}

		private void OnDisable() {
			characterWriter.CommandReceiver.OnPositionTarget.DeregisterResponse();
		}

		IEnumerator UpdateTransform() {
			while (true) {
				yield return new WaitForSeconds (1 / 9);
				positionWriter.Send (new Position.Update ().SetCoords (transform.position.ToCoordinates ()));
				rotationWriter.Send (new Rotation.Update ().SetRotation(transform.eulerAngles.z));
			}
		}

		private void Update() {
			UpdateBrain ();
		}

		private void UpdateBrain() {
			foreach (Action a in queuedActions) {currentActions.Add (a);}
			queuedActions.Clear ();

			foreach (Action a in currentActions) {a.Update ();}

			foreach (Action a in completedActions) {currentActions.Remove (a);}
			completedActions.Clear ();
		}

		private Nothing OnPositionTarget(PositionTargetRequest request, ICommandCallerInfo callerinfo) {
			currentActions.Clear ();
			if (request.command == "goto")
				StartAction(new ActionSeek(this, null, new Vector3((float)request.targetPosition.x, (float)request.targetPosition.z, 0f)));
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
			return new Nothing ();
		}

		public void StartAction(Action a) {
			queuedActions.Add (a);
		}

		public void StopAction(Action a) {
			completedActions.Add (a);
		}

	}

}
