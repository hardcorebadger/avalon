using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core
{

	public class PlayerOnlineController : MonoBehaviour {

		[Require] private PlayerOnline.Writer playerOnlineWriter;

		// Use this for initialization
		void OnEnable () {
			playerOnlineWriter.CommandReceiver.OnConstruct.RegisterResponse (OnConstruct);
		}

		// Update is called once per frame
		void OnDisable () {
			playerOnlineWriter.CommandReceiver.OnConstruct.DeregisterResponse ();
		}

		private ConstructionResponse OnConstruct(ConstructionRequest request, ICommandCallerInfo callerinfo) {
			SpatialOS.Commands.CreateEntity (playerOnlineWriter, EntityTemplates.EntityTemplateFactory.CreateHouseConstructionTemplate (new Vector3((float)request.position.x, (float)request.position.y)));
			return new ConstructionResponse(true);
		}
	}

}