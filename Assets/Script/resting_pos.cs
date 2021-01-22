using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resting_pos : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other) // in case of collision with leap motion tracked hand and cube gameObject
    {
        //check if it's the collider from the center of the hand
        if (other.name == "vr_hand_R")
        {
            GUIC.marker.Write("hand entered resting position");
            Debug.Log("hand entered resting position");

            //if detection is active -> start trial
            if(GUIC.restingDetectionActive)
            {
                GUIC.marker.Write("Trial start invoked by hand on resting position");
                Debug.Log("Trial start invoked by hand on resting position");

                GUIC.TrialStart();
            }

            //GUIC.handIsOnResting = true;

        }

    }

    private void OnTriggerExit(Collider other)
    {
        //check if it's the collider from the hand
        if (other.name == "vr_hand_R")
        {
            GUIC.marker.Write("hand left resting position");
            Debug.Log("hand left resting position");

            //GUIC.handIsOnResting = false;

        }

    }

}
