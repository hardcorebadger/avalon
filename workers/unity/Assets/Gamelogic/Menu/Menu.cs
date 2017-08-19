using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour {

	public InputField[] inputs;
	private EventSystem eventSystem;

	// Use this for initialization
	public virtual void Start() {
		this.eventSystem = EventSystem.current;

	}
	
	// Update is called once per frame
	public virtual void Update() {
		HandleInputs ();

	}


	/*
		 *	Handles tab key and enter, using inputs array set in editor
		 */
	private void HandleInputs() {

		Selectable current = null;

		// Figure out if we have a valid current selected gameobject
		if (eventSystem.currentSelectedGameObject != null) {
			// Unity doesn't seem to "deselect" an object that is made inactive
			if (eventSystem.currentSelectedGameObject.activeInHierarchy) {
				current = eventSystem.currentSelectedGameObject.GetComponent<Selectable> ();
			}
		}
		int index = 0;
		if (current != null) {
			for (int x = 0; x < inputs.Length; x++) {
				InputField i = inputs [x];
				if (current.name == i.name) {
					index = x;
				}
			}
		}
		if (Input.GetKeyDown (KeyCode.Tab) && inputs.Length != 0) {
			Selectable next = null;

			if (current != null) {
				int newIndex = index + 1;
				if (newIndex > inputs.Length - 1) {
					//restart at top
					next = inputs [0];
				} else {
					next = inputs [newIndex];
				}
			} else if (inputs.Length >= 1) {
				next = inputs [0];
			}

			if (next != null) {
				next.Select ();
			}

		}

		if (Input.GetKeyDown (KeyCode.Return) && current != null) {
			OnEnter ();
		}
	}

	/*
		 *	Triggered when enter key pressed while input field selected (input field MUST be in inputs array)
		 */
	protected virtual void OnEnter() {

	}
		
	public virtual void OnPointerDown(PointerEventData eventData, int id)
	{
	}

	public virtual void OnPointerUp(PointerEventData eventData, int id)
	{

	}
}
