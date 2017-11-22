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
	
	public class AITaskConstruction : AIAction {

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

		public EntityId constructionSite;
		public Option<EntityId> district;

		public AITaskConstruction(CharacterController o, EntityId w, Option<EntityId> d) : base(o,"construction") {
			constructionSite = w;
			district = d;
		}

		public AITaskConstruction(CharacterController o, EntityId w) : base(o,"construction") {
			constructionSite = w;
			district = new Option<EntityId>();
		}
	
		public override int Update(){
			
			if (shouldRespond != 100) {
				return shouldRespond;
			}
				
			switch (state) {
			case 0:
				// send command asking for a get job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Construction.Commands.GetJob.Descriptor, new ConstructionJobRequest (agent.GetItemInHand()), constructionSite)
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
					else {
						return 200;
					}
				}
				taskResult = task.Update ();
				if (AIAction.OnTermination (taskResult))
					state++;
				break;
			case 3:
				// send command giving results of get job
				SpatialOS.Commands.SendCommand (agent.characterWriter, Construction.Commands.CompleteJob.Descriptor, new ConstructionJobResult (assignment,taskResult), constructionSite)
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
	
				if (taskResponse.response == 100) { /* keep working */
					// restart
					state = 0;
					task = null;
					taskResult = 100;
				} else {
					// terminate
					return taskResponse.response;
				}
				break;
			}
			return 100;
		}

		private void OnJobCompletionResponse(TaskResponse n) {
			state++;
			taskResponse = n;
		}

		private void OnJobResponse(ConstructionJobAssignment a) {
			assignment = a;
			state++;
		}

		private void OnJobRequestFailed() {
			shouldRespond = 501;
			// it doesnt exist (anymore)
		}

		private void OnJobCompletionRequestFailed() {
			shouldRespond = 502;
		}

		public override void OnKill () {
			if (state == 2 && task != null /* the task is being done */) {
				task.OnKill ();
				SpatialOS.Commands.SendCommand (agent.characterWriter, Construction.Commands.CompleteJob.Descriptor, new ConstructionJobResult (assignment, 420), constructionSite);
			} else if (state > 2) {
				if (assignment.toGet.HasValue)
					agent.DropItem ();
			}
		}


	}

}