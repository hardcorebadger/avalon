using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Gamelogic.Core {

	public class ActionSeek : ActionLocomotion {

		public Vector3 target;

		public ActionSeek(CharacterController o, Vector3 pos) : base(o)	{
			target = pos;
		}

		public override ActionCode Update () {
			Vector3 dir = (target - owner.transform.position);
			Steer (ref dir);
			float dist = Vector3.Distance(target, owner.transform.position);

			if (dist <= owner.arrivalRadius) {
				owner.rigidBody.velocity = Vector2.zero;
				return ActionCode.Success;
			}
				
			owner.transform.Rotate(new Vector3(0,0,GetRotationTo (dir)));
			owner.rigidBody.velocity = owner.transform.TransformDirection (new Vector2 (0, owner.speed));

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