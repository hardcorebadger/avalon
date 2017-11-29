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

		public static Entity CreateChunkTemplate(Chunk.Data  d, Vector3 pos) {
			return EntityBuilder.Begin ()
				.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent ("chunk")
				.SetPersistence (true)
				.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
				.AddComponent (d, CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateEntityTemplate(string name, Vector3 pos, int ownerId, EntityId ownerObject, Option<EntityId> district) {
			if (name.StartsWith("construction")) {
				return CreateConstructionTemplate (name, pos, ownerId, ownerObject, district, false);
			} else if (name.StartsWith("building")) {
				return CreateBuildingTemplate (name, pos, ownerId, ownerObject, district);
			} else if (name == "character") {
				return CreateCharacterTemplate (pos, ownerId, ownerObject, district);
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
				return CreateBasicEntityTemplate (GetRandomSize (GetRandomColor(name)), pos);
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



		public static Entity CreateBuildingTemplate(string name, Vector3 pos, int ownerId, EntityId ownerObject, Option<EntityId> district) {

			SourcingOption sourcing = new SourcingOption (true, new List<EntityId> (), 100f, new Vector3d (pos.x, pos.z, pos.y));

			if (name == "building-forester") {
				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent (new Building.Data (GameSettings.standardRadius, 2, 1, 100F, district, 0), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_LOGGING, 4), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Forester.Data (), CommonRequirementSets.PhysicsOnly)
					.Build ();
			} else if (name == "building-quarry") {
				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent (new Building.Data (GameSettings.standardRadius, 2, 2, 100F, district, 0), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_MINING, 4), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Quarry.Data (), CommonRequirementSets.PhysicsOnly)
					.Build ();
			} else if (name == "building-stockpile") {
				Improbable.Collections.Map<int,int> initialQuotas = new Improbable.Collections.Map<int,int> ();
				initialQuotas.Add (0, 10);
				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent (new Building.Data (GameSettings.standardRadius,3,1, 100F, district, 0), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_STORAGE, 4), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Storage.Data (sourcing, initialQuotas), CommonRequirementSets.PhysicsOnly)
					.Build ();
			} else if (name == "building-farm") {
				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent (new Building.Data (GameSettings.standardRadius,2,2, 100F, district, 0), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_FARMING, 4), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Farm.Data (), CommonRequirementSets.PhysicsOnly)
					.Build ();
			} else if (name == "building-settlement") {
				Map<EntityId, Vector3d> p = new Map<EntityId, Vector3d> ();
				p.Add (district.Value, new Vector3d (pos.x, pos.y, pos.z));
				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent (new Building.Data (GameSettings.settlementRadius, 3, 3, 1000F, district, GameSettings.settlementBeds), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new District.Data (p, 0, new Improbable.Collections.Map<EntityId, JobInfoOption> (), new Improbable.Collections.List<EntityId> (), new Improbable.Collections.Map<int,ItemTrend>()), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_BUILDING, 12), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Builder.Data (), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Inventory.Data (GameSettings.startingInventory, 200), CommonRequirementSets.PhysicsOnly)
					.Build ();
			} else if (name == "building-house-3d") {

				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent(new Building.Data(GameSettings.standardRadius,1,1, 100F, district, 2), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.Build();
			} else if (name == "building-wall") {

				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent(new Building.Data(GameSettings.standardRadius,1,1, 1000F, district, 0), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.Build();
			} else if (name == "building-tower") {

				return EntityBuilder.Begin ()
					.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
					.AddMetadataComponent (name)
					.SetPersistence (true)
					.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
					.AddComponent(new Building.Data(GameSettings.standardRadius,1,1, 1000F, district, 0), CommonRequirementSets.PhysicsOnly)
					.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_TOWER, 4), CommonRequirementSets.PhysicsOnly)
					.AddComponent(new Tower.Data(0), CommonRequirementSets.PhysicsOnly)
					.Build();
			}
			return CreateBasicBuildingTemplate (name, pos, ownerId, ownerObject, district);
		}

		public static Entity CreateBasicBuildingTemplate(string name, Vector3 pos, int ownerId, EntityId ownerObject, Option<EntityId> district) {

			return EntityBuilder.Begin ()
				.AddPositionComponent (pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent (name)
				.SetPersistence (true)
				.SetReadAcl (CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Building.Data(GameSettings.standardRadius,1,1, 100F, district, 0), CommonRequirementSets.PhysicsOnly)
				.AddComponent (new Owned.Data (ownerId, OwnedType.OWNED_BUILDING, ownerObject), CommonRequirementSets.PhysicsOnly)
				.Build();

		}

		public static Entity CreateDestroyedBuildingTemplate(string name, Vector3 pos, int ownerId, EntityId ownerObject, Option<EntityId> district) {
			return CreateConstructionTemplate (name, pos, ownerId, ownerObject, district, true);
		}
			
		public static Entity CreateConstructionTemplate(string name, Vector3 pos, int ownerId, EntityId ownerObject, Option<EntityId> district, bool wasDestroyed) {
			Improbable.Collections.Map<int, ConstructionRequirement> req = new Improbable.Collections.Map<int, ConstructionRequirement> ();
			int x = 1;
			int z = 1;

			if (GameSettings.debugMode) {
				req.Add (0, new ConstructionRequirement (0, 1, 0));
			}

			if (name == "construction-house-3d") {
				if (!GameSettings.debugMode) {
					req.Add (0, new ConstructionRequirement (0, 5, 0));
					req.Add (1, new ConstructionRequirement (0, 5, 0));
				}
			} else if (name == "construction-forester") {
				if (!GameSettings.debugMode) {
					req.Add (0, new ConstructionRequirement (0, 10, 0));
					req.Add (1, new ConstructionRequirement (0, 10, 0));
				}
				x = 2;
			} else if (name == "construction-quarry") {
				if (!GameSettings.debugMode) {
					req.Add (0, new ConstructionRequirement (0, 10, 0));
					req.Add (1, new ConstructionRequirement (0, 10, 0));
				}
				x = 2;
				z = 2;
			} else if (name == "construction-farm") {
				if (!GameSettings.debugMode) {
					req.Add (0, new ConstructionRequirement (0, 5, 0));
					req.Add (1, new ConstructionRequirement (0, 10, 0));
				}
				x = 2;
				z = 2;
			} else if (name == "construction-stockpile") {
				if (!GameSettings.debugMode) {
					req.Add (0, new ConstructionRequirement (0, 10, 0));
					req.Add (1, new ConstructionRequirement (0, 10, 0));
				}
				x = 3;
			} else if (name == "construction-settlement") {
				if (!GameSettings.debugMode) {
					req.Add (0, new ConstructionRequirement (0, 5, 0));
				}
				x = 3;
				z = 3;
			} else if (name == "construction-road") {
				if (!GameSettings.debugMode) {
					req.Add (1, new ConstructionRequirement (0, 1, 0));
				}
			} else if (name == "construction-wall") {
				if (!GameSettings.debugMode) {
					req.Add (1, new ConstructionRequirement (0, 3, 0));
				}
			} else if (name == "construction-tower") {
				if (!GameSettings.debugMode) {
					req.Add (1, new ConstructionRequirement (0, 3, 0));
				}
			}
				
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new WorkSite.Data(new Improbable.Collections.List<EntityId>(), WorkType.WORK_BUILDING, 4), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Construction.Data(req,wasDestroyed), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Building.Data(GameSettings.standardRadius,x,z, 100f, district, 0), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(ownerId, OwnedType.OWNED_CONSTRUCTION, ownerObject), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreatePineTemplate(string name, Vector3 pos) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent(name)
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Gatherable.Data(5f, new ItemStack(0,1), WorkType.WORK_LOGGING), CommonRequirementSets.PhysicsOnly)
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
				.AddComponent(new Gatherable.Data(5f, new ItemStack(0,1), WorkType.WORK_LOGGING), CommonRequirementSets.PhysicsOnly)
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
				.AddComponent(new Gatherable.Data(5f, new ItemStack(1,1), WorkType.WORK_QUARRYING), CommonRequirementSets.PhysicsOnly)
				.Build();
		}

		public static Entity CreateCharacterTemplate(Vector3 pos, int playerId, EntityId playerObject, Option<EntityId> district) {
			return EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.PhysicsOnly)
				.AddMetadataComponent("character")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Rotation.Data(0f), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Character.Data(playerId, CharacterState.DEFAULT, 0, -1, 0, 100, district, new Option<EntityId>(), false, 0f), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new Owned.Data(playerId, OwnedType.OWNED_CHARACTER, playerObject), CommonRequirementSets.PhysicsOnly)

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
			var acl = Acl.Build()
				.SetReadAccess(CommonRequirementSets.PhysicsOrVisual)
				.SetWriteAccess<EntityAcl>(CommonRequirementSets.PhysicsOnly)
				.SetWriteAccess<Position>(CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.SetWriteAccess<Player>(CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.SetWriteAccess<PlayerOnline>(CommonRequirementSets.PhysicsOnly)
				.SetWriteAccess<HeartbeatCounter>(CommonRequirementSets.PhysicsOnly);

			var e = EntityBuilder.Begin()
				.AddPositionComponent(pos, CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddMetadataComponent("player")
				.SetPersistence(true)
				.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
				.AddComponent(new Player.Data(creator), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
				.AddComponent(new PlayerOnline.Data(playerId, new List<EntityId>(), new List<EntityId>()), CommonRequirementSets.PhysicsOnly)
				.AddComponent(new HeartbeatCounter.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout),CommonRequirementSets.PhysicsOnly)
				.Build();

			e.SetAcl(acl);
			return e;
		}

    }

}
