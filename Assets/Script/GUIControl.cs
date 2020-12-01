using UnityEngine;
using System.Collections;
using Assets.LSL4Unity.Scripts; // reference the LSL4Unity namespace to get access to all classes
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
//using Uniduino; // adding arduino     //[BPA: removed vibro feedback]

public class GUIControl : MonoBehaviour {

    // create LSL markerstream
    public LSLMarkerStream marker;
    
    public static GameObject table, plane, leapmotion, introGUI, textBox, endexp, startContinue, resting, questionnaire, q, ra, la, send, fixationCross, cue, cueText,
        mainMenu, calibrationMenu, configurationMenu, inputParticipantID, inputParticipantAge, inputParticipantGender, inputArmLength, buttonExperiment,
        buttonTraining, buttonShoulderPos, textHintShoulderPos, textMissingInputs, tableSetup, buttonMaximumReach, buttonCupPositions, buttonTablePosition, textHintShoulderFirst,
        textHintCupPos, textHintTablePos, breakCanvasVR, breakCanvasDesktop;

    //GameObject cubeMiddle, cubeLeft, cubeRight;
    private GameObject cubeFarLeft, cubeFarMiddleLeft, cubeFarMiddle, cubeFarMiddleRight, cubeFarRight, cubeNearLeft, cubeNearMiddleLeft, cubeNearMiddle, cubeNearMiddleRight, cubeNearRight;

    //GameObject[] cubeGameObjArr = new GameObject[3];  // Game Object Array
    private GameObject[] cubeGameObjArr = new GameObject[10];  // Game Object Array
    private string[] stimulusPositions;

    // main GUI/Experiment parameters
    public int nrOfTrialsPerTask = 100;
    private int nrOfTrialsTotal;
    private bool training = false;
    //public int repeatNrOfTrials = 1;
    //[HideInInspector] // Hides var below
    //private int repetition = 1;
    //[HideInInspector] // Hides var below
    private int currentBlock = 0;
    //public int volatility = 1;
    //public float isiTime = -5f;

    //new
    public float fixationDuration = 1f;             //500ms fixation cross is visible
    public float cueDurationAvg = 1.5f;             //1.5s cue average duration
    public float cueDurationVariation = 0.5f;       //500ms variation (so the cue duration is 1500ms +- 500ms
    public float stimulusDurationMax = 5.0f;          //4s max target is visible
    public float feedbackDuration = 2.0f;           //1s feedback duration
    public float minimumTaskDuration = 1.0f;   //1s minimum collision duration for a successful response


    public static float[] cueDurations;
    private float currentCueDuration;           //stores indivudual cue duration of the current trial

    //public static string[] tasks = new string[4] {"grabFar", "grabNear", "pointFar", "pointNear"};
    public static string[] tasks = new string[4] { "touchFar", "touchNear", "pointFar", "pointNear" };
    public static int[] taskSeq = new int[] { 0, 1, 2, 3 };
    public static int[] trialTasks;
    private string currentTask;
    private string currentCondition;
    private GameObject currentStimulusObj;

    // Cube Seq if only Bigger cube has been used
    public static int[] CubeSeq = new int[] { 0, 1, 2, 3, 4};   //using the same array for near and far cause it's easier
    public static int[] CubePositions = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public static int[] angles = new int[] {-40, -20, 0, 20, 40};
    public int offsetNearPercent = 50;
    public int offsetFarPercent = 20;

    float sphereRadius = 0;
    public static float actualTime = 0; // start timer at 0

    private int[] NormalConflictArr;
    
    public static int trialSeqCounter;
    public static int cubeSeqCounter = 0;
    
    [HideInInspector] // Hides var below
    public bool flagTouchEvent = false;
    public static bool flagStart = false;
    
    // trial logic handler
    public static bool updateEndOfTrialLogic = false;
    public static bool experimentEnd = false;
    private bool endUpdate = true;

    public static bool experimentStarted = false;
    public static bool fixationCrossActivated = false;
    public static bool cueActivated = false;
    public static bool targetActivated = false;
    public static bool taskSuccess = false;
    public static bool collisionActive = false;
    public static string currentResponseType = "none";
    public static System.Diagnostics.Stopwatch collisionDuration = new System.Diagnostics.Stopwatch();
    private static GameObject currentCollisionObj = null;
    public static bool visualFeedbackActive = false;
    private static Color32 customYellow = new Color32(255, 191, 0, 255);
    private int expControlStatus;
    private string participantID;
    private int participantAge;
    private string participantGender;
    private float armLength;
    private float armLengthCalculated;
    public static bool idSet = false;
    public static bool ageSet = false;
    public static bool genderSet = false;
    public static bool armLengthSet = false;
    public static bool shoulderSet = false;
    public static bool maxReachSet = false;
    public static bool cupPositionsSet = false;
    public static bool tablePosSet = false;
    private bool trackerFoundShoulderPos = true;
    private bool trackerFoundMaxReach = true;
    private bool trackerFoundCupPos = true;

    private Vector3 shoulderPosition;
    private Vector3 maxReachPosition;

    // parameter variables
    public static float reaction_time = 0;
    public static float reaction_time_temp = 0;
    public static float reaction_start_time = 0;

    //break timer
    public static float breakDurationCountdown;

    private string tempMarkerText = "";


    // Use this for initialization
    void Start()
    {
        //calculate total number of trials
        nrOfTrialsTotal = nrOfTrialsPerTask * tasks.Length;

        NormalConflictArr = new int[nrOfTrialsTotal];


        // init LSL markerstream
        marker = FindObjectOfType<LSLMarkerStream>();
        
        // Finding the game object 
        table = GameObject.Find("Table");
        plane = GameObject.Find("Plane");
        leapmotion = GameObject.Find("LeapHandController");
        introGUI = GameObject.Find("IntroGUI");
        textBox = GameObject.Find("TextBox");
        endexp = GameObject.Find("End");
        startContinue = GameObject.Find("StartContinue");
        resting = GameObject.Find("resting");
        fixationCross = GameObject.Find("FixationCross");
        cue = GameObject.Find("Cue");
        cueText = GameObject.Find("CueText");
        //shoulder = GameObject.Find("Shoulder");
        mainMenu = GameObject.Find("MainMenu");
        calibrationMenu = GameObject.Find("CalibrationMenu");
        configurationMenu = GameObject.Find("ConfigurationMenu");
        inputParticipantID = GameObject.Find("InputParticipantID");
        inputParticipantAge = GameObject.Find("InputParticipantAge");
        inputParticipantGender = GameObject.Find("DropdownGender");
        inputArmLength = GameObject.Find("InputParticipantArmLength");
        buttonExperiment = GameObject.Find("ButtonExperiment");
        buttonTraining = GameObject.Find("ButtonTraining");
        buttonShoulderPos = GameObject.Find("ButtonShoulderPos");
        textHintShoulderPos = GameObject.Find("TextHintShoulderPos");
        textMissingInputs = GameObject.Find("TextMissingInputs");
        tableSetup = GameObject.Find("TableSetup");
        buttonMaximumReach = GameObject.Find("ButtonMaximumReach");
        buttonCupPositions = GameObject.Find("ButtonSetCupPositions");
        buttonTablePosition = GameObject.Find("ButtonTablePos");
        textHintShoulderFirst = GameObject.Find("TextHintShoulderFirst");
        textHintCupPos = GameObject.Find("TextHintCupPos");
        textHintTablePos = GameObject.Find("TextHintTablePos");
        breakCanvasVR = GameObject.Find("BreakCanvasVR");
        breakCanvasDesktop = GameObject.Find("BreakCanvasDesktop");

        //Stimulus
        cubeFarLeft = GameObject.Find("CubeFarLeft");
        cubeFarMiddleLeft = GameObject.Find("CubeFarMiddleLeft");
        cubeFarMiddle = GameObject.Find("CubeFarMiddle");
        cubeFarMiddleRight = GameObject.Find("CubeFarMiddleRight");
        cubeFarRight = GameObject.Find("CubeFarRight");
        cubeNearLeft = GameObject.Find("CubeNearLeft");
        cubeNearMiddleLeft = GameObject.Find("CubeNearMiddleLeft");
        cubeNearMiddle = GameObject.Find("CubeNearMiddle");
        cubeNearMiddleRight = GameObject.Find("CubeNearMiddleRight");
        cubeNearRight = GameObject.Find("CubeNearRight");


        // Assign stimulus game objects to arrays
        cubeGameObjArr[0] = cubeFarLeft;
        cubeGameObjArr[1] = cubeFarMiddleLeft;
        cubeGameObjArr[2] = cubeFarMiddle;
        cubeGameObjArr[3] = cubeFarMiddleRight;
        cubeGameObjArr[4] = cubeFarRight;
        cubeGameObjArr[5] = cubeNearLeft;
        cubeGameObjArr[6] = cubeNearMiddleLeft;
        cubeGameObjArr[7] = cubeNearMiddle;
        cubeGameObjArr[8] = cubeNearMiddleRight;
        cubeGameObjArr[9] = cubeNearRight;

        stimulusPositions = new string[cubeGameObjArr.Length];

        // Randomize Cube Appearance Sequence
        RandomizeArray.ShuffleArray(CubeSeq);


        //create array with tasks for all trials
        trialTasks = new int[nrOfTrialsTotal];

        //Fill array with tasks. For example: 4 different tasks and 100 trials per task - the array should contain 100 values of each task.
        //We use a method minimizing task repetition in subsequent trials.
        int tempTrialCounter = 0;
        int tempTaskCounter = 0;
        RandomizeArray.ShuffleArray(taskSeq);

        //Debug.Log("Task sequence:");

        while(tempTrialCounter < trialTasks.Length)
        {
            if(tempTaskCounter >= taskSeq.Length)
            {
                tempTaskCounter = 0;
                RandomizeArray.ShuffleArray(taskSeq);
            }

            trialTasks[tempTrialCounter] = taskSeq[tempTaskCounter];
            //Debug.Log(tasks[taskSeq[tempTaskCounter]]);

            tempTaskCounter++;
            tempTrialCounter++;

        }

        /* //Debug: print out the array:
        Debug.Log("Shuffled trialTasks array:");
        for(int i=0; i<trialTasks.Length; i++)
        {
            Debug.Log(tasks[trialTasks[i]]);
        }*/


        //generate list of cue times for all trials:
        cueDurations = new float[nrOfTrialsTotal];

        //Debug.Log("All cue durations:");
        for(int i=0; i<nrOfTrialsTotal; i++)
        {
            //the goal here is to get linear distributed values in the range of cueDurationAvg-cueDuRationVariation and cueDurationAvg+cueDuRationVariation
            cueDurations[i] = i * (cueDurationVariation*2 / (nrOfTrialsTotal-1)) + cueDurationAvg - cueDurationVariation;
            //Debug.Log(cueDurations[i].ToString());
        }
        //shuffle cue duration order
        RandomizeArray.ShuffleArray(cueDurations);


        //deactivate the "Start Experiment" and "Training" Buttons:
        buttonExperiment.GetComponent<Button>().interactable = false;
        buttonTraining.GetComponent<Button>().interactable = false;

        textHintTablePos.SetActive(false);
        textHintShoulderPos.SetActive(false);
        endexp.SetActive(false);

        //start the Main Menu:
        StartMainMenu();

    }//start()
    

    // Disable All Cubes rendering and collider properties
    public void DeactivateAllCubes()
    {
        //Deactivate all Stimulus and change color to white to all Big Cubes
        foreach (GameObject obj in cubeGameObjArr)
        {
            obj.SetActive(false);
            obj.GetComponent<Renderer>().material.color = Color.white;
        }

    }

    public void ActivateAllCubes()
    {
        foreach (GameObject obj in cubeGameObjArr)
        {
            obj.SetActive(true);
        }

    }


    // Start of experiment
    public void ControlState()
    {    
        //if (Input.GetMouseButtonDown(0))
        // after hitting any key once, the Input.anyKeyDown is otherwise false for every frame update
        // therefore the if clause code is only run once at the beginning when hitting any key to start
        //{
            // run experiment
            flagStart = true;
            experimentEnd = false;
            currentBlock += 1;
            trialSeqCounter = 0;

            // Making instruction invisible in scene and start rendering table and plane
            table.gameObject.GetComponent<Renderer>().enabled = true;
            plane.gameObject.GetComponent<Renderer>().enabled = true;
            introGUI.gameObject.GetComponent<Canvas>().enabled = false;
            //endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
            endexp.SetActive(false);
            //questionnaire.SetActive(false);

            // enable collision possibility
            startContinue.SetActive(true);
            resting.gameObject.GetComponent<Renderer>().enabled = true;

            //write experiment start marker
            tempMarkerText = 
                "experiment:start;" +
                "trialsPerTask:" + nrOfTrialsPerTask.ToString() + ";" +
                "trialsTotal:" + nrOfTrialsTotal.ToString() + ";" +
                "fixationDuration:" + fixationDuration.ToString() + ";" +
                "cueDurationAvg:" + cueDurationAvg.ToString() + ";" +
                "cueDurationVariation:" + cueDurationVariation.ToString() + ";" +
                "stimulusDurationMax:" + stimulusDurationMax.ToString() + ";" +
                "feedbackDuration:" + feedbackDuration.ToString() + ";" +
                "minTaskDuration:" + minimumTaskDuration.ToString() + ";" +
                "offsetNearPercent:" + offsetNearPercent.ToString() + ";" +
                "offsetFarPercent:" + offsetFarPercent.ToString();
            marker.Write(tempMarkerText);
            Debug.Log(tempMarkerText);

            //write participant info (from configuration menu)
            tempMarkerText =
                "participantID:" + participantID + ";" +
                "participantAge:" + participantAge.ToString() + ";" +
                "participantGender" + participantGender + ";" +
                "participantArmLength" + armLength;
            marker.Write(tempMarkerText);
            Debug.Log(tempMarkerText);

            //write calibration info (from calibration menu)
            tempMarkerText =
                "posTable:" + table.transform.position.ToString() + ";" +
                "posShoulder:" + shoulderPosition.ToString() + ";" +
                "posMaxReach:" + maxReachPosition.ToString() + ";" +
                "armLengthCalculated:" + armLengthCalculated.ToString() + ";" +
                "stimulusPositions:" + stimulusPositions.ToString();
            marker.Write(tempMarkerText);
            Debug.Log(tempMarkerText);
        //}
    }

    //  Control all trials 
    public void ControlTrial()
    {
        //Check if flag has been activated through TrialStart() via touching the startButton in VR
        if (experimentStarted)
        {
            // add the time taken to render last frame, experiment logic is based on this parameter
            // actualTime is constantly growing
            actualTime += Time.deltaTime;

            if (trialSeqCounter < nrOfTrialsTotal && !experimentEnd) // run all trials
            {
                // the following loop is then run each frame for a new started trial

                //Start of trial: show fixation cross
                if (actualTime <= fixationDuration)
                {
                    // do once to update sequence of trials and to write marker

                    //should run only once at trial start
                    if (!fixationCrossActivated)
                    {
                        //enable fixation cross
                        fixationCross.SetActive(true);
                        marker.Write("Fixation cross activated");
                        Debug.Log("Fixation Cross activated: " + actualTime.ToString());
                        fixationCrossActivated = true;

                        // Enable flag to detect events of GameObject Cube
                        flagTouchEvent = true;  //[ToDo: is this still used anywhere?]

                        //GUIControl.marker.Write("box:spawned;condition:" + feedback_type + ";trial_nr:" + (((currentBlock - 1) * 100) + trialSeqCounter + 1) + ";normal_or_conflict:" + normConflict + ";cube:" + cubeGameObjArr[CubeSeq[cubeSeqCounter]] + ";isiTime:" + (isiTime + 6));
                    }
                }

                //after fixation cross: show cue
                if (actualTime > fixationDuration && actualTime <= fixationDuration + currentCueDuration)
                {
                    if (!cueActivated)
                    {
                        //deactivate fixation cross
                        fixationCross.SetActive(false);
                        marker.Write("Fixation cross deactivated");
                        Debug.Log("Fixation Cross deactivated: " + actualTime.ToString());
                        fixationCrossActivated = false;
                        
                        //activate cue and set correct cue text
                        cueText.GetComponent<UnityEngine.UI.Text>().text = currentTask;
                        marker.Write("cueTextActivated:" + currentTask);
                        Debug.Log("Cue activated: " + currentTask + " " + actualTime.ToString());

                        cue.SetActive(true);
                        cueActivated = true;
                    }
                }

                //after showing cue: show stimulus
                if (actualTime > fixationDuration + currentCueDuration)
                {
                    if(!taskSuccess)    //if the current task has not been successful yet
                    {
                        if (!targetActivated)
                        {
                            //deactivate cue
                            cue.SetActive(false);
                            marker.Write("cue text deactivated");
                            Debug.Log("Cue deactivated: " + actualTime.ToString());
                            cueActivated = false;

                            //activate stimulus
                            CubeVisible(currentStimulusObj);
                            Debug.Log("Stimulus activated: " + actualTime.ToString());

                            //set reaction start time
                            reaction_start_time = actualTime;

                            targetActivated = true;
                            
                        }

                        //check for successful response (if the collision has reached the minimum duration)
                        if (collisionActive)
                        {
                            if ((float)collisionDuration.ElapsedMilliseconds/1000 >= minimumTaskDuration)
                            {
                                //activate correct/incorrect visual feedback
                                visualFeedbackActive = true;
                                VisualFeeback(currentCollisionObj, currentResponseType);
                            }
                        }
                        //if time for a response has run out -> go to next trial (but NOT if there is an active collision)
                        else if(actualTime > fixationDuration + currentCueDuration + stimulusDurationMax)
                        {
                            marker.Write("response time over");
                            Debug.Log("response time over. " + actualTime.ToString());
                            
                            DeactivateAllCubes();
                            NextTrial();
                        }

                    }
                    //if the current task has been successful
                    else
                    {
                        //wait for visual feedback to finish and then -> go to next trial
                        if (actualTime > fixationDuration + currentCueDuration + reaction_time + minimumTaskDuration + feedbackDuration)
                        {
                            //deactivate stimulus
                            DeactivateAllCubes();

                            marker.Write("visual feeback duration over");
                            Debug.Log("visual feeback duration over");

                            marker.Write("visualFeedback:off");
                            Debug.Log("visualFeedback:off " + actualTime.ToString());

                            //transition to next trial
                            NextTrial();
                        }
                    }
 
                }

            }//run all trials


            if (experimentEnd)  // after all trials are finished
            {
                //write expemriment end marker
                marker.Write("experiment:end");
                Debug.Log("experiment:end");

                //activate experiment end text
                endexp.SetActive(true);

                experimentStarted = false;

                //go to main menu
                StartMainMenu();
            }

            /*
            // experiment end determined by nrOfTrials
            if (trialSeqCounter == nrOfTrialsTotal && actualTime > 0 && !experimentEnd) // after all trials are finished
            {
                // block end
                //if (repetition < repeatNrOfTrials)
                //{
                    //repetition += 1;
                    endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = true;
                    DeactivateAllCubes();
                    startContinue.SetActive(false);
                    flagStart = false;
                //}
            }

            if (experimentEnd && actualTime > 0 && endUpdate) //!experimentEnd
            {
                //questionnaire.SetActive(false);
                table.gameObject.GetComponent<Renderer>().enabled = false;
                plane.gameObject.GetComponent<Renderer>().enabled = false;
                resting.gameObject.GetComponent<Renderer>().enabled = false;
                endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = true;

                //GUIControl.marker.Write("block:end;currentBlockNr:" + currentBlock + ";condition:" + feedback_type + ";training:" + BoolToString(training));
                //Debug.Log("block:end;currentBlockNr:" + currentBlock + ";condition:" + feedback_type + ";training:" + BoolToString(training));
                endUpdate = false;
            }
            */

        }//if experimentStarted
   
    }//ControlTrial()


    public void TrialStart()
    {
        // disable collide on start disc
        startContinue.SetActive(false);

        //set experiment control status to experiment (needed here when coninuing after break)
        //expControlStatus = 4;

        // reset trial time
        actualTime = 0.0f;

        experimentStarted = true;   //activate flag to start the experiment

        //set cure duration for current trial
        //currentCueDuration = Random.Range(cueDurationAvg - cueDurationVariation, cueDurationAvg + cueDurationVariation);
        currentCueDuration = cueDurations[trialSeqCounter];
        //Debug.Log("currentCueDuration: " + currentCueDuration.ToString());

        //set task for current trial
        if (tasks[trialTasks[trialSeqCounter]].Contains("point"))
            currentTask = "point";
        else if (tasks[trialTasks[trialSeqCounter]].Contains("touch"))
            currentTask = "touch";

        //set condition for current trial
        if (tasks[trialTasks[trialSeqCounter]].Contains("Far"))
            currentCondition = "far";
        else if (tasks[trialTasks[trialSeqCounter]].Contains("Near"))
            currentCondition = "near";

        //set stimulus object for current trial
        if (currentCondition == "near")
        {
            //For the near positions we have to add 5 to the index, cause the near positions are the indexes 5-9 in the cubeGameObjArray
            currentStimulusObj = cubeGameObjArr[CubePositions[cubeSeqCounter + 5]];
        }
        else if (currentCondition == "far")
        {
            currentStimulusObj = cubeGameObjArr[CubePositions[cubeSeqCounter]];
        }

        //write trial start marker
        tempMarkerText =
            "trialStart:" + trialSeqCounter.ToString() + ";" +
            "task:" + currentTask + ";" +
            "condition:" + currentCondition + ";" +
            "cueDuration:" + currentCueDuration.ToString() + ";" +
            "stimulusPosition:" + currentStimulusObj.gameObject.name;
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        targetActivated = false;
    }


    public void NextTrial()
    {
        //send trial end marker
        marker.Write("trialEnd:" + trialSeqCounter.ToString());
        Debug.Log("trialEnd:" + trialSeqCounter.ToString());

        trialSeqCounter = trialSeqCounter + 1;
        cubeSeqCounter = cubeSeqCounter + 1;

        // reshuffle stimulus sequence
        if (cubeSeqCounter == CubeSeq.Length-1)
        {
            cubeSeqCounter = 0;
            RandomizeArray.ShuffleArray(CubeSeq); // re-randomize stimulus sequence 
        }
        
        taskSuccess = false;

        collisionActive = false;
        visualFeedbackActive = false;
        collisionDuration.Reset();

        //break after 1/4, 1/2, 3/4 of total trials:
        if (trialSeqCounter == (int)(nrOfTrialsTotal / 4) || trialSeqCounter == (int)(nrOfTrialsTotal / 2) || trialSeqCounter == (int)(nrOfTrialsTotal * 3 / 4))
        {
            //start break not next trial
            if (trialSeqCounter == (int)(nrOfTrialsTotal / 4) || trialSeqCounter == (int)(nrOfTrialsTotal * 3 / 4))
                StartBreak(120f);  //2min break
            else
                StartBreak(180f);  //3min break
        }
        //check if all trials have been run
        else if (trialSeqCounter == nrOfTrialsTotal)
            //set flag for experiment end and don't start another trial
            experimentEnd = true;
        else
            TrialStart();
        
    }


    // Enable selected cube for rendering and collider properties
    public void CubeVisible(GameObject GO)
    {
        DeactivateAllCubes();
        
        GO.SetActive(true);
        marker.Write("activateStimulus:" + GO.name);
        Debug.Log("activateStimulus:" + GO.name);

        //added this here to activate the collider (cause may have been disabled after touch in earlier trial)
        GO.GetComponent<SphereCollider>().enabled = true;
    }


    //starts the "initial" feedback before minimum task duration is reached. The feedback is primarily an aiming aid.
    public void StartVisualFeedback(GameObject GO, string collisionEvent)
    {
        //check if collision is already active but the visual feedback (green/red) is not active
        if (!collisionActive && !visualFeedbackActive)
        {
            //save current collision object reference for later
            currentCollisionObj = GO;

            //start the hit duration
            collisionDuration.Start();

            //set hit flag
            collisionActive = true;

            //set response type
            currentResponseType = collisionEvent;

            //save current time as possible reaction time
            reaction_time_temp =  actualTime - reaction_start_time;

            if (collisionEvent == "point") {
                //activate early visual feedback
                GO.gameObject.GetComponent<Renderer>().material.color = customYellow;

                tempMarkerText =
                    "initialVisualFeedbackStarted:" + collisionEvent + ";" +
                    "color:yellow;" +
                    "object:" + GO.gameObject.name;
                marker.Write(tempMarkerText);
                Debug.Log(tempMarkerText + " " + actualTime.ToString());
            }
            //if (collisionEvent == "grab") {
            if (collisionEvent == "touch") {
                //activate early visual feedback
                GO.gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                tempMarkerText =
                    "initialVisualFeedbackStarted:" + collisionEvent + ";" +
                    " color:cyan;" +
                    "object:" + GO.gameObject.name;
                marker.Write(tempMarkerText);
                Debug.Log(tempMarkerText + " " + actualTime.ToString());
            }
        }

    }


    //stops the "initial" feedback when the object is not hit anymore (only if the minumim task duration was not reached)
    public void StopVisualFeedback(string collisionEvent)
    {
        //check if collision already active but the visual feedback (green/red) is not active
        if (collisionActive && !visualFeedbackActive)
        {
            //check if the same responseType
            if(currentResponseType == collisionEvent)
            {
                //reset the hit duration
                collisionDuration.Reset();

                //reset hit flag
                collisionActive = false;

                //clear response type
                currentResponseType = "none";

                //reset possible reaction time
                reaction_time_temp = 0;

                //reset color of the collision object
                currentCollisionObj.GetComponent<Renderer>().material.color = Color.white;

                tempMarkerText =
                    "initialVisualFeedbackStopped:" + collisionEvent + ";" +
                    " color:white;" +
                    "object:" + currentCollisionObj.gameObject.name;
                marker.Write(tempMarkerText);
                Debug.Log(tempMarkerText + " " + actualTime.ToString());

                //reset collision object
                currentCollisionObj = null;
            }
        }

    }


    // Feedback of Stimulus on touch
    public void VisualFeeback(GameObject GO, string collisionEvent)     //collisionEvent should be "touch"/"grab"/"point"
    {
        //check if correct or incorrect feedback
        if (currentTask == collisionEvent)
        {
            //correct task feedback:
            GO.gameObject.GetComponent<Renderer>().material.color = Color.green;

            tempMarkerText =
                "visualFeedbackStarted:" + collisionEvent + ";" +
                "color:green;" +
                "correctTask:true;" +
                "responseTime:" + reaction_time_temp.ToString();
            marker.Write(tempMarkerText);
            Debug.Log(tempMarkerText + " " + actualTime.ToString());
        }
        else
        {
            //wrong task feedback:
            GO.gameObject.GetComponent<Renderer>().material.color = Color.red;

            tempMarkerText =
                "visualFeedbackStarted:" + collisionEvent + ";" +
                "color:red;" +
                "correctTask:false;" +
                "responseTime:" + reaction_time_temp.ToString();
            marker.Write(tempMarkerText);
            Debug.Log(tempMarkerText + " " + actualTime.ToString());
        }

        reaction_time = reaction_time_temp;

        taskSuccess = true;
    }


    public Vector3 CalculatePosition(GameObject gameObject, Vector3 rootPosition, float distance, int angle)
    {
        //This method calculates the new position of an object if it is moved using a distance and an angle from a root position
        //and returns the new position as a Vector3.

        //Debug.Log("CalculatePosition: " + gameObject.name);
        //Debug.Log("RootPos: " + rootPosition.ToString() + " distance: " + distance.ToString() + " angle: " + angle.ToString());

        var q = Quaternion.AngleAxis(angle, Vector3.up);
        //Debug.Log("q: " + q.ToString());

        //Vector3 newPosition = new Vector3(currentPosition.x + newX, currentPosition.y, currentPosition.z + newZ);
        Vector3 newPosition = rootPosition + q * Vector3.forward * distance;

        //Debug.Log("newPosition: " + newPosition.ToString());

        return newPosition;
    }


    public void MoveCups(GameObject[] cupObjects, Vector3 rootPosition, Vector3 maxReachPosition , int[] angles, int offsetNear_percent, int offsetFar_percent)
    {
        //This method moves all cups into individual calculated positions.
        //Half of the cups will be in an near area and the other half in an far area. SO near and far have different distance values while moving.
        //The distances will be calcuated using the offset values.
        //The new position will be calculated using the root position, distance and angle.

        int currentAngle=0;
        float currentDistance=0;
        float currentArmLength = maxReachPosition.z - rootPosition.z;

        for (int i=0; i<cupObjects.Length; i++)
        {
            //determine angle
            if (cupObjects[i].name.Contains("MiddleLeft"))
            {
                currentAngle = angles[1];
            }
            else if (cupObjects[i].name.Contains("MiddleRight"))
            {
                currentAngle = angles[3];
            }
            else if (cupObjects[i].name.Contains("Middle"))
            {
                currentAngle = angles[2];
            }
            else if (cupObjects[i].name.Contains("Left"))
            {
                currentAngle = angles[0];
            }
            else if (cupObjects[i].name.Contains("Right"))
            {
                currentAngle = angles[4];
            }

            //check if near or far
            if (cupObjects[i].name.Contains("Far"))
            {
                //currentDistance = armlength_cm + ((float)armlength_cm/100)*offsetFar_percent;
                currentDistance = currentArmLength + (currentArmLength / 100) * offsetFar_percent;
            }
            else if (cupObjects[i].name.Contains("Near"))
            {
                //currentDistance = armlength_cm - ((float)armlength_cm/100)*offsetNear_percent;
                currentDistance = currentArmLength - (currentArmLength / 100) * offsetNear_percent;
            }
            //Debug.Log("currentDistance in m: " + currentDistance.ToString());

            //change the height (y-axis) value from rootPosition to the cupObjects height value (so that is corretly positioned at table heigth)
            Vector3 newRootPosition = new Vector3(rootPosition.x, cupObjects[i].transform.position.y, rootPosition.z);

            //move current object
            cupObjects[i].transform.position = CalculatePosition(cupObjects[i], newRootPosition, currentDistance, currentAngle);

            //save position for lsl marker
            stimulusPositions[i] = cupObjects[i].transform.position.ToString();
        }

    }


     static string BoolToString(bool b)
    {
        return b ? "true":"false";
    }

    
    public void StartMainMenu()
    {
        //This method is used for the "Configuration" button on the main menu. WHen the button is pressed this method is executed.
        Debug.Log("Starting Main Menu");
        expControlStatus = 0;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(true);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);
        //introGUI.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        introGUI.SetActive(false);
        //plane.gameObject.GetComponent<Renderer>().enabled = false;
        plane.SetActive(false);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        //table.gameObject.GetComponent<Renderer>().enabled = false;
        table.SetActive(false);
        DeactivateAllCubes();
        startContinue.SetActive(false);
        //resting.gameObject.GetComponent<Renderer>().enabled = false;
        resting.SetActive(false);
        //questionnaire.SetActive(false);
        //endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        //endexp.SetActive(false);      //commented out so that the text will stay after experiment ended and main menu is displayed for experimenter (but not for participant)
        //shoulder.SetActive(false);
        breakCanvasVR.SetActive(false);
        breakCanvasDesktop.SetActive(false);

    }

    public void StartConfiguration()
    {
        //This method is used for the "Configuration" button on the main menu. WHen the button is pressed this method is executed.
        Debug.Log("Starting Configuration");
        expControlStatus = 1;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(true);
        //introGUI.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        introGUI.SetActive(false);
        //plane.gameObject.GetComponent<Renderer>().enabled = false;
        plane.SetActive(false);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        //table.gameObject.GetComponent<Renderer>().enabled = false;
        table.SetActive(false);
        DeactivateAllCubes();
        startContinue.SetActive(false);
        //resting.gameObject.GetComponent<Renderer>().enabled = false;
        resting.SetActive(false);
        //questionnaire.SetActive(false);
        //endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        endexp.SetActive(false);
        //shoulder.SetActive(false);
        breakCanvasVR.SetActive(false);
        breakCanvasDesktop.SetActive(false);
    }


    public void ConfigurationExit()
    {
        //this is called when pressing the "Back" button in the configuration menu

        //save data from the inputs:

        //participantID
        if (inputParticipantID.GetComponent<InputField>().text != "")
        {
            idSet = true;
            participantID = inputParticipantID.GetComponent<InputField>().text;
        }
        else
            idSet = false;

        //participantAge
        if (inputParticipantAge.GetComponent<InputField>().text != "")
        {
            try
            {
                participantAge = int.Parse(inputParticipantAge.GetComponent<InputField>().text);
                ageSet = true;
            }
            catch (System.FormatException e)
            {
                Debug.LogException(e);
                participantAge = 0;
                inputParticipantAge.GetComponent<InputField>().text = "";
                ageSet = false;
            }
        }
        else
            ageSet = false;

        //participantGender
        if (!inputParticipantGender.GetComponent<Dropdown>().options[inputParticipantGender.GetComponent<Dropdown>().value].text.Equals("?"))
        {
            genderSet = true;
            participantGender = inputParticipantGender.GetComponent<Dropdown>().options[inputParticipantGender.GetComponent<Dropdown>().value].text;
        }
        else
            genderSet = false;

        //armLength
        if (inputArmLength.GetComponent<InputField>().text != "")
        {
            try
            {
                armLength = (int.Parse(inputArmLength.GetComponent<InputField>().text)/10); //mm -> cm
                Debug.Log("armLength: " + armLength.ToString());
                armLengthSet = true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                armLength = 0;
                inputArmLength.GetComponent<InputField>().text = "";
                armLengthSet = false;
            }
        }
        else
            armLengthSet = false;
        /*
        Debug.Log("participantID: " + participantID + " InputField.text: " + inputParticipantID.GetComponent<InputField>().text);
        Debug.Log("participantAge: " + participantAge.ToString() + " InputField.text: " + inputParticipantAge.GetComponent<InputField>().text);
        Debug.Log("participantGender: " + participantGender + " InputField.text: " + inputParticipantGender.GetComponent<Dropdown>().options[inputParticipantGender.GetComponent<Dropdown>().value].text);
        Debug.Log("armLength: " + armLength + " InputField.text: " + inputArmLength.GetComponent<InputField>().text);
        */

        //Go back to main menu
        StartMainMenu();
    }


    public void StartCalibration()
    {
        //This method is used for the "Configuration" button on the main menu. WHen the button is pressed this method is executed.
        Debug.Log("Starting Calibration");
        expControlStatus = 2;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(true);
        configurationMenu.SetActive(false);
        //introGUI.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        introGUI.SetActive(false);
        //plane.gameObject.GetComponent<Renderer>().enabled = false;
        plane.SetActive(true);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        //table.gameObject.GetComponent<Renderer>().enabled = false;
        table.SetActive(true);
        DeactivateAllCubes();
        startContinue.SetActive(false);
        //resting.gameObject.GetComponent<Renderer>().enabled = false;
        resting.SetActive(true);
        //questionnaire.SetActive(false);
        //endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        endexp.SetActive(false);
        //shoulder.SetActive(true);
        breakCanvasVR.SetActive(false);
        breakCanvasDesktop.SetActive(false);

        ActivateAllCubes();
    }

    public void StartTraining()
    {
        //This method is used for the "Start Training" button on the main menu. WHen the button is pressed this method is executed.
        marker.Write("Main menu: Start Training button pressed");
        Debug.Log("Starting Training");
        expControlStatus = 3;

    }
    
    public void StartExperiment()
    {
        //This method is used for the "Start Experiment" button on the main menu. WHen the button is pressed this method is executed.
        marker.Write("Main menu: Start Experiment button pressed");
        Debug.Log("Starting Experiment");

        expControlStatus = 4;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);
        //introGUI.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        introGUI.SetActive(true);
        //plane.gameObject.GetComponent<Renderer>().enabled = false;
        plane.SetActive(true);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        //table.gameObject.GetComponent<Renderer>().enabled = false;
        table.SetActive(true);
        DeactivateAllCubes();
        startContinue.SetActive(true);
        //resting.gameObject.GetComponent<Renderer>().enabled = false;
        resting.SetActive(true);
        //questionnaire.SetActive(false);
        //endexp.gameObject.gameObject.GetComponent<Canvas>().enabled = false;
        endexp.SetActive(false);
        //shoulder.SetActive(false);
        breakCanvasVR.SetActive(false);
        breakCanvasDesktop.SetActive(false);
    }


    public void StartBreak(float breakDuration)
    {
        //start break timer
        breakDurationCountdown = breakDuration;

        //set experiment control status to "break"
        expControlStatus = 5;

        experimentStarted = false;

        //activate break text
        breakCanvasVR.SetActive(true);
        breakCanvasDesktop.SetActive(true);

        tempMarkerText =
            "break:start;" +
            "afterTrial:" + (trialSeqCounter-1).ToString() + ";" +
            "breakDuration:" + breakDuration.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);
    }

    public void StopBreak()
    {
        //deactivate break text
        breakCanvasVR.SetActive(false);
        breakCanvasDesktop.SetActive(false);

        marker.Write("break:end;afterTrial:" + (trialSeqCounter-1).ToString());
        Debug.Log("break:end;afterTrial:" + (trialSeqCounter-1).ToString());

        //activate startContinue so that the participant can continue with experiment
        startContinue.SetActive(true);

        marker.Write("break over, waiting for manual continue");
        Debug.Log("Break time is over. Waiting for participant to continue...");

        expControlStatus = 4;
    }


    public void SetShoulderPosition()
    {
        //This method should measure the position of the participants right shoulder.
        //THe shoulder position is later used as the root position for the calutation of the cup positions.

        //Get Tracker position
        //GameObject tracker = GameObject.Find("Controller (left)");
        GameObject tracker = GameObject.Find("Controller (right)");

        try
        {
            shoulderPosition = tracker.transform.position;
            shoulderSet = true;
            trackerFoundShoulderPos = true;
            textHintShoulderPos.SetActive(false);
            //Debug.Log("shoulderPosition: " + shoulderPosition.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            //textHintConfigFirst.GetComponent<Text>().text = "Error: Tracker not found";
            trackerFoundShoulderPos = false;
            textHintShoulderPos.SetActive(true);
        }

    }


    public void SetMaximumReach()
    {
        //This method should measure the participant's forward maximum reach position. We later use this position to calculate all the cups position on the table.
        //We let the participant reach forward as much as they can while placing their hand on the table.
        //Then we put a Vive Tracker next to the fingertips (so that the middle of the Tracker is at the finger tips) and measure the Tracker position.

        //Get Tracker position
        //GameObject tracker = GameObject.Find("Controller (left)");
        GameObject tracker = GameObject.Find("Controller (right)");

        try
        {
            maxReachPosition = tracker.transform.position;
            maxReachSet = true;
            //Debug.Log("maxReachPosition: " + maxReachPosition.ToString());

            //calculate armlength based on the tracking positions of shoulder and maxReach (just for fun and curiosity):
            //pythagoras: a²+b²=c² 
            //a: height difference between shoulder and table
            //b: forward position difference between shoulder and maxReach position
            //c=sqrt(a²+b²)
            armLengthCalculated = Mathf.Sqrt( Mathf.Pow(shoulderPosition.y-maxReachPosition.y, 2) + Mathf.Pow(shoulderPosition.z-maxReachPosition.z, 2));
            //Debug.Log("armLengthCalculated: " + armLengthCalculated.ToString());

        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            textHintShoulderFirst.GetComponent<Text>().text = "Error: Tracker not found";
        }

    }


    public void SetCupPositions()
    {
        //calculate all cup positions according to the armlength and move them to the new positions:
        //MoveCups(cubeGameObjArr, resting.transform.position, 70, angles, offsetNearPercent, offsetFarPercent);
        //MoveCups(cubeGameObjArr, shoulder.transform.position, armLength, angles, offsetNearPercent, offsetFarPercent);
        MoveCups(cubeGameObjArr, shoulderPosition, maxReachPosition, angles, offsetNearPercent, offsetFarPercent);

        cupPositionsSet = true;
    }


    public void SetTablePosition()
    {
        //How this should be done: 
        // - During Calibration but a Vive Tracker on the table at the resting position
        // - Important: If the Tracker is visually rotated in VR, like 90° tilted to the side, this will not work!!!
        // - press the "Set Table Position" button
        //The position of the Tracker is taken and now we try to calculate the offset in height and position between the Tracker on the real table
        //and the virtual table Then the position of the real table and the virtual table are synchronized.

        //Get Tracker position
        //GameObject tracker = GameObject.Find("Controller (left)");
        GameObject tracker = GameObject.Find("Controller (right)");
        Vector3 trackerPosition = new Vector3();

        //Get virtual table size in world space
        Vector3 tablePosition = table.transform.position;
        Vector3 tableLocalPosition = table.transform.localPosition;
        Vector3 tableSize = table.GetComponent<Renderer>().bounds.size;

        float trackerHeightOffset = 0.02f;       //the tracker height is measured manually cause I don't know how to get the model size. The hole Tracker is about 4cm and the object handle is about half that height.

        Vector3 tableSetupPosition = tableSetup.transform.position;

        try
        {
            trackerPosition = tracker.transform.position;
            /*
            Debug.Log("TrackerPostion: " + trackerPosition.ToString());
            Debug.Log("TableSize: " + tableSize.ToString());
            Debug.Log("tablePosition: " + tablePosition.ToString());
            Debug.Log("tableLocalPosition: " + tableLocalPosition.ToString());
            */
            //calculate offset
            float offsetX = trackerPosition.x - resting.transform.position.x;       //resting position because the Tracker should be put at the resting position
            float offsetY = trackerPosition.y-trackerHeightOffset - tableSize.y - tablePosition.y;    //table height
            float offsetZ = trackerPosition.z - resting.transform.position.z;       //resting position because the Tracker should be put at the resting position

            //Debug.Log("offsetX: " + offsetX.ToString() + " offsetY: " + offsetY.ToString() + " offsetZ: " + offsetZ.ToString());

            //move virtual table (we move the parent object "tableSetup" instead of only the table, because we also need the other objects connected to the table to move the same)
            tableSetup.transform.position = new Vector3(tableSetupPosition.x + offsetX, tableSetupPosition.y + offsetY, tableSetupPosition.z + offsetZ);

            //Debug.Log("new tablePosition: " + tablePosition.ToString());
            //Debug.Log("new tableLocalPosition: " + tableLocalPosition.ToString());

            textHintTablePos.SetActive(false);
            tablePosSet = true;
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError(e);
            textHintTablePos.GetComponent<Text>().text = "Error: Tracker not found.";
            textHintTablePos.SetActive(true);
        }

    }


    // Update is called once per frame
    void Update ()
    {
        switch (expControlStatus) {

            case 0: //main menu
                {
                    //check if all inputs in Configuration have been given and alle calibration was made
                    if (idSet && ageSet && genderSet && armLengthSet && cupPositionsSet && tablePosSet)
                    {
                        buttonExperiment.GetComponent<Button>().interactable = true;
                        buttonTraining.GetComponent<Button>().interactable = true;
                        textMissingInputs.SetActive(false);
                    }
                    else
                    {
                        buttonExperiment.GetComponent<Button>().interactable = false;
                        buttonTraining.GetComponent<Button>().interactable = false;
                        textMissingInputs.SetActive(true);
                    }
                    break;
                }

            case 1: //configuration
                {
                    break;
                }
            case 2: //calibration
                {
                    //check if shoulder position has been set
                    if (shoulderSet)
                    {
                        buttonMaximumReach.GetComponent<Button>().interactable = true;

                        if(trackerFoundMaxReach)
                            textHintShoulderFirst.SetActive(false);
                        else
                            textHintShoulderFirst.SetActive(true);                        
                    }
                    else
                    {
                        buttonMaximumReach.GetComponent<Button>().interactable = false;
                        textHintShoulderFirst.SetActive(true);
                    }

                    //check if maximum reach has been set
                    if(maxReachSet)
                    {
                        buttonCupPositions.GetComponent<Button>().interactable = true;

                        if(trackerFoundCupPos)
                            textHintCupPos.SetActive(false);
                        else
                            textHintCupPos.SetActive(true);                        
                    }
                    else
                    {
                        buttonCupPositions.GetComponent<Button>().interactable = false;
                        textHintCupPos.SetActive(true);
                    }
                    break;
                }
            case 3: //training
                {
                    break;
                }
            case 4: //experiment
                {
                    //ControlState(); // run only once after hitting any key to start, in fact runs whenever key is hit

                    if (flagStart) // flagStart is true after hitting any key to start at the beginning or after block end
                    {
                        ControlTrial();
                    }
                    else
                    {
                        ControlState(); // run only once after hitting any key to start, in fact runs whenever key is hit
                    }
                    break;
                }
            case 5: //break
                {
                    breakDurationCountdown -= Time.deltaTime;

                    //check break timer
                    if (breakDurationCountdown <= 0)
                    {
                        //stop break and continue with experiment
                        StopBreak();
                    }

                    //continue experiment
                    break;
                }

        }

    }

}