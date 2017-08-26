using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Assets.Gamelogic.Utils;
using Improbable.Worker;
using Improbable.Unity.Core.Acls;
using Improbable.Unity.Entity;

namespace Assets.Gamelogic.Core
{

	public class PlayerOnlineController : MonoBehaviour {

		[Require] private PlayerOnline.Writer playerOnlineWriter;
		[Require] private Player.Reader playerReader;
		[Require] private Position.Reader positionReader;

		[Require]
		private HeartbeatCounter.Writer HeartbeatCounterWriter;

		private Coroutine heartbeatCoroutine;

		// Use this for initialization
		void OnEnable () {
			
			playerReader.HeartbeatTriggered.Add(OnHeartbeat);
			heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatCheckIntervalSecs, CheckHeartbeat));
			playerOnlineWriter.CommandReceiver.OnConstruct.RegisterResponse (OnConstruct);
			playerOnlineWriter.CommandReceiver.OnConstructTown.RegisterResponse (OnConstructTown);
		}

		// Update is called once per frame
		void OnDisable () {
			playerReader.HeartbeatTriggered.Remove(OnHeartbeat);
			StopCoroutine(heartbeatCoroutine);
			playerOnlineWriter.CommandReceiver.OnConstruct.DeregisterResponse ();
			playerOnlineWriter.CommandReceiver.OnConstructTown.DeregisterResponse ();

		}

		private ConstructionResponse OnConstruct(ConstructionRequest request, ICommandCallerInfo callerinfo) {

			string entityName = "construction-" + request.buildingName;
			Vector3 pos = new Vector3 ((float)request.position.x, (float)request.position.y);
			int ownerId = playerOnlineWriter.Data.playerId;

			SpatialOS.Commands.CreateEntity (playerOnlineWriter, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate(entityName, pos, ownerId, new Improbable.Collections.Option<EntityId>()));
			return new ConstructionResponse(true);
		}

		private ConstructionResponse OnConstructTown(ConstructionTownRequest request, ICommandCallerInfo callerinfo) {

			string entityName = "construction-" + request.buildingName;
			Vector3 pos = new Vector3 ((float)request.position.x, (float)request.position.y);
			int ownerId = playerOnlineWriter.Data.playerId;

			SpatialOS.Commands.CreateEntity (playerOnlineWriter, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate (entityName, pos, ownerId, request.townCenter));

			return new ConstructionResponse(true);
		}

		private void OnHeartbeat(Heartbeat _)
		{
			SetHeartbeat(SimulationSettings.TotalHeartbeatsBeforeTimeout);
		}


		private void SetHeartbeat(uint timeoutBeatsRemaining)
		{
			HeartbeatCounterWriter.Send(new HeartbeatCounter.Update().SetTimeoutBeatsRemaining(timeoutBeatsRemaining));
		}


		private void CheckHeartbeat()
		{
			var heartbeatsRemainingBeforeTimeout = HeartbeatCounterWriter.Data.timeoutBeatsRemaining;
			if (heartbeatsRemainingBeforeTimeout == 0)
			{
				StopCoroutine(heartbeatCoroutine);
				DeletePlayerEntity();
				return;
			}
			SetHeartbeat(heartbeatsRemainingBeforeTimeout - 1);
		}


		private void DeletePlayerEntity()
		{
			SpatialOS.WorkerCommands.SendCommand (PlayerCreator.Commands.DisconnectPlayer.Descriptor, new DisconnectPlayerRequest (playerOnlineWriter.Data.playerId, (long)positionReader.Data.coords.x, (long)positionReader.Data.coords.z), playerReader.Data.creator);
			SpatialOS.Commands.DeleteEntity(HeartbeatCounterWriter, gameObject.EntityId());
		}

	}

}