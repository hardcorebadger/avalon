using System.Collections;
using System.Collections.Generic;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;
using Assets.Gamelogic.Utils;

namespace Assets.Gamelogic.Core {

	[WorkerType(WorkerPlatform.UnityClient)]
	public class PlayerController : MonoBehaviour {

		public float speed = 0.1f;
		public float zoomSpeed = 1f;

		[HideInInspector]
		[Require] public Player.Writer playerWriter;

		public static PlayerController instance;
		private Coroutine heartbeatCoroutine;


		private void OnEnable() {
			
			heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatSendingIntervalSecs, SendHeartbeat));
		}

		private void OnDisable() {
			StopCoroutine(heartbeatCoroutine);
		}

		// Use this for initialization
		void Start () {
			Camera.main.transform.SetParent (transform);
			Camera.main.transform.localPosition = new Vector3(0,0,-90);
			instance = this;
		}
		
		// Update is called once per frame
		void Update () {
			if (Input.GetAxis ("Horizontal") != 0f || Input.GetAxis ("Vertical") != 0f) {
				SelectionManager.instance.ClearSelected ();
			}
			if (SelectionManager.instance.selected.Count > 0) {
				transform.position = Vector3.Lerp (transform.position, SelectionManager.instance.GetMedianSelectionPosition ()+Vector3.forward*transform.position.z, 0.1f);
			} else {
				transform.position += new Vector3 (Input.GetAxis ("Horizontal")*speed, Input.GetAxis ("Vertical")*speed, Input.GetAxis("Mouse ScrollWheel")*zoomSpeed);
			}
		}

		public float pixelToUnits = 40f;

		public float RoundToNearestPixel(float unityUnits)
		{
			float valueInPixels = unityUnits * pixelToUnits;
			valueInPixels = Mathf.Round(valueInPixels);
			float roundedUnityUnits = valueInPixels * (1 / pixelToUnits);
			return roundedUnityUnits;
		}

		private void SendHeartbeat()
		{
			playerWriter.Send(new Player.Update().AddHeartbeat(new Heartbeat()));
		}

	}

}