using System.Collections.Generic;
using System.Collections;

using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;
using UnityEngine;

// Placed on a GameObject in a Unity scene to execute SpatialOS connection logic on startup.
namespace Assets.Gamelogic.Core
{
    public class Bootstrap : MonoBehaviour
    {
        public WorkerConfigurationData Configuration = new WorkerConfigurationData();
		public Item[] items;

		public static PlayerDataComponent playerDataObject;

		public static Improbable.Collections.Map<int, PlayerColor> players = new Improbable.Collections.Map<int, PlayerColor> ();

		public static int playerId = 1;

        private void Start() {
			Bootstrap.playerDataObject = FindObjectOfType<PlayerDataComponent> ();
			if (Bootstrap.playerDataObject != null) {
				playerId = Bootstrap.playerDataObject.data.id;
			}
            SpatialOS.ApplyConfiguration(Configuration);
			Item.InitializeItems (items);

            Time.fixedDeltaTime = 1.0f / SimulationSettings.FixedFramerate;

            // Distinguishes between when the Unity is running as a client or a server.
            switch (SpatialOS.Configuration.WorkerPlatform)
            {
			case WorkerPlatform.UnityWorker:

				Application.targetFrameRate = SimulationSettings.TargetServerFramerate;
				SpatialOS.OnDisconnected += reason => Application.Quit ();
				SpatialOS.Connect(gameObject);

                    break;
                case WorkerPlatform.UnityClient:
					LoadPlayers ();

                    Application.targetFrameRate = SimulationSettings.TargetClientFramerate;
					SpatialOS.OnConnected += OnClientConnection;
                    break;
            }

        }

        private static void OnClientConnection() {
			var playerCreatorQuery = Query.HasComponent<PlayerCreator>().ReturnOnlyEntityIds();
			SpatialOS.WorkerCommands.SendQuery(playerCreatorQuery)
				.OnSuccess(OnSuccessfulPlayerCreatorQuery)
				.OnFailure(OnFailedPlayerCreatorQuery);
        }

		private static void OnSuccessfulPlayerCreatorQuery(EntityQueryResult queryResult) {
			if (queryResult.EntityCount < 1) {
				Debug.LogError("Failed to find PlayerCreator. SpatialOS probably hadn't finished loading the initial snapshot. Try again in a few seconds.");
				return;
			}

			var playerCreatorEntityId = queryResult.Entities.First.Value.Key;
			RequestPlayerCreation(playerCreatorEntityId);
		}

		private static void OnFailedPlayerCreatorQuery(ICommandErrorDetails _) {
			Debug.LogError("PlayerCreator query failed. SpatialOS workers probably haven't started yet. Try again in a few seconds.");
		}

		// Send a CreatePlayer command to the PLayerCreator entity requesting a Player entity be spawned.
		private static void RequestPlayerCreation(EntityId playerCreatorEntityId) {
			SpatialOS.WorkerCommands.SendCommand(PlayerCreator.Commands.CreatePlayer.Descriptor, new CreatePlayerRequest(playerId, playerDataObject.data.token), playerCreatorEntityId)
				.OnFailure(response => OnCreatePlayerFailure(response, playerCreatorEntityId));
		}

		private static void OnCreatePlayerFailure(ICommandErrorDetails _, EntityId playerCreatorEntityId) {
			Debug.LogWarning("CreatePlayer command failed - you probably tried to connect too soon. Try again in a few seconds.");
		}

		private void LoadPlayers() {
			WWWForm form = new WWWForm ();
			form.AddField ("test", "test");
			WWW w = new WWW ("http://cdn.lilsumn.com/players.php", form);    
			StartCoroutine (LoadPlayerData (w));

		}

		private IEnumerator LoadPlayerData(WWW _w) {
			yield return _w; 
			if (_w.error == null) {
				if (_w.text.Contains ("!!BAD!!LOGIN!!")) {
					Debug.LogWarning ("Bad Login!");
				} else {
					Players playerData = JsonUtility.FromJson<Players> (_w.text);
					for (int x = 0; x < playerData.list.Count; x++) {
						if (players.ContainsKey (playerData.list [x].id)) {

							players [playerData.list [x].id] = new PlayerColor (playerData.list [x].red, playerData.list [x].green, playerData.list [x].blue);
						} else {
							players [playerData.list [x].id] = new PlayerColor (playerData.list [x].red, playerData.list [x].green, playerData.list [x].blue);
						}
					}
					SpatialOS.Connect(gameObject);

				}
			} else {
				Debug.LogWarning(_w.error);

			}
		}

		[System.Serializable]
		public class Players 
		{
			public List<LoginMenu.PlayerData> list;
		}

    }
}
