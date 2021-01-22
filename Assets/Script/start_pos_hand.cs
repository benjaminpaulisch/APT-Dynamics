using UnityEngine;
using System.Collections;

public class start_pos_hand : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other) // in case of collision with leap motion tracked hand and cube gameObject
    {
        //check if it's the collider from the fingertip
        if (other.name == "f_index.03_end")
        {
            GUIC.marker.Write("continue button pressed after a break");
            Debug.Log("continue button pressed after a break");

            //GUIC.TrialStart();
            GUIC.ContinueAfterBreak();

        }

    }
}
