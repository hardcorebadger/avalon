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
		public CharacterController owner;
		public GameObject targetObject;
		private float time = 0;

		public ActionAttack(CharacterController o, EntityId eid) : base(o)	{
			targetId = eid;
			owner = o;
		}

		public override ActionCode Update () {
			switch (stage) {
			case 0:
				//find object
				if (LocalEntities.Instance.ContainsEntity (targetId)) {
					IEntityObject g = LocalEntities.Instance.Get (targetId);
					targetObject = g.UnderlyingGameObject;
					seek = new ActionSeek (owner, targetId, targetObject.transform.position);
					stage = 2;
				} else {
					//must query
				}
				break;
			case 1:
				//wait for query?
				break;
			case 2:
				if (targetObject != null) {
					seek.target = targetObject.transform.position;
					ActionCode seekCode = seek.Update ();
					if (seekCode == ActionCode.Success) {
						//can attack
						stage = 3;
						time = 0;
						Debug.LogWarning ("Starting timer");

					} else {

					}
				}
				break;
			case 3:
				time += Time.deltaTime;
				if (time > 6f) {
					stage = 4;
					time = 0;
					Debug.LogWarning ("Calling og hit");

				}
				seek.Update ();
				break;
			case 4: 
				seek.Update ();
				owner.anim.SetTrigger ("attack");
				stage = 5;
				break;
			case 5: 
				seek.Update ();
				Debug.LogWarning ("waiting for hit");

				break;

			
			}
			if (targetObject != null) {
				
				return ActionCode.Perpetual;
			} else {
				Debug.LogWarning ("WHY?");
				return ActionCode.Success;
			}
		}

		public override void OnDealHit () {
			base.OnDealHit ();
			Debug.LogWarning ("CALLING HIT");
			SpatialOS.Commands.SendCommand (owner.characterWriter, Character.Commands.ReceiveHit.Descriptor, new ReceiveHitRequest(owner.characterWriter.EntityId), targetId);
			stage = 2;

		}
	
	}

}