using UnityEngine;
using System.Collections;

public class CubeBigLeft : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other)
    {        
        //GUIC.StartVisualFeedback(gameObject, "grab");
        GUIC.StartVisualFeedback(gameObject, "touch");         
    }

    private void OnTriggerExit(Collider other)
    {
        //GUIC.StopVisualFeedback("grab");
        GUIC.StopVisualFeedback("touch");
    }

}
