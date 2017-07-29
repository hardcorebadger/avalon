using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class CharacterController : MonoBehaviour {

		[Require] private Character.Writer characterWriter;
		[Require] private Position.Writer positionWriter;
		[Require] private Rotation.Writer rotationWriter;

		private Rigidbody2D rigidbody;

		private void OnEnable() {
			characterWriter.CommandReceiver.OnGoto.RegisterResponse(OnGoto);
			transform.position = positionWriter.Data.coords.ToUnityVector();
			StartCoroutine ("UpdateTransform");

			rigidbody = GetComponent<Rigidbody2D> ();
		}

		private void OnDisable() {
			characterWriter.CommandReceiver.OnGoto.DeregisterResponse();
		}

		IEnumerator UpdateTransform() {
			while (true) {
				yield return new WaitForSeconds (1 / 9);
				positionWriter.Send (new Position.Update ().SetCoords (transform.position.ToCoordinates ()));
				rotationWriter.Send (new Rotation.Update ().SetRotation(transform.eulerAngles.z));
			}
		}

		private void Update() {
			rigidbody.velocity = transform.TransformDirection(new Vector2 (0, 1f));
			rigidbody.angularVelocity = 1f;
		}

		private Nothing OnGoto(GotoRequest request, ICommandCallerInfo callerinfo) {
			Debug.LogWarning ("walk to: " + request.targetPosition.x + ", " + request.targetPosition.z);
			return new Nothing ();
		}

	}

}
