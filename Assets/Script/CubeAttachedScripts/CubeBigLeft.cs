using UnityEngine;
using System.Collections;

public class CubeBigLeft : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other)
    {
        //check if it's the collider from the fingertip
        if (other.name == "f_index.03_end")
        {
            //in Learning only start correct feedback!
            if (GUIC.learningStarted)
            {
                if (GUIC.currentTask == "touch")
                {
                    GUIC.StartVisualFeedback(gameObject, "touch");
                }
            }
            else
            {
                GUIC.StartVisualFeedback(gameObject, "touch");
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        //check if it's the collider from the fingertip
        if (other.name == "f_index.03_end")
        {
            GUIC.StopVisualFeedback("touch");
        }
    }
}
