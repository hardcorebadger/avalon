using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Worker;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;


namespace Assets.Gamelogic.Core {

	public class BuilderController : MonoBehaviour {

		[Require] private Builder.Writer builderWriter;
		[Require] private Building.Writer buildingWriter;

		private DistrictController districtController;

		void OnEnable () {
			builderWriter.CommandReceiver.OnGetJob.RegisterResponse(OnGetJob);
			builderWriter.CommandReceiver.OnCompleteJob.RegisterResponse(OnCompleteJob);
			districtController = GetComponent<DistrictController> ();
		}

		void OnDisable() {
			builderWriter.CommandReceiver.OnGetJob.DeregisterResponse();
			builderWriter.CommandReceiver.OnCompleteJob.DeregisterResponse();
		}

		private BuilderJobAssignment OnGetJob(BuilderJobRequest r , ICommandCallerInfo __) {
			return new BuilderJobAssignment (districtController.GetFirstConstructionSite());
		}

		private TaskResponse OnCompleteJob(BuilderJobResult result, ICommandCallerInfo _) {
			return new TaskResponse (200);
		}


	}

}