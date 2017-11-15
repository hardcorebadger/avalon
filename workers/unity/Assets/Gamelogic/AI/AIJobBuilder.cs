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

	public class AIJobBuilder : AIActionJob {


		// description //
		// agent performs a builder cycle

		// assumptions //
		// none

		// end condition //
		// agent completed a builder cycle

		// reponse codes //

		private BuilderJobAssignment assignment;
		private AIAction task;
		private int taskResult = 100;

		public AIJobBuilder(CharacterController o, EntityId w, Vector3 p, EntityId d) : base(o, w, p, d, new Option<Vector3>(),"builder") {
		}

		public override int Update(){
			if (shouldRespond != 100) {
				return shouldRespond;
			}

			switch (state) {
			case 0:
				// send command asking for a job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Builder.Commands.GetJob.Descriptor, new BuilderJobRequest (), workSite)
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
					if (assignment.construction.HasValue) {
						task = new AITaskConstruction (agent, assignment.construction.Value, district);
					} else {
						task = new AIActionWait (agent, 60f);
					}
				}
				taskResult = task.Update ();
				if (AIAction.OnTermination (taskResult))
					state++;
				break;
			case 3:
				// send command giving results of job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Builder.Commands.CompleteJob.Descriptor, new BuilderJobResult (assignment,taskResult), workSite)
					.OnSuccess (response => OnJobCompletionResponse (response))
					.OnFailure (response => OnJobCompletionRequestFailed ());
				state++;
				break;
			case 4:
				// waiting
				break;
			case 5:
				// requeue this job
				agent.QueueAction (10, new AIJobBuilder (agent, workSite, workSitePosition, district));
				// terminate
				return 200;
			}
			return 100;
		}

		private void OnJobResponse(BuilderJobAssignment a) {
			assignment = a;
			state++;
		}

		private void OnJobCompletionResponse(TaskResponse n) {
			state++;
		}

		private void OnJobRequestFailed() {
			shouldRespond = 501;
		}

		private void OnJobCompletionRequestFailed() {
			shouldRespond = 502;
		}

		public override void OnKill () {
			if (state == 2 && task != null /* the task is being done */) {
				task.OnKill ();
				SpatialOS.Commands.SendCommand (agent.characterWriter, Builder.Commands.CompleteJob.Descriptor, new BuilderJobResult (assignment, 420), workSite);
			}
			agent.QueueAction (10, new AIJobBuilder (agent, workSite, workSitePosition, district));
		}
	}

}