using Assets.Gamelogic.Core;
using Improbable;
using Improbable.Worker;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Assets.Gamelogic.EntityTemplates;

namespace Assets.Editor 
{
	public class SnapshotMenu : MonoBehaviour
	{
		[MenuItem("Improbable/Snapshots/Generate Default Snapshot")]
		[UsedImplicitly]
		private static void GenerateDefaultSnapshot()
		{
			var snapshotEntities = new Dictionary<EntityId, Entity>();

			// Add entity data to the snapshot
			var currentEntityId = 1;
			snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreatePlayerCreatorTemplate());

			for (float z = -500; z <= 500; z++) {
				for (float x = -500; x <= 500; x++) {
					if (Random.Range(0,1100) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateEntityTemplate("pine", new Vector3(x,0,z)));
					else if (Random.Range(0,1100) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateEntityTemplate("oak", new Vector3(x,0,z)));
					else if (Random.Range(0,1100) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateEntityTemplate("rock", new Vector3(x,0,z)));
					else if (Random.Range(0,1100) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateEntityTemplate("grass", new Vector3(x,0,z)));
					else if (Random.Range(0,1100) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateEntityTemplate("tulip", new Vector3(x,0,z)));
					
				}
			}

			SaveSnapshot(snapshotEntities);
		}

		private static void SaveSnapshot(IDictionary<EntityId, Entity> snapshotEntities)
		{
			File.Delete(SimulationSettings.DefaultSnapshotPath);
			var maybeError = Snapshot.Save(SimulationSettings.DefaultSnapshotPath, snapshotEntities);

			if (maybeError.HasValue)
			{
				Debug.LogErrorFormat("Failed to generate initial world snapshot: {0}", maybeError.Value);
			}
			else
			{
				Debug.LogFormat("Successfully generated initial world snapshot at {0}", SimulationSettings.DefaultSnapshotPath);
			}
		}
	}
}
