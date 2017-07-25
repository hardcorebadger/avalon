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
			CreatePlayerWithReservedId(callerinfo.CallerWorkerId);
			return new CreatePlayerResponse();
		}

		private void CreatePlayerWithReservedId(string clientWorkerId)
		{
			SpatialOS.Commands.ReserveEntityId(playerCreatorWriter)
				.OnSuccess(result => CreatePlayer(clientWorkerId, result.ReservedEntityId))
				.OnFailure(failure => OnFailedReservation(failure, clientWorkerId));
		}

		private void OnFailedReservation(ICommandErrorDetails response, string clientWorkerId)
		{
			Debug.LogError("Failed to Reserve EntityId for Player: " + response.ErrorMessage + ". Retrying...");
			CreatePlayerWithReservedId(clientWorkerId);
		}

		private void CreatePlayer(string clientWorkerId, EntityId entityId)
		{
			// Initial position is slightly randomised to prevent colliders interpenetrating at start
			var playerEntityTemplate = EntityTemplateFactory.CreatePlayerTemplate(clientWorkerId, Vector3.zero);
			SpatialOS.Commands.CreateEntity(playerCreatorWriter, entityId, playerEntityTemplate)
				.OnFailure(failure => OnFailedPlayerCreation(failure, clientWorkerId, entityId));
		}

		private void OnFailedPlayerCreation(ICommandErrorDetails response, string clientWorkerId, EntityId entityId)
		{
			Debug.LogError("Failed to Create PlayerShip Entity: " + response.ErrorMessage + ". Retrying...");
			CreatePlayer(clientWorkerId, entityId);
		}
	}
}
