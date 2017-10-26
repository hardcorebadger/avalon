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
	
	public class AIJobConstruction : AIActionJob {

		// description //
		// agent does a job cycle (1 task) at a construction site

		// assumptions //
		// none

		// end condition //
		// agent has completed 1 job cycle

		// reponse codes //
		// 501 = job request failed
		// 502 = job completion request failed

		private ConstructionJobAssignment assignment;
		private TaskResponse taskResponse;
		private AIAction task;
		private int taskResult = 100;

		public AIJobConstruction(CharacterController o, EntityId w, Vector3 p, Option<EntityId> d) : base(o,w, p, d) {
		}

		public override int Update(){
			
			if (shouldRespond != 100) {
				return shouldRespond;
			}
				
			switch (state) {
			case 0:
				// send command asking for a get job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Construction.Commands.GetJob.Descriptor, new Nothing (), workSite)
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
					if (assignment.toGet.HasValue)
						task = new AITaskConstructionGet (agent, assignment, this);
					else
						task = new AIActionWait (agent, 60f);
				}
				taskResult = task.Update ();
				if (AIAction.OnTermination (taskResult))
					state++;
				break;
			case 3:
				// send command giving results of get job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Construction.Commands.CompleteJob.Descriptor, new ConstructionJobResult (assignment,taskResult), workSite)
					.OnSuccess (response => OnJobCompletionResponse (response))
					.OnFailure (response => OnJobCompletionRequestFailed ());
				state++;
				break;
			case 4:
				// waiting
				break;
			case 5:
				// drop item because construction took it
				if (assignment.toGet.HasValue)
					agent.DropItem ();
				// requeue this job
				if (taskResponse.response == 100) { /* there's no district and no applicable in hand item */
					agent.QueueAction (10, new AIJobConstruction (agent, workSite, workSitePosition, district));
					Debug.LogWarning ("construction job requeuing");
				} else if (taskResponse.response == 400)
					Debug.LogWarning ("construction job quitting due to issue");
				else if (taskResponse.response == 200)
					Debug.LogWarning ("construction job quitting due to completion");
				// terminate
				return 200;
			}
			return 100;
		}

		private void OnJobCompletionResponse(TaskResponse n) {
			Debug.LogWarning ("tak response:  " + n.response);
			state++;
			taskResponse = n;
		}

		private void OnJobResponse(ConstructionJobAssignment a) {
			Debug.LogWarning ("job response");
			assignment = a;
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