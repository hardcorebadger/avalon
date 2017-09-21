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

		public static Entity CreateEntityTemplate(string name, Vector3 pos, int ownerId) {
			if (name == "character") {
				return CreateCharacterTemplate (pos, ownerId);
			} else if (name.StartsWith("construction")) {
				return CreateConstructionTemplate (name, pos, ownerId);
			} else if (name.StartsWith("building")) {
				return CreateBuildingTemplate (name, pos, ownerId);
			}
			return null;
		}

		public static Entity CreateEntityTemplate(string name, Vector3 pos) {
			if (name == "pine") {
				return CreatePineTemplate (GetRandomSize (name), pos);
			} else if (name == "oak") {
				return CreateOakTemplate (GetRandomSize (name), pos);
			} else if (name == "rock") {
				return CreateRockTemplate (GetRandomSize (name), pos);
			} else if (name == "tulip") {
				return CreatePineTemplate (GetRandomSize (GetRandomColor(name)), pos);
			} else if (name == "grass") {
				return CreateBasicEntityTemplate (GetRandomSize (name), pos);
			}
			return CreateBasicEntityTemplate(name,pos);
		}

		private static string GetRandomSize(string baseName) {
			int i = Random.Range (0, 3);
			if (i == 0)
				return baseName + "-sm";
			else if (i == 1)
				return baseName + "-md";
			else 
				return baseName + "-lg";
		}

		private static string GetRandomColor(string baseName) {
			int i = Random.Range (0, 2);
			if (i == 0)
				return baseName + "-pink";
			else 
				return baseName + "-purple";
		}



		public static Entity CreateBuildingTemplate(string name, Vector3 pos, int ownerId) {
			if (name == "building-house-3d")
				return CreateBasicBuildingTemplate (name, pos, ownerId);

			return null;
		}

		public static Entity CreateBasicBuildingTemplate(string name, Vector3 pos, int ownerId) {

			return EntityBuilder.Begin ()
				.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent (name)
				.SetPersistence (true)
				.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Building.Data(0), CommonRequirementSets.PhysicsOnly)
				.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING), CommonRequirementSets.PhysicsOnly)
				.Build();

		}

		public static Entity CreateStorageBuildingTemplate(string name, Vector3 pos, int ownerId) {

			return EntityBuilder.Begin ()
				.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent (name)
				.SetPersistence (true)
				.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
				.AddComponent (new Inventory.Data (new Improbable.Collections.Map<int,int> (), 5000), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Building.Data(2), CommonRequirementSets.PhysicsOnly)
				.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING), CommonRequirementSets.PhysicsOnly)
				.Build();

		}

		public static Entity CreateConstructionTemplate(string name, Vector3 pos, int ownerId) {
			Improbable.Collections.Map<int, ConstructionRequirement> req = new Improbable.Collections.Map<int, ConstructionRequirement> ();

			if (name == "construction-house-3d")
				req.Add (1, new ConstructionRequirement (0, 10));

			SourcingOption sourcing = new SourcingOption (true, new List<EntityId> (), 30f, new Vector3d (pos.x, pos.z, pos.y));

			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_BUILDING), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Construction.Data(req,sourcing), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Building.Data(0), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(ownerId, OwnedType.OWNED_CONSTRUCTION), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePineTemplate(string name, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(5f, new ItemStack(1,1), WorkType.WORK_LOGGING), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateOakTemplate(string name, Vector3 pos) {
			Improbable.Collections.Map<int,int> i = new Improbable.Collections.Map<int,int> ();
			i.Add (1, 1);
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(5f, new ItemStack(1,1), WorkType.WORK_LOGGING), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateBasicEntityTemplate(string name, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.Build();
		}

		public static Entity CreateRockTemplate(string name, Vector3 pos) {
			Improbable.Collections.Map<int,int> i = new Improbable.Collections.Map<int,int> ();
			i.Add (3, 1);
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(10f, new ItemStack(2,1), WorkType.WORK_QUARRYING), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateCharacterTemplate(Vector3 pos, int playerId) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("character")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Rotation.Data(0f), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Character.Data(playerId, CharacterState.DEFAULT, 0, -1), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Inventory.Data(new Improbable.Collections.Map<int,int>(), 200), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(playerId, OwnedType.OWNED_CHARACTER), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePlayerCreatorTemplate() {
			return EntityBuilder.Begin()
				.AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("player-creator")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOnly)
				.AddComponent(new PlayerCreator.Data(new Improbable.Collections.Map<int, PlayerInfo>()), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePlayerTemplate(EntityId creator, string clientWorkerId, int playerId, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddMetadataComponent("player")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Player.Data(creator), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddComponent(new PlayerOnline.Data(playerId), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new HeartbeatCounter.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout),CommonRequirementSets.PhysicsOnly)
				.Build();
		}

    }

}
