using Assets.Gamelogic.EntityTemplates;
using System.Collections;
using Improbable.Collections;
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

		private Map<int, PlayerInfo> players;

		private void OnEnable()
		{
			playerCreatorWriter.CommandReceiver.OnCreatePlayer.RegisterResponse(OnCreatePlayer);
			playerCreatorWriter.CommandReceiver.OnDisconnectPlayer.RegisterResponse(OnDisconnectPlayer);
			playerCreatorWriter.CommandReceiver.OnSendChat.RegisterResponse (OnSendChat);

			players = playerCreatorWriter.Data.players;

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

						if (!players.ContainsKey (player.id)) {
							FirstLogin (callerInfo.CallerWorkerId, player.id, Vector3.zero);

						} else {
							// Find the player
							ReturningPlayer (callerInfo.CallerWorkerId, player.id);
						}

						BroadcastNotification ("[[" + player.id + "]] has logged in!");
					}
				}
			} else {
				Debug.LogError(_w.error);

			}
		}

		private void FirstLogin(string clientWorkerId, int playerId, Vector3 pos) {
			CreatePlayer (clientWorkerId, playerId, pos);

		}

		private void ReturningPlayer(string clientWorkerId, int playerId) {
			PlayerInfo i = players[playerId];
			i.online = true;
			players[playerId] = i;
			playerCreatorWriter.Send(new PlayerCreator.Update()
				.SetPlayers(players)
			);
			SpatialOS.Commands.SendCommand (playerCreatorWriter, PlayerOnline.Commands.PlayerLoginAccess.Descriptor, new LoginAccessRequest (clientWorkerId), players[playerId].id);
		}

		private DisconnectPlayerResponse OnDisconnectPlayer(DisconnectPlayerRequest request, ICommandCallerInfo callerinfo) {
			PlayerInfo i = players[request.id];
			i.online = false;
			players[request.id] = i;
			playerCreatorWriter.Send(new PlayerCreator.Update()
				.SetPlayers(players)
			);

			BroadcastNotification ("[[" + i.id + "]] has logged out!");

			return new DisconnectPlayerResponse();
		}

		private IEnumerator CreateFamily(int playerId, EntityId playerObject, Vector3 pos) {
			yield return new WaitForSeconds (10F);
			ReserveCharacterId (playerId, playerObject, pos, 0);
		}
			
		private void ReserveCharacterId(int playerId,  EntityId playerObject, Vector3 pos, int cur) {
			if (cur >= 3)
				return;
			
			SpatialOS.Commands.ReserveEntityId(playerCreatorWriter)
				.OnSuccess(result => CreateCharacterEntity(result.ReservedEntityId, playerId, playerObject, pos, cur))
				.OnFailure(failure => OnFailedReservation(failure));
		}

		private void CreateCharacterEntity(EntityId entityId, int playerId, EntityId playerObject, Vector3 pos, int cur) {

			//REGISTER NEW ENTITY ID TO PLAYER
			SpatialOS.Commands.SendCommand (
				playerCreatorWriter, 
				PlayerOnline.Commands.RegisterCharacter.Descriptor, 
				new CharacterPlayerRegisterRequest (entityId), 
				playerObject
			);

			var characterEntityTemplate = EntityTemplateFactory.CreateCharacterTemplate((pos + new Vector3 (Mathf.Round(Random.Range (-10, 10)), 3f, Mathf.Round(Random.Range (-10, 10)))), playerId, playerObject, new Option<EntityId>());
			SpatialOS.Commands.CreateEntity(playerCreatorWriter, entityId, characterEntityTemplate)
				.OnSuccess(response => ReserveCharacterId(playerId, playerObject,pos ,cur+1))
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

			// create the entity
			var playerEntityTemplate = EntityTemplateFactory.CreatePlayerTemplate(playerCreatorWriter.EntityId, clientWorkerId, playerId, pos);
			SpatialOS.Commands.CreateEntity(playerCreatorWriter, entityId, playerEntityTemplate)
				.OnSuccess(result => OnPlayerEntityCreated(playerId, entityId, pos))
				.OnFailure(failure => OnFailedEntityCreation(failure, entityId));
		}

		private void OnPlayerEntityCreated(int playerId, EntityId entityId, Vector3 pos) {

			// add them to the player map
			players.Add(playerId, new PlayerInfo(entityId, true));
			playerCreatorWriter.Send(new PlayerCreator.Update()
				.SetPlayers(players)
			);

			StartCoroutine(CreateFamily (playerId, entityId, pos));

		}

	

		private void OnFailedReservation(ICommandErrorDetails response) {
			Debug.LogError("Failed to Reserve EntityId, Aborting");
		}

		private void OnFailedEntityCreation(ICommandErrorDetails response, EntityId entityId) {
			Debug.LogError("Failed to Create Entity: " + response.ErrorMessage);
		}

		private void BroadcastMessage(string message, int sender = -1) {
			foreach (int i in players.Keys) {
				if (sender != i) {
					SpatialOS.Commands.SendCommand(playerCreatorWriter, Player.Commands.ReceiveChat.Descriptor, new ReceiveChatRequest(message, sender), players[i].id);
				}
 			}
		}

		private void BroadcastNotification(string message) {
			foreach (int i in players.Keys) {
					SpatialOS.Commands.SendCommand(playerCreatorWriter, Player.Commands.ReceiveNotification.Descriptor, new ReceiveNotificationRequest(message, new Option<EntityId>(), new Option<Vector3d>()), players[i].id);
			}
		}


		private Nothing OnSendChat(SendChatRequest r, ICommandCallerInfo _) {
			BroadcastMessage(r.message, r.player);
			return new Nothing ();
		}

	}
}
