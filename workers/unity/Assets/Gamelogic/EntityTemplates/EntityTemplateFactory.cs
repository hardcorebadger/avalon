using Improbable.Core;
using Improbable.Worker;
using Improbable.Unity.Core.Acls;
using Improbable.Unity.Entity;
using Improbable;
using Improbable.Collections;
using UnityEngine;
using Assets.Gamelogic.Core;

namespace Assets.Gamelogic.EntityTemplates
{
    public static class EntityTemplateFactory
    {

		public static Entity CreateTreeTemplate(Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("Pine")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.Build();
		}

		public static Entity CreateCharacterTemplate(int playerId, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("Character")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Rotation.Data(0f), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Character.Data(playerId), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePlayerCreatorTemplate() {
			return EntityBuilder.Begin()
				.AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("PlayerCreator")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOnly)
				.AddComponent(new PlayerCreator.Data(), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePlayerTemplate(string clientWorkerId, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddMetadataComponent("Player")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Player.Data(0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.Build();
		}

    }
}
