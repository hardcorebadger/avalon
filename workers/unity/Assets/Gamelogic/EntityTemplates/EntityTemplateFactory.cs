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

		public static Entity CreateHouseConstructionTemplate(Vector3 pos) {

			Improbable.Collections.Map<int, ConstructionRequirement> req = new Improbable.Collections.Map<int, ConstructionRequirement> ();
			req.Add (1, new ConstructionRequirement (0, 10));

			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("House-Construction")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Construction.Data(req), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateSackTemplate(Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("Sack")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Inventory.Data(new Improbable.Collections.Map<int,int>(), 200), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateTreeTemplate(Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("Pine")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(1, "Pine Tree"), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Inventory.Data(new Improbable.Collections.Map<int,int>(), 200), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateCharacterTemplate(int playerId, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("Character")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Rotation.Data(0f), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Character.Data(playerId), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Inventory.Data(new Improbable.Collections.Map<int,int>(), 200), CommonRequirementSets.PhysicsOnly)
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
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddMetadataComponent("Player")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Player.Data(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddComponent(new PlayerOnline.Data(1), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

    }
}
