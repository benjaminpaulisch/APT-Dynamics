using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageSwitcher : MonoBehaviour {

	public GUIControl GUIC;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void OnTriggerEnter(Collider other) // in case of collision with leap motion tracked hand and cube gameObject
	{
		//check if it's the collider from the fingertip
		if (other.name == "f_index.03_end")
		{
			GUIC.marker.Write("next page button pressed");
			Debug.Log("next page button pressed");

			GUIC.NextInstructionPage();

		}

	}

}
