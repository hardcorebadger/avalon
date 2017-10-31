using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable.Core;
using Improbable.Worker;

namespace Assets.Gamelogic.Core {

	public class AITaskPlantTree : AIAction {

		// description //
		// agent walks to a location, plants a tree, and returns

		// assumptions //
		// none

		// end condition //
		// agent is back at the work site having planted a tree

		// reponse codes //
		// 401 = assignment didnt ask to chop

		private AIActionGoTo seek;
		private AIActionPlace place;
		private AIJobForester job;
		private ForesterJobAssignment assignment;
		private int placeResult;

		public AITaskPlantTree(CharacterController o, ForesterJobAssignment a, AIJobForester j) : base(o,"planttreetask") {
			assignment = a;
			job = j;
		}

		public override int Update(){
			switch (state) {
			case 0:
				// setup
				if (!assignment.plant.HasValue)
					return 401;
				state++;
				break;
			case 1:
				// walk to tree
				if (seek == null)
					seek = new AIActionGoTo (agent, assignment.plant.Value.ToUnityVector());
				if (AIAction.OnSuccess (seek.Update ())) {
					seek = null;
					state++;
				}
				break;
			case 2:
				if (place == null)
					place = new AIActionPlace (agent, GetTreeEntity ());
				placeResult = place.Update ();
				if (AIAction.OnTermination (placeResult))
					state++;
				break;
			case 3:
				// walk back, return get response
				if (seek == null)
					seek = new AIActionGoTo (agent, job.workSite, job.workSitePosition);
				if (AIAction.OnSuccess (seek.Update ()))
					return placeResult;
				break;
			}
			return 100;
		}

		private Entity GetTreeEntity() {
			return EntityTemplates.EntityTemplateFactory.CreateEntityTemplate ("pine", assignment.plant.Value.ToUnityVector ());
		}
	}

}