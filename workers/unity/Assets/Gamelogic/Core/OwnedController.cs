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

	public class OwnedController : MonoBehaviour {

		[Require] private Owned.Writer ownedWriter;


		// Use this for initialization
		void OnEnable () {
			ownedWriter.CommandReceiver.OnSetOwner.RegisterResponse(OnSetOwner);

		}

		private void OnDisable()
		{
			ownedWriter.CommandReceiver.OnSetOwner.DeregisterResponse();
		}

		// Update is called once per frame
		void Update () {
			
		}

		public int getOwner() {
			return ownedWriter.Data.owner;
		}

		public EntityId getOwnerObject() {
			return ownedWriter.Data.player;
		}

		private OwnResponse OnSetOwner(OwnRequest request, ICommandCallerInfo callerinfo) {
			ownedWriter.Send (new Owned.Update ()
				.SetOwner (request.owner)
				.SetPlayer (request.player)
			);
			return new OwnResponse (true);
		}

	}

}