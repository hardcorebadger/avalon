using Improbable.Collections;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker.Query;
using Improbable.Worker;
using Improbable.Entity;
using Improbable.Unity.Core.EntityQueries;

namespace Assets.Gamelogic.Core {

	public class AIJobForester : AIActionJob {

		// description //
		// agent performs a forester cycle

		// assumptions //
		// none

		// end condition //
		// agent completed a forester cycle

		// reponse codes //

		private ForesterJobAssignment assignment;
		private AIAction task;
		private int taskResult = 100;

		public AIJobForester(CharacterController o, EntityId w, Vector3 p, Option<EntityId> d) : base(o, w, p, d) {
		}

		public override int Update(){
			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				// send command asking for a job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Forester.Commands.GetJob.Descriptor, new Nothing (), workSite)
					.OnSuccess (response => OnJobResponse (response))
					.OnFailure (response => OnJobRequestFailed ());
				state++;
				break;
			case 1:
				// waiting
				break;
			case 2:
				// execute get job
				if (task == null) {
					if (assignment.chop.HasValue)
						task = new AITaskChopTree (agent, assignment, this);
					else if (assignment.plant.HasValue)
						task = new AITaskPlantTree (agent, assignment, this);
					else
						task = new AIActionWait (agent, 60f);
				}
				taskResult = task.Update ();
				if (AIAction.OnTermination (taskResult))
					state++;
				break;
			case 3:
				// send command giving results of job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Forester.Commands.CompleteJob.Descriptor, new ForesterJobResult (assignment,taskResult), workSite)
					.OnSuccess (response => OnJobCompletionResponse (response))
					.OnFailure (response => OnJobCompletionRequestFailed ());
				state++;
				break;
			case 4:
				// waiting
				break;
			case 5:
				// requeue this job
				agent.QueueAction (10, new AIJobForester (agent, workSite, workSitePosition, district));
				// terminate
				return 200;
			}
			return 100;
		}

		private void OnJobResponse(ForesterJobAssignment a) {
			assignment = a;
			state++;
		}

		private void OnJobCompletionResponse(Nothing n) {
			state++;
		}

		private void OnJobRequestFailed() {
			shouldRespond = 501;
		}

		private void OnJobCompletionRequestFailed() {
			shouldRespond = 502;
		}
	}

}