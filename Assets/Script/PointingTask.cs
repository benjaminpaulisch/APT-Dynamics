using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointingTask : MonoBehaviour {

    //[BPA: created after the german tutorial https://viscircle.de/wie-sie-ein-raycast-in-unity-3d-erstellen-koennen/]

    private RaycastHit vision;          //used for detecting raycast collision
    public float rayLength = 4.0f;      //used for assigning a length to the raycast
    private Rigidbody pointedObject;    //used to assign the object we're pointing at to a variable we can use

    public GUIControl GUIC;


    private GameObject currentHitObject = null;
    private bool hitActive = false;

    // Use this for initialization
    void Start () {
        //rayLength = 4.0f;
	}
	
	// Update is called once per frame
	void Update () {

        //only when raycast is set to active
        if (GUIC.activateRaycast)
        {
            //This will constantly draw the ray in our scene view so we can see where the ray is going
            Debug.DrawRay(gameObject.transform.position, gameObject.transform.up * rayLength, Color.blue, 0.2f);

            //This statement is called when the raycast is hitting a collider in the scene
            if (Physics.Raycast(gameObject.transform.position, gameObject.transform.up, out vision, rayLength))
            {

                //determine if the object our raycast is hitting has the "pointable" tag
                if (vision.collider.tag == "pointable") {

                    //check if it's a new hit
                    if (!hitActive)
                    {
                        /*in Learning only start correct feedback!
                        if (GUIC.learningStarted)
                        {
                            if (GUIC.currentTask == "point")
                            {
                                hitActive = true;
                                //Debug.Log(vision.collider.name);    //output the name of the object our raycast is hitting
                                GUIC.StartVisualFeedback(vision.collider.gameObject, "point");
                            }
                        }
                        else*/
                        {
                            hitActive = true;
                            //Debug.Log(vision.collider.name);    //output the name of the object our raycast is hitting
                            GUIC.StartVisualFeedback(vision.collider.gameObject, "point");
                        }
                    }
                }
            
            }
            //no hit
            else
            {
                //check if was a hit before
                if (hitActive)
                {
                    hitActive = false;
                    GUIC.StopVisualFeedback("point");
                }
            }
        }
        //hand is moving
        else
        {
            //check if was a hit before
            if (hitActive)
            {
                hitActive = false;
                GUIC.StopVisualFeedback("point");
            } 
        }

    }
}
