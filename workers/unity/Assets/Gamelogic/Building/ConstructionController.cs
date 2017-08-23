﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core {
	
	public class ConstructionController : MonoBehaviour {

		[Require] private Construction.Writer constructionWriter;

		private Dictionary<int,Requirement> requirements;
		private OwnedController owned;

		// Use this for initialization
		void OnEnable () {

			constructionWriter.CommandReceiver.OnGive.RegisterResponse(OnGive);
			constructionWriter.CommandReceiver.OnGiveMultiple.RegisterResponse(OnGiveMultiple);

			requirements = new Dictionary<int,Requirement> ();
			UnwrapComponentRequirements ();

			owned = GetComponent<OwnedController> ();

		}

		void OnDisable() {
			constructionWriter.CommandReceiver.OnGive.DeregisterResponse();
			constructionWriter.CommandReceiver.OnGiveMultiple.DeregisterResponse();
		}

		private GiveResponse OnGive(ItemStack itemStack, ICommandCallerInfo callerinfo) {
			bool f = Insert (itemStack.id, itemStack.amount);
			CheckConstructionProgress ();
			return new GiveResponse (f);
		}

		private GiveResponse OnGiveMultiple(ItemStackList itemStackList, ICommandCallerInfo callerinfo) {
			foreach (int id in itemStackList.inventory.Keys) {
				if (!requirements.ContainsKey (id))
					return new GiveResponse (false);
			}
			foreach (int id in itemStackList.inventory.Keys) {
				Insert (id, itemStackList.inventory [id]);
			}
			CheckConstructionProgress ();
			return new GiveResponse (true);
		}

		private void UnwrapComponentRequirements() {
			foreach (int key in constructionWriter.Data.requirements.Keys) {
				ConstructionRequirement val = constructionWriter.Data.requirements[key];
				requirements.Add (key, new Requirement(val.amount, val.required));
			}
		}

		private Improbable.Collections.Map<int,ConstructionRequirement> WrapComponentRequirements() {
			Improbable.Collections.Map<int,ConstructionRequirement> wrapped = new Improbable.Collections.Map<int,ConstructionRequirement> ();
			foreach (int key in requirements.Keys) {
				Requirement val = requirements[key];
				wrapped.Add (key, new ConstructionRequirement(val.amount, val.required));
			}
			return wrapped;
		}

		private void SendRequirementsUpdate() {
			constructionWriter.Send (new Construction.Update ()
				.SetRequirements (WrapComponentRequirements())
			);
		}

		public void Log() {
			foreach (int key in requirements.Keys) {
				Requirement val = requirements[key];
				Debug.Log(Item.GetName (key) + ": " + val.amount + " / " + val.required);
			}
		}

		public bool Insert(int id, int amount) {
			if (!requirements.ContainsKey (id))
				return false;

			Requirement r = requirements [id];
			r.amount += amount;
			requirements [id] = r;
			if (r.amount > r.required)
				Debug.LogWarning ("construction overfilling - item loss will occur");

			Log ();
			SendRequirementsUpdate ();
			return true;
		}

		private void Clear() {
			requirements.Clear ();
			SendRequirementsUpdate ();
		}

		public int Count(int i) {
			Requirement req;
			requirements.TryGetValue (i, out req);
			return req.amount;
		}

		private void CheckConstructionProgress() {
			foreach (int key in requirements.Keys) {
				Requirement val = requirements[key];
				if (val.amount < val.required)
					return;
			}
			// fully stocked
			SpatialOS.Commands.CreateEntity (constructionWriter, EntityTemplates.EntityTemplateFactory.CreateEntityTemplate ("construction-house", transform.position, owned.getOwner()))
				.OnSuccess (entityId => OnHouseCreated ());
		}

		private void OnHouseCreated() {
			SpatialOS.WorkerCommands.DeleteEntity (gameObject.EntityId());
		}

		public struct Requirement {
			public int amount;
			public int required;
			public Requirement(int a, int r) {
				amount = a;
				required = r;
			}
		}
	}
		
}