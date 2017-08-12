using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {

	public class WorkSiteController : MonoBehaviour {

		[Require] private WorkSite.Writer workSiteWriter;

		// Use this for initialization
		void OnEnable () {
			workSiteWriter.CommandReceiver.OnEnlist.RegisterResponse (OnEnlist);
		}
		
		// Update is called once per frame
		void OnDisable () {
			workSiteWriter.CommandReceiver.OnEnlist.DeregisterResponse ();
		}

		private EnlistResponse OnEnlist(EnlistRequest request, ICommandCallerInfo callerinfo) {
			return new EnlistResponse (workSiteWriter.Data.type);
		}
	}

}