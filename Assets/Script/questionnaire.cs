using UnityEngine;
using System.Collections;

public class questionnaire : MonoBehaviour {

    public GUIControl GUIC;

    void OnTriggerEnter(Collider other) // in case of collision with leap motion tracked hand and cube gameObject
    {
        //GUIC.QuestMarker(gameObject);         //[BPA:removed questionaire]
        //GUIC.VisualFeeback(gameObject);
    }
}
