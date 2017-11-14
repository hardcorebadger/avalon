using System.Collections;
using Improbable.Collections;
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
		[Require] private EntityAcl.Writer aclWriter;

		[Require]
		private HeartbeatCounter.Writer HeartbeatCounterWriter;

		private Coroutine heartbeatCoroutine;
		List<EntityId> characters;
		List<EntityId> districts;

		// Use this for initialization
		void OnEnable () {
			
			playerReader.HeartbeatTriggered.Add(OnHeartbeat);
			heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatCheckIntervalSecs, CheckHeartbeat));
			playerOnlineWriter.CommandReceiver.OnPlayerLoginAccess.RegisterResponse (OnPlayerLoginAccess);
			playerOnlineWriter.CommandReceiver.OnConstruct.RegisterResponse (OnConstruct);
			playerOnlineWriter.CommandReceiver.OnRegisterCharacter.RegisterResponse (OnRegisterCharacter);
			playerOnlineWriter.CommandReceiver.OnDeregisterCharacter.RegisterResponse (OnDeregisterCharacter);
			playerOnlineWriter.CommandReceiver.OnRegisterDistrict.RegisterResponse (OnRegisterDistrict);
			playerOnlineWriter.CommandReceiver.OnDeregisterDistrict.RegisterResponse (OnDeregisterDistrict);
			characters = playerOnlineWriter.Data.characters;
			districts = playerOnlineWriter.Data.districts;

		}

		// Update is called once per frame
		void OnDisable () {
			playerReader.HeartbeatTriggered.Remove(OnHeartbeat);
			StopCoroutine(heartbeatCoroutine);
			playerOnlineWriter.CommandReceiver.OnConstruct.DeregisterResponse ();
			playerOnlineWriter.CommandReceiver.OnRegisterCharacter.DeregisterResponse ();
			playerOnlineWriter.CommandReceiver.OnDeregisterCharacter.DeregisterResponse ();
			playerOnlineWriter.CommandReceiver.OnRegisterDistrict.DeregisterResponse ();
			playerOnlineWriter.CommandReceiver.OnDeregisterDistrict.DeregisterResponse ();
		}

		private LoginAccessResponse OnPlayerLoginAccess(LoginAccessRequest r, ICommandCallerInfo callerinfo) {

			var write = new Map<uint, WorkerRequirementSet>();
			write.Add (EntityAcl.ComponentId, CommonRequirementSets.PhysicsOnly);
			write.Add (Position.ComponentId, CommonRequirementSets.SpecificClientOnly(r.clientWorkerId));
			write.Add (Player.ComponentId, CommonRequirementSets.SpecificClientOnly(r.clientWorkerId));
			write.Add (PlayerOnline.ComponentId, CommonRequirementSets.PhysicsOnly);
			write.Add (HeartbeatCounter.ComponentId, CommonRequirementSets.PhysicsOnly);

			aclWriter.Send(new EntityAcl.Update()
				.SetComponentWriteAcl(write)
			);

			return new LoginAccessResponse();
		}

		private ConstructionResponse OnConstruct(ConstructionRequest request, ICommandCallerInfo callerinfo) {
			
			string entityName = "construction-" + request.buildingName;
			int ownerId = playerOnlineWriter.Data.playerId;

			SpatialOS.Commands.CreateEntity (playerOnlineWriter, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate(entityName, request.position.ToUnityVector(), ownerId, gameObject.EntityId(), request.district))
				.OnSuccess (entityId => OnContructionCreated (entityId.CreatedEntityId, request));
			return new ConstructionResponse(true);
		}

		public void OnContructionCreated(EntityId id, ConstructionRequest req) {
			// send add request to req.district with the id created
			if (req.district.HasValue) {
				SpatialOS.Commands.SendCommand (playerOnlineWriter, District.Commands.RegisterBuilding.Descriptor, new BuildingRegistrationRequest (id, req.position, 0, new List<int>(), true), req.district.Value);
			}
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
				SpatialOS.WorkerCommands.SendCommand (PlayerCreator.Commands.DisconnectPlayer.Descriptor, new DisconnectPlayerRequest (playerOnlineWriter.Data.playerId), playerReader.Data.creator);
				return;
			}
			SetHeartbeat(heartbeatsRemainingBeforeTimeout - 1);
		}

		private Nothing OnRegisterCharacter(CharacterPlayerRegisterRequest r, ICommandCallerInfo _) {
			characters.Add(r.characterId);
			playerOnlineWriter.Send (new PlayerOnline.Update ()
				.SetCharacters(characters)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterCharacter(CharacterPlayerDeregisterRequest r, ICommandCallerInfo _) {
			characters.Remove(r.characterId);

			playerOnlineWriter.Send (new PlayerOnline.Update ()
				.SetCharacters(characters)
			);
			return new Nothing ();
		}

		private Nothing OnRegisterDistrict(DistrictRegisterRequest r, ICommandCallerInfo _) {
			if (districts.Count == 0) {
				StartCoroutine (SendCharactersToDistrict (r));
				foreach (var c in characters) {
					SpatialOS.Commands.SendCommand (
						playerOnlineWriter, 
						Character.Commands.SetDistrict.Descriptor, 
						new SetCharacterDistrictRequest (r.districtId), 
						c
					);
				}
			}

			districts.Add(r.districtId);

			playerOnlineWriter.Send (new PlayerOnline.Update ()
				.SetDistricts(districts)
			);
			return new Nothing ();
		}

		private Nothing OnDeregisterDistrict(DistrictDeregisterRequest r, ICommandCallerInfo _) {
			
			districts.Remove(r.districtId);

			playerOnlineWriter.Send (new PlayerOnline.Update ()
				.SetDistricts(districts)
			);
			return new Nothing ();
		}

		private IEnumerator SendCharactersToDistrict(DistrictRegisterRequest r) {
			yield return new WaitForSeconds (5f);
			SpatialOS.Commands.SendCommand (
				playerOnlineWriter, 
				District.Commands.RegisterCharacter.Descriptor, 
				new CharacterRegistrationRequest (characters), 
				r.districtId
			);

		}

	}

}