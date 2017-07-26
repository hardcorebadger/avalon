using Assets.Gamelogic.EntityTemplates;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
	[WorkerType(WorkerPlatform.UnityWorker)]
	public class PlayerCreatorController : MonoBehaviour
	{
		[Require] private PlayerCreator.Writer playerCreatorWriter;

		private void OnEnable()
		{
			playerCreatorWriter.CommandReceiver.OnCreatePlayer.RegisterResponse(OnCreatePlayer);
		}

		private void OnDisable()
		{
			playerCreatorWriter.CommandReceiver.OnCreatePlayer.DeregisterResponse();
		}

		private CreatePlayerResponse OnCreatePlayer(CreatePlayerRequest request, ICommandCallerInfo callerinfo)
		{
			CreateFamily(request.playerId, Vector3.zero);
			CreatePlayer(callerinfo.CallerWorkerId, Vector3.zero);
			return new CreatePlayerResponse();
		}


		private void CreateFamily(int playerId, Vector3 pos) {
			ReserveCharacterId (playerId, pos, 0);
		}

		private void ReserveCharacterId(int playerId, Vector3 pos, int cur) {
			if (cur >= 3)
				return;
			
			SpatialOS.Commands.ReserveEntityId(playerCreatorWriter)
				.OnSuccess(result => CreateCharacterEntity(result.ReservedEntityId, playerId, pos, cur))
				.OnFailure(failure => OnFailedReservation(failure));
		}

		private void CreateCharacterEntity(EntityId entityId, int playerId, Vector3 pos, int cur) {
			var playerEntityTemplate = EntityTemplateFactory.CreateCharacterTemplate(playerId, pos);
			SpatialOS.Commands.CreateEntity(playerCreatorWriter, entityId, playerEntityTemplate)
				.OnSuccess(response => ReserveCharacterId(playerId,(pos + new Vector3 (Random.Range (-3, 3), 0, Random.Range (-3, 3))),cur+1))
				.OnFailure(failure => OnFailedEntityCreation(failure, entityId));
		}

		private void CreatePlayer(string clientWorkerId, Vector3 pos) {
			ReservePlayerId (clientWorkerId, pos);
		}

		private void ReservePlayerId(string clientWorkerId, Vector3 pos) {
			SpatialOS.Commands.ReserveEntityId(playerCreatorWriter)
				.OnSuccess(result => CreatePlayerEntity(clientWorkerId, result.ReservedEntityId, pos))
				.OnFailure(failure => OnFailedReservation(failure));
		}

		private void CreatePlayerEntity(string clientWorkerId, EntityId entityId, Vector3 pos) {
			var playerEntityTemplate = EntityTemplateFactory.CreatePlayerTemplate(clientWorkerId, pos);
			SpatialOS.Commands.CreateEntity(playerCreatorWriter, entityId, playerEntityTemplate)
				.OnFailure(failure => OnFailedEntityCreation(failure, entityId));
		}

		private void OnFailedReservation(ICommandErrorDetails response) {
			Debug.LogError("Failed to Reserve EntityId, Aborting");
		}

		private void OnFailedEntityCreation(ICommandErrorDetails response, EntityId entityId) {
			Debug.LogError("Failed to Create Entity: " + response.ErrorMessage);
		}
	}
}
