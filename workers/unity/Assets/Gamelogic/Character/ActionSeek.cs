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
			float dist = dir.magnitude;
			Steer (ref dir);

			if (dist > 5)
				owner.rigidBody.velocity = owner.transform.TransformDirection (new Vector2 (0, owner.speed));
			else if (dist > 3)
				owner.rigidBody.velocity = owner.transform.TransformDirection (new Vector2 (0, owner.speed / 2));
			else {
				owner.rigidBody.velocity = Vector2.zero;
				return ActionCode.Success;
			}

			owner.transform.Rotate(new Vector3(0,0,GetScaledRotation(GetRotationTo (dir))));
			return ActionCode.Working;
		}
	}

}