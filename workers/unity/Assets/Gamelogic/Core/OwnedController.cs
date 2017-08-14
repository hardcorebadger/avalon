using System.Collections;
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

public class OwnedController : MonoBehaviour {

	[Require] private Owned.Writer ownedWriter;


	// Use this for initialization
	void OnEnable () {
		ownedWriter.CommandReceiver.OnSetOwner.RegisterResponse(OnSetOwner);

	}

	private void OnDisable()
	{
		ownedWriter.CommandReceiver.OnSetOwner.DeregisterResponse();
	}

	// Update is called once per frame
	void Update () {
		
	}

	public int getOwner() {
		return ownedWriter.Data.owner;
	}

	private OwnResponse OnSetOwner(OwnRequest request, ICommandCallerInfo callerinfo) {
		if (this.ownedWriter.Data.owner == 0) {
			ownedWriter.Send (new Owned.Update ()
				.SetOwner (request.owner)
			);
			return new OwnResponse (true);

		} else 
			return new OwnResponse (false);
	}

}
