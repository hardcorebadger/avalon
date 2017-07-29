using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class PositionController : MonoBehaviour {

		[Require] private Position.Writer positionWriter;

		void OnEnable() {
			transform.position = positionWriter.Data.coords.ToUnityVector();
			StartCoroutine ("UpdatePosition");
		}

		IEnumerator UpdatePosition() {
			while (true) {
				yield return new WaitForSeconds (1 / 9);
				if (transform.position != positionWriter.Data.coords.ToUnityVector ())
					positionWriter.Send (new Position.Update ().SetCoords (transform.position.ToCoordinates ()));
			}
		}

	}

	public static class Vector3Extensions
	{
		public static Coordinates ToCoordinates(this Vector3 vector3)
		{
			return new Coordinates(vector3.x, vector3.z, vector3.y);
		}

		public static Vector3 ToVector3(this Coordinates coord)
		{
			return new Vector3((float)coord.x, (float)coord.z, (float)coord.y);
		}
	}

}