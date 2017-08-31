using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Assets.Gamelogic.Core {

	public abstract class ActionLocomotion : Action {

		public ActionLocomotion(CharacterController o) : base(o)	{}

		private Vector3 CastPos() {
			return owner.transform.position;
		}

		protected void Steer (ref Vector3 dir) {
			dir.Normalize ();
			Debug.DrawRay (CastPos(), dir*owner.range, Color.red);
			if (!CanWalk (dir, owner.range))
				Avoid (ref dir, owner.range);
			Debug.DrawRay (CastPos(), dir*owner.range, Color.blue);
		}

		protected bool CanWalk(Vector3 dir, float castDist) {
			RaycastHit[] hits = Physics.SphereCastAll (CastPos(), owner.width, dir, owner.range);
			bool flag = true;
			foreach (RaycastHit hit in hits) {
				if (hit.collider.gameObject == owner.gameObject)
					continue;
				if (!hit.collider.isTrigger)
					flag = false;
			}
			return flag;
		}

		protected void Avoid (ref Vector3 dir, float dist) {

			Vector3 dirP = dir;
			Vector3 dirN = dir;
			float tests = 0;
			while (!CanWalk(dirP.normalized, dist)) {
				Rotate (ref dirP, owner.interpolation);
				tests += owner.interpolation;
				if (tests > 360f)
					break;
			}
			tests = 0;
			while (!CanWalk(dirN.normalized, dist)) {
				Rotate (ref dirN, -1*owner.interpolation);
				tests += owner.interpolation;
				if (tests > 360f)
					break;
			}
			if (Vector3.Distance (dirP, owner.GetFacingDirection()) < Vector3.Distance (dirN, owner.GetFacingDirection()))
				dir = dirP;
			else
				dir = dirN;
		}

		protected void Rotate(ref Vector3 v, float theta) {
			theta *= Mathf.Deg2Rad;
			float cos = Mathf.Cos (theta);
			float sin = Mathf.Sin (theta);
			v = owner.transform.InverseTransformDirection (v);
			v = new Vector3(v.x*cos - v.z*sin,0f,v.z*cos + v.x*sin);
			v = owner.transform.TransformDirection (v);
		}

		protected float GetRotationTo(Vector3 dif) {
			float deg = (float)(Mathf.Acos (Vector3.Dot (owner.GetFacingDirection(), dif.normalized)) * 180 / 3.14);
			Vector3 cross = Vector3.Cross (owner.GetFacingDirection(), dif.normalized).normalized;

			if (Single.IsNaN (deg))
				return 0f;
			if (cross.y < 0)
				return (float)deg * -1f;
			else
				return (float)deg;
		}

		protected float GetScaledRotation(float rotation) {
			if (Mathf.Abs(rotation) < owner.maxRotation*Time.deltaTime)
				return rotation;
			if (rotation < -owner.maxRotation*Time.deltaTime)
				return -owner.maxRotation*Time.deltaTime;
			return owner.maxRotation*Time.deltaTime;
		}

	}

}