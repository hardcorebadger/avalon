﻿using System.Collections;
using System.Collections.Generic;
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

	public class ActionResourcePut : Action {

		private SourcingOption sourcing;

		public ActionResourcePut(CharacterController o) : base(o)	{}

		public override ActionCode Update () {
			return ActionCode.Perpetual;
		}
	}

}
