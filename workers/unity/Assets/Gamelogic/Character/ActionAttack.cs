using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
using Improbable.Unity.Entity;
namespace Assets.Gamelogic.Core {

	public class ActionAttack : ActionLocomotion {

		public EntityId targetId;//optional
		public int stage = 0;
		public ActionSeek seek;
		public CharacterController controller;
		public GameObject targetObject;

		public ActionAttack(CharacterController o, EntityId eid) : base(o)	{
			targetId = eid;
			controller = o;
		}

		public override ActionCode Update () {
			switch (stage) {
			case 0:
				//find object
				if (LocalEntities.Instance.ContainsEntity (targetId)) {
					IEntityObject g = LocalEntities.Instance.Get (targetId);
					targetObject = g.UnderlyingGameObject;
					seek = new ActionSeek (controller, targetId, targetObject.transform.position);
					stage = 2;
				} else {
					//must query
				}
				break;
			case 1:
				//wait for query?
				break;
			case 2:
				seek.target = targetObject.transform.position;
				ActionCode seekCode = seek.Update ();
				if (seekCode == ActionCode.Success) {
					//can attack
				} else {

				}
				break;

			}
			return ActionCode.Perpetual;
		}
	
	}

}