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

			for (float y = -500; y <= 500; y++) {
				for (float x = -500; x <= 500; x++) {
					if (Random.Range(0,400) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreatePineTemplate(GetRandomSize("pine"), new Vector3(x,y,0)));
					else if (Random.Range(0,400) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateOakTemplate(GetRandomSize("oak"), new Vector3(x,y,0)));
					else if (Random.Range(0,400) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateRockTemplate(GetRandomSize("rock"), new Vector3(x,y,0)));
					else if (Random.Range(0,400) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateBasicEntityTemplate(GetRandomSize("grass"), new Vector3(x,y,0)));
					else if (Random.Range(0,800) == 0)
						snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateBasicEntityTemplate(GetRandomSize(GetRandomColor("tulip")), new Vector3(x,y,0)));
					
				}
			}

			snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateHouseCheatTemplate(new Vector3(7,7,0)));


			SaveSnapshot(snapshotEntities);
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
