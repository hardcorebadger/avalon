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
using Improbable.Collections;

namespace Assets.Gamelogic.Core {

	public class ActionEat : Action {

		private int state = 0;
		private bool failed = false;
		private bool success = false;
		private Action subAction = null;
		private bool didSource = false;
		private Option<EntityId> district;
		private CharacterController o;

		public ActionEat(CharacterController o, Option<EntityId> d) : base(o)	{
			this.o = o;
			district = d;
		}

		public override ActionCode Update () {
			switch (state) {
			case 0:
				// check if has food, if has food skip to eat 
				if (o.GetItemInHand () != 2) {
					List<int> i = new List<int> ();
					i.Add (2);
					subAction = new ActionResourceGetDistrict (owner, district, i);
					state = 1;

				} else
					state = 2;
				
				break;
			case 1:
				// time to go get some more resources
				ActionCode c = subAction.Update ();
				if (c == ActionCode.Failure)
					return ActionCode.Failure;
				else if (c == ActionCode.Success) {
					didSource = true;
					state = 2;
				}

				// and got food
				break;
			case 2: 
				//eaat 
				o.Eat (50);
				if (o.hunger > 40)
					state = 0;
				else
					success = true;
				
				break;
			}

			if (success)
				return ActionCode.Success;
			else if (failed)
				return ActionCode.Failure;
			else
				return ActionCode.Perpetual;
		}

	}

}

