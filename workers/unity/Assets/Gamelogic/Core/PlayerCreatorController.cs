using Assets.Gamelogic.EntityTemplates;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

			WWWForm form = new WWWForm ();
			form.AddField ("id", request.playerId);
			form.AddField ("token", request.session);

			WWW w = new WWW ("http://cdn.lilsumn.com/login.php", form);    
			StartCoroutine (LoadPlayerData (w, callerinfo));

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
				.OnSuccess(response => ReserveCharacterId(playerId,(pos + new Vector3 (Random.Range (-3, 3), Random.Range (-3, 3), 0)),cur+1))
				.OnFailure(failure => OnFailedEntityCreation(failure, entityId));
		}

		private void CreatePlayer(string clientWorkerId, int playerId, Vector3 pos) {
			ReservePlayerId (clientWorkerId, playerId, pos);
		}

		private void ReservePlayerId(string clientWorkerId, int playerId, Vector3 pos) {
			SpatialOS.Commands.ReserveEntityId(playerCreatorWriter)
				.OnSuccess(result => CreatePlayerEntity(clientWorkerId, playerId, result.ReservedEntityId, pos))
				.OnFailure(failure => OnFailedReservation(failure));
		}

		private void CreatePlayerEntity(string clientWorkerId, int playerId, EntityId entityId, Vector3 pos) {
			var playerEntityTemplate = EntityTemplateFactory.CreatePlayerTemplate(clientWorkerId, playerId, pos);
			SpatialOS.Commands.CreateEntity(playerCreatorWriter, entityId, playerEntityTemplate)
				.OnFailure(failure => OnFailedEntityCreation(failure, entityId));
		}

		private void OnFailedReservation(ICommandErrorDetails response) {
			Debug.LogError("Failed to Reserve EntityId, Aborting");
		}

		private void OnFailedEntityCreation(ICommandErrorDetails response, EntityId entityId) {
			Debug.LogError("Failed to Create Entity: " + response.ErrorMessage);
		}

		private IEnumerator LoadPlayerData(WWW _w, ICommandCallerInfo callerInfo) {
			yield return _w; 

			if (_w.error == null) {
				if (_w.text.Contains ("!!BAD!!LOGIN!!")) {
					Debug.LogError ("Bad Login!");
				} else {
					LoginMenu.PlayerData player = JsonUtility.FromJson<LoginMenu.PlayerData> (_w.text);

					if (player.status != 200) {
						//failed
						Debug.LogError ("Bad Login!");
					} else {
						Vector3 playerPos = new Vector3 (player.x,  player.y, 0);
						CreateFamily(player.id, Vector3.zero);
						CreatePlayer(callerInfo.CallerWorkerId, player.id, playerPos);
					}
				}
			} else {
				Debug.LogError(_w.error);

			}
		}


	}
}
