using UnityEngine;
using System.Collections;

public class CubeBigLeft : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other)
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

    private void OnTriggerExit(Collider other)
    {
        GUIC.StopVisualFeedback("touch");
    }
}
