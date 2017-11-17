using System.Collections;
using System.Collections.Generic;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
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

			Bootstrap.SetPlayerObject (this);

			playerWriter.CommandReceiver.OnReceiveChat.RegisterResponse (OnReceiveChat);
			playerWriter.CommandReceiver.OnReceiveNotification.RegisterResponse (OnReceiveNotification);

			transform.eulerAngles = new Vector3 (30, 45, 0);
			heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatSendingIntervalSecs, SendHeartbeat));
		}

		private void OnDisable() {
			StopCoroutine(heartbeatCoroutine);
		}

		// Use this for initialization
		void Start () {
			Camera.main.transform.SetParent (transform);
			Camera.main.transform.localPosition = new Vector3(0,0,-500);
			Camera.main.transform.localEulerAngles = new Vector3 (0, 0, 0);
			instance = this;
		}
		
		// Update is called once per frame
		void Update () {
			if (!SelectionManager.instance.IsChatting ()) {
				transform.position += transform.TransformDirection (new Vector3 (Input.GetAxis ("Horizontal") * speed, 0f, 0f));
				transform.position += new Vector3 (Input.GetAxis ("Vertical") * speed, 0f, Input.GetAxis ("Vertical") * speed);
				Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
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

		private Nothing OnReceiveChat(ReceiveChatRequest r, ICommandCallerInfo _) {
			UIManager.DisplayMessage (r.message, r.player);
			return new Nothing ();
		}

		private Nothing OnReceiveNotification(ReceiveNotificationRequest r, ICommandCallerInfo _) {
			UIManager.DisplayNotification (r.message);
			//trigger notification visual on r.entity

			return new Nothing ();
		}


	}

}