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

		public static Entity CreateHouseTemplate(Vector3 pos, int ownerId) {

			Improbable.Collections.List<ResourceType> types = new Improbable.Collections.List<ResourceType> ();
			types.Add (ResourceType.RESOURCE_TIMBER);
			types.Add (ResourceType.RESOURCE_STONE);
			types.Add (ResourceType.RESOURCE_ORE);
			types.Add (ResourceType.RESOURCE_FOOD);
			types.Add (ResourceType.RESOURCE_TREASUE);
			types.Add (ResourceType.RESOURCE_WEAPONRY);
			types.Add (ResourceType.RESOURCE_MISC);

			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("House")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Inventory.Data(new Improbable.Collections.Map<int,int>(), 5000), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Storage.Data(types), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(ownerId, OwnedType.OWNED_HOME), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateHouseCheatTemplate(Vector3 pos, int ownerId) {

			Improbable.Collections.List<ResourceType> types = new Improbable.Collections.List<ResourceType> ();
			types.Add (ResourceType.RESOURCE_TIMBER);
			types.Add (ResourceType.RESOURCE_STONE);
			types.Add (ResourceType.RESOURCE_ORE);
			types.Add (ResourceType.RESOURCE_FOOD);
			types.Add (ResourceType.RESOURCE_TREASUE);
			types.Add (ResourceType.RESOURCE_WEAPONRY);
			types.Add (ResourceType.RESOURCE_MISC);

			Improbable.Collections.Map<int,int> inv = new Improbable.Collections.Map<int,int>();
			inv.Add (1, 11);

			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("House")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Inventory.Data(inv, 5000), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Storage.Data(types), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(ownerId, OwnedType.OWNED_HOME), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateHouseConstructionTemplate(Vector3 pos, int ownerId) {

			Improbable.Collections.Map<int, ConstructionRequirement> req = new Improbable.Collections.Map<int, ConstructionRequirement> ();
			req.Add (1, new ConstructionRequirement (0, 10));

			SourcingOption sourcing = new SourcingOption (true, new List<EntityId> (), 30f, new Vector3d (pos.x, pos.z, pos.y));

			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("House-Construction")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_BUILDING), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Construction.Data(req,sourcing), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(ownerId, OwnedType.OWNED_CONSTRUCTION), CommonRequirementSets.PhysicsOnly)
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

		public static Entity CreatePineTemplate(string name, Vector3 pos) {
			Improbable.Collections.Map<int,int> i = new Improbable.Collections.Map<int,int> ();
			i.Add (1, 1);
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(1, "Pine Tree", 50, i), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateOakTemplate(string name, Vector3 pos) {
			Improbable.Collections.Map<int,int> i = new Improbable.Collections.Map<int,int> ();
			i.Add (1, 1);
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(1, "Oak Tree",50,i), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateBasicEntityTemplate(string name, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.Build();
		}

		public static Entity CreateRockTemplate(string name, Vector3 pos) {
			Improbable.Collections.Map<int,int> i = new Improbable.Collections.Map<int,int> ();
			i.Add (3, 1);
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(2, "Rock", 100,i), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateCharacterTemplate(int playerId, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("Character")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Rotation.Data(0f), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Character.Data(playerId, CharacterState.DEFAULT, 0), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Inventory.Data(new Improbable.Collections.Map<int,int>(), 200), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(playerId, OwnedType.OWNED_CHARACTER), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePlayerCreatorTemplate() {
			return EntityBuilder.Begin()
				.AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("PlayerCreator")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOnly)
				.AddComponent(new PlayerCreator.Data(new Improbable.Collections.Map<int, PlayerInfo>()), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePlayerTemplate(EntityId creator, string clientWorkerId, int playerId, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos.Flip(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddMetadataComponent("Player")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Player.Data(creator), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddComponent(new PlayerOnline.Data(playerId), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new HeartbeatCounter.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout),CommonRequirementSets.PhysicsOnly)
				.Build();
		}

    }
}
