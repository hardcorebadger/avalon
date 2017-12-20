using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Improbable;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Configuration;
using Improbable.Unity.Core;
using Improbable.Unity.Core.EntityQueries;
using Improbable.Entity.Component;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;

namespace Assets.Gamelogic.Core {

	public class UIBuilding : UIPreviewWidget {

		public override void Load(UIPreviewWindow window, GameObject target) {
			base.Load (window, target);
		}
			
		public void DeleteBuildingPressed() {
			SpatialOS.Commands.SendCommand (PlayerController.instance.playerWriter, Building.Commands.DestroyBuilding.Descriptor, new Nothing (), targetObject.EntityId ());
			previewWindow.Close ();
		}

	}

}