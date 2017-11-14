﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class WorkSiteVisualizer : MonoBehaviour {

		[Require] private WorkSite.Reader workSiteReader;
		[Require] private Owned.Reader ownedReader;

		public int workers = 0;
		public int maxWorkers = 0;
		public Color ownerColor;

		private int currentCharViz = 0;

		void OnEnable () {
			if (workSiteReader.HasAuthority)
				return;
			workSiteReader.ComponentUpdated.Add(OnWorkSiteUpdated);
			workers = workSiteReader.Data.workers.Count;
			maxWorkers = workSiteReader.Data.maxWorkers;
			ownerColor = GetOwnerColor ();
		}

		public Color GetOwnerColor() {
			return new Color (Bootstrap.players [ownedReader.Data.owner].red, Bootstrap.players [ownedReader.Data.owner].green, Bootstrap.players [ownedReader.Data.owner].blue);
		}

		void OnDisable () {
			workSiteReader.ComponentUpdated.Remove(OnWorkSiteUpdated);
		}
		
		void OnWorkSiteUpdated (WorkSite.Update update) {
			if (workSiteReader.HasAuthority)
				return;
			if (update.workers.HasValue) {
				workers = update.workers.Value.Count;
			}
			if (update.maxWorkers.HasValue) {
				maxWorkers = update.maxWorkers.Value; 
			}
		}
	}

}