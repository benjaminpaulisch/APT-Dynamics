﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IdleChanger : MonoBehaviour
{
	
	private Animator anim;						
	private AnimatorStateInfo currentState;		
	private AnimatorStateInfo previousState;	

	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator> ();
		currentState = anim.GetCurrentAnimatorStateInfo (0);
		previousState = currentState;

		anim.SetBool("Next", true);	//switch to pointy finger gesture
	}
	// Update is called once per frame
	void  Update ()
	{
		if (Input.GetKeyDown ("up")) {
			anim.SetBool ("Next", true);
		}
		
		if (Input.GetKeyDown ("down")) {
			anim.SetBool ("Back", true);
		}
		
		if (anim.GetBool ("Next")) {
			currentState = anim.GetCurrentAnimatorStateInfo (0);
			if (previousState.nameHash != currentState.nameHash) {
				anim.SetBool ("Next", false);
				previousState = currentState;				
			}
		}
		
		if (anim.GetBool ("Back")) {
			currentState = anim.GetCurrentAnimatorStateInfo (0);
			if (previousState.nameHash != currentState.nameHash) {
				anim.SetBool ("Back", false);
				previousState = currentState;
			}
		}


		if (anim.GetBool ("Fist")) {
			currentState = anim.GetCurrentAnimatorStateInfo (0);
			if (previousState.nameHash != currentState.nameHash) {
				anim.SetBool ("Fist", false);
				previousState = currentState;				
			}
		}
		
		if (anim.GetBool ("Point")) {
			currentState = anim.GetCurrentAnimatorStateInfo (0);
			if (previousState.nameHash != currentState.nameHash) {
				anim.SetBool ("Point", false);
				previousState = currentState;
			}
		}
	}


	public void GotoFist()
    {
		anim.SetBool ("Fist", true);
    }

	public void GotoPoint()
    {
		anim.SetBool ("Point", true);
    }

}


