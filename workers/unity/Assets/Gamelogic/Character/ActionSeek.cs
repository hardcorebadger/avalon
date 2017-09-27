using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;

namespace Assets.Gamelogic.Core {

	public class ActionSeek : ActionLocomotion {

		public Vector3 target;
		public EntityId targetId;//optional
		public bool hasTargetEntity = false;

		public ActionSeek(CharacterController o, Vector3 pos) : base(o)	{
			target = pos;
		}

		public ActionSeek(CharacterController o, EntityId eid, Vector3 pos) : base(o)	{
			target = pos;
			targetId = eid;
			hasTargetEntity = true;
		}

		public override ActionCode Update () {
			Vector3 dir = (target - owner.transform.position);
			dir.Normalize ();
			Steer (ref dir);

			Collider[] colliders;

			// do this for either - if the entity ts looking for gets deleted this will trigger saying it got there
			colliders = Physics.OverlapSphere (target, owner.arrivalRadius);
			foreach (Collider c in colliders) {
				if (c.gameObject == owner.gameObject) {
					owner.SetVelocity (0f);
					owner.rigidBody.angularVelocity = Vector3.zero;
					return ActionCode.Success;
				}
			}

			// extra overlap for large collider objects
			if (hasTargetEntity) {
				colliders = Physics.OverlapSphere (owner.transform.position, owner.arrivalRadius);
				foreach (Collider c in colliders) {
					if (c.gameObject.EntityId() == targetId) {
						owner.SetVelocity (0f);
						owner.rigidBody.angularVelocity = Vector3.zero;
						return ActionCode.Success;
					}
				}
			}

			float f = GetRotationTo (dir);
			Vector3 v = owner.facing.eulerAngles;
			v += new Vector3 (0, f, 0);
			owner.facing.eulerAngles = v;
			owner.SetVelocity (owner.speed);

			return ActionCode.Working;
		}

		public override void Log() {
			Vector3 dir = (target - owner.transform.position);
			RaycastHit2D[] hits = Physics2D.CircleCastAll (owner.transform.position, owner.width, dir, owner.range);
			foreach (RaycastHit2D hit in hits) {
				if (hit.collider.gameObject != owner.gameObject && !hit.collider.isTrigger)
					Debug.LogWarning (hit.collider.name);
			}
			Debug.LogWarning (Vector3.Distance (target, owner.transform.position));
		}
	}

}