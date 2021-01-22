using UnityEngine;
using System.Collections;

public class start_pos_hand : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other) // in case of collision with leap motion tracked hand and cube gameObject
    {
        GUIC.marker.Write("continue button pressed after a break");
        Debug.Log("continue button pressed after a break");

        //GUIC.TrialStart();
        GUIC.ContinueAfterBreak();

    }
}
