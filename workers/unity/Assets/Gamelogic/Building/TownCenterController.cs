using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class TownCenterController : MonoBehaviour {

		[Require] private TownCenter.Writer townCenterWriter;

		private List<EntityId> tentativeCancellations;

		private void OnEnable() {
			townCenterWriter.CommandReceiver.OnAddCitizen.RegisterResponse (OnAddCitizen);
			townCenterWriter.CommandReceiver.OnAddBuilding.RegisterResponse (OnAddBuilding);
			townCenterWriter.CommandReceiver.OnRemoveCitizen.RegisterResponse (OnRemoveCitizen);
			townCenterWriter.CommandReceiver.OnRemoveBuilding.RegisterResponse (OnRemoveBuilding);

			townCenterWriter.CommandReceiver.OnTentativeAddCitizen.RegisterResponse (OnTentativeAddCitizen);
			townCenterWriter.CommandReceiver.OnTentativeRemoveCitizen.RegisterResponse (OnTentativeRemoveCitizen);

			tentativeCancellations = new List<EntityId> ();

			StartCoroutine (TownTick());
		}

		private void OnDisable() {
			townCenterWriter.CommandReceiver.OnAddCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnAddBuilding.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnRemoveCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnRemoveBuilding.DeregisterResponse ();

			townCenterWriter.CommandReceiver.OnTentativeAddCitizen.DeregisterResponse ();
			townCenterWriter.CommandReceiver.OnTentativeRemoveCitizen.DeregisterResponse ();
		}

		private System.Collections.IEnumerator TownTick() {
			while (enabled) {
				// tick ever minute
				yield return new WaitForSeconds (60f);
			}
		}

		// definitive commands

		private TownAddResponse OnAddCitizen(TownAddRequest request, ICommandCallerInfo callerinfo) {
			AddCitizen (request.entity);
			return new TownAddResponse ();
		}

		private TownAddResponse OnAddBuilding(TownAddRequest request, ICommandCallerInfo callerinfo) {
			AddBuilding (request.entity);
			return new TownAddResponse ();
		}

		private TownRemoveResponse OnRemoveCitizen(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			RemoveCitizen(request.entity);
			return new TownRemoveResponse ();
		}

		private TownRemoveResponse OnRemoveBuilding(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			RemoveBuilding (request.entity);
			return new TownRemoveResponse ();
		}

		// tentative commands are used when characters are moving over worker borders
		// when a character is disabled on worker 1 a tentative removal is called
		// a timer is set, as long as a tentative add is recieved before the timer, the citizen stays in the list
		// if an add is not recieved, the character is removed

		// this system is here so that however a chracter is destroyed, the world doesn't fall apart from bad links
		// usually characters should be destoring themselves, in which case they can definitively remove themselves before destruction
		// Also there's no way to know when the character actually spawned, so this is used to add them tentatively in case they are new
	

		private TownAddResponse OnTentativeAddCitizen(TownAddRequest request, ICommandCallerInfo callerinfo) {
			if (!townCenterWriter.Data.citizens.Contains(request.entity)) 
				AddCitizen(request.entity);
			else
				tentativeCancellations.Add(request.entity);
			
			return new TownAddResponse ();
		}

		private TownRemoveResponse OnTentativeRemoveCitizen(TownRemoveRequest request, ICommandCallerInfo callerinfo) {
			StartCoroutine(TentativeRemoval(request.entity));
			return new TownRemoveResponse ();
		}

		private System.Collections.IEnumerator TentativeRemoval(EntityId id) {
			yield return new WaitForSeconds(5f);
			if (!tentativeCancellations.Contains (id)) {
				RemoveCitizen (id);
			} else {
				tentativeCancellations.Remove (id);
			}
		}

		// actual update functions

		public void RemoveCitizen(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.citizens;
			newList.Remove (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetCitizens (newList)
			);
		}

		public void RemoveBuilding(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.buildings;
			newList.Remove (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetBuildings (newList)
			);
		}

		public void AddCitizen(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.citizens;
			newList.Add (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetCitizens (newList)
			);
		}

		public void AddBuilding(EntityId id) {
			List<EntityId> newList = townCenterWriter.Data.buildings;
			newList.Add (id);
			townCenterWriter.Send (new TownCenter.Update ()
				.SetBuildings (newList)
			);
		}

	}

}
