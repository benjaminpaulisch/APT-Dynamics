using UnityEngine;
using System.Collections;

public class start_first_trial : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other) // in case of collision with leap motion tracked hand and cube gameObject
    {
        //check if it's the collider from the fingertip
        if (other.name == "f_index.03_end")
        {
            GUIC.marker.Write("start first trial button pressed");
            Debug.Log("start first trial button pressed");

            //GUIC.TrialStart();
            GUIC.StartFirstTrial();

        }

    }
}
