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

	public class ActionDamage : ActionLocomotion {

		public EntityId targetId;//optional
		public int stage = 0;
		public ActionSeek seek;
		public GameObject targetObject;
		private float time = 4f;
		private float timeMax = 3f;

		public ActionDamage(CharacterController o, EntityId eid) : base(o)	{
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
					} else {

					}
				}
				break;
			case 3:
				time += Time.deltaTime;
				if (time > timeMax) {
					stage = 4;
					time = 0;
				}
				seek.Update ();
				break;
			case 4: 
				seek.Update ();
				owner.anim.SetTrigger ("attack");
				owner.characterWriter.Send (new Character.Update ()
					.AddShowHit(new Nothing())
				);
				stage = 5;
				break;
			case 5: 
				seek.Update ();
				break;			
			}

			if (targetObject != null) {
				return ActionCode.Perpetual;
			} else {
				return ActionCode.Success;
			}
		}

		public override void OnDealHit () {
			base.OnDealHit ();
			SpatialOS.Commands.SendCommand (owner.characterWriter, Building.Commands.ReceiveDamage.Descriptor, new ReceiveDamageRequest(owner.characterWriter.EntityId, owner.owned.getOwner(), owner.owned.getOwnerObject()), targetId);
			stage = 2;
			timeMax = Random.Range (1.0f, 2.0f);

		}

	}

}