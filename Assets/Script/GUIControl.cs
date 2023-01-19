using UnityEngine;
using System.Collections;
using Assets.LSL4Unity.Scripts; // reference the LSL4Unity namespace to get access to all classes
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
//using Uniduino; // adding arduino     //[BPA: removed vibro feedback]

public class GUIControl : MonoBehaviour {

    //Inspector Interface:
    [Header("General Config")]
    public float isiDurationAvg = 2;                //1.5s ISI duration on average
    public float isiDurationVariation = 1;          //0.5s variation (so the ISI duration range is betweeen 1s and 2s)
    public float fixationDuration = 1f;             //1s fixation cross is visible
    public float cueDurationAvg = 2f;               //2s cue average duration
    public float cueDurationVariation = 1f;         //1s variation (so the cue duration is 2s +- 1s
    public float responseTimeMax = 5.0f;        //5s max target is visible
    public float feedbackDuration = 1.0f;           //2s feedback duration
    public float minimumTaskDuration = 1.0f;        //1s minimum collision duration for a successful response
    public int[] stimulusAngles = new int[] {-30, -20, -10, 10, 20, 30};    //the angles at which the stimulus can be positioned from the shoulder
    public int offsetNearPercent = 40;              //offset from maximum reach position
    public int offsetFarPercent = 40;               //offset from maximum reach position
    public bool earlyFeedbackOn = true;
    public bool handMovementThresholdOn = false;
    public float handMovementThreshold = 0.00075f;    //this is a distance value. It's the distance the hand moved between two frames.
    public string mpsQuestionnairePath = "Assets/Questionnaires/Data/Questions/MPS_Questionaire.json";

    [Header("Experiment specific")]
    public int trialsPerTask = 60;
    public int standardBreakDuration = 60;          //2 minutes
    public int standardBreakEveryTrials = 100;
    public int halfTimeBreakDuration = 300;         //5 minutes
    public int manualBrakeDuration = 10;            //10 seconds
    public int manualBreakEveryTrials = 20;         //a manual break every 25 trials (except half time)

    /*
    [Header("Learning specific")]
    public int trialsPerTaskLearning = 6;
    public float cueDurationLearning = 1.5f;
    */

    [Header("Training specific")]
    public int trialsPerTaskTraining = 6;

    [Header("Baseline specific")]
    public int baselineDuration = 120;              //2 minutes
    //public int trialsPerTaskBaselineNew = 6;

    [Header("Misc")]
    public LSLMarkerStream marker;
    public GameObject rightHand;
    public GameObject EyeTracking;
    public GameObject ButtonEyeCalibration;
    public GameObject vrQuestionnaireToolkit;
    public GameObject vrQuestionnaireExport;
    public GameObject vivePointers;

    [Header("Debugging")]    
    public bool disableSRanipal = false;


    [HideInInspector] // Hides vars from Inspector
    // general
    public static string[] tasks = new string[4] { "touchFar", "touchNear", "pointFar", "pointNear" };
    private int[] taskSeq = new int[] { 0, 1, 2, 3 };
    private int[] trialTasks;
    [HideInInspector] // Hides vars from Inspector
    public string currentTask;
    private string currentCondition;
    private GameObject currentStimulusObj;
    private string stimulusPositions = "";
    private int[] cubeSeqExperiment = new int[] { 0, 1, 2, 3, 4, 5 };   //using the same array for near and far cause it's easier
    private int[] cubeSeqTraining = new int[] { 1, 4 };     //using the same array for near and far cause it's easier
    private int[] cubeSeq;
    private int[] cubePositions = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
    private int cubeSeqCounter = 0;
    public static int trialSeqCounter;
    public static string currentResponseType = "none";
    public static System.Diagnostics.Stopwatch collisionDuration = new System.Diagnostics.Stopwatch();
    public static float collisionStartTime = 0;
    private static GameObject currentCollisionObj = null;
    private static Color32 brightRed = new Color32(255, 160, 160, 255);
    private static Color32 brightGreen = new Color32(160, 255, 160, 255);
    private Vector3 shoulderPosition;
    private Vector3 maxReachPosition;
    private string tempMarkerText = "";
    private int currentInstructionPage;

    private float actualTime = 0;       // start timer at 0
    private float reaction_time = 0;
    private float reaction_time_temp = 0;
    private float reaction_start_time = 0;

    private string participantNo;
    private int participantAge;
    private string participantGender;
    private float armLength;
    private float armLengthCalculated;
    private float currentStimulusDistanceFromShoulder = 0f;
    private float currentStimulusDistanceFromShoulder2D = 0f;
    private int currentStimulusAngle = 0;

    //questionnaire
    private VRQuestionnaireToolkit.GenerateQuestionnaire vrQuestionnaire;
    private string currentQuestionnaireName = "";


    // experiment logic handler
    private bool idSet = false;
    private bool ageSet = false;
    private bool genderSet = false;
    private bool armLengthSet = false;
    private bool shoulderSet = false;
    private bool maxReachSet = false;
    private bool cupPositionsSet = false;
    private bool tablePosSet = false;
    private bool trackerFoundShoulderPos = true;
    private bool trackerFoundMaxReach = true;
    private bool trackerFoundCupPos = true;
    private static bool visualFeedbackActive = false;
    [HideInInspector] // Hides vars from Inspector
    public int expControlStatus;
    private float breakDurationCountdown;       //break timer
    private bool questionnaireStarted = false;
    private bool questionnaireFinished = false;

    // trial logic handler
    private bool experimentStarted = false;
    private bool isiStarted = false;
    private bool fixationCrossActivated = false;
    private bool cueActivated = false;
    private bool targetActivated = false;
    private bool taskSuccess = false;
    [HideInInspector] // Hides vars from Inspector
    public bool collisionActive = false;
    [HideInInspector] // Hides vars from Inspector
    public bool restingDetectionActive = false;
    public static bool flagStart = false;
    public static bool experimentEnd = false;
    [HideInInspector] // Hides vars from Inspector
    public bool activateRaycast = false;

    [HideInInspector] // Hides vars from Inspector
    public bool handIsMoving = true;
    private Vector3 handPosition_old = Vector3.zero;

    /* learning specific
    [HideInInspector] // Hides vars from Inspector
    public bool learningStarted = false;
    private int learningRunNo = 0;
    private string endTextLearning = "The learning block has ended.\nPlease contact the experimenter.";
    */

    // training specific
    [HideInInspector] // Hides vars from Inspector
    public bool trainingStarted = false;
    private int trainingRunNo = 0;
    private string endTextTraining = "The training block has ended.\nPlease contact the experimenter.";

    // experiment specific
    private int nrOfTrialsTotal;
    private float[] isiDurations;
    private float currentIsiDuration;         //stores individual ISI duration of the current trial
    private float[] cueDurations;
    private float currentCueDuration;           //stores individual cue duration of the current trial
    private int experimentRunNo = 0;
    private string endTextExp = "The experiment has ended.\nThank you for your participation.";
    private bool manualBreakTriggered = false;
    private string startNextTrialText = "Put your hand on the resting position to start the trial";

    //baseline specific
    [HideInInspector] // Hides vars from Inspector
    public bool baselineRunning = false;
    private string endTextBaseline = "The baseline has ended.\nPlease contact the experimenter.";
    private int baselineClosedRunNo = 0;
    private int baselineOpenRunNo = 0;
    //private int baselineNewRunNo = 0;
    private string startBaselineText = "Put your hand on the resting position to start the baseline.";


    // Game objects
    public static GameObject table, plane, instructionsExperiment, /*instructionsLearning,*/ instructionsTraining, instructionsBaselineClosed, instructionsBaselineOpen, /*instructionsBaselineNew,*/ textBox, end, endTextBox, startTrialCanvas, resting, tableCalibration, questionnaire, q, ra, la, send, fixationCross, cue, cueText,
        mainMenu, calibrationMenu, configurationMenu, inputParticipantNo, inputParticipantAge, inputParticipantGender, inputArmLength, buttonExperiment, startTrialText,
        /*buttonLearning,*/ buttonTraining, buttonShoulderPos, textHintShoulderPos, textMissingInputs, tableSetup, buttonMaximumReach, buttonCubePositions, buttonTablePosition, textHintShoulderFirst,
        textHintCupPos, textHintTablePos, breakCanvasDesktop, vr_hand_R, continueCanvas, continueButton, baselineClosedCanvas, buttonBaselineClosed, buttonBaselineOpen, startFirstTrial, torso, //buttonBaselineNew,
        buttonNextPage, /*textLearningPage1, textLearningPage2, textLearningPage3, textLearningPage4, textLearningPage5, textLearningPage6, textLearningNextPage,*/ textTrainingPage1, textTrainingPage2, textTrainingPage3, textTrainingPage4, textTrainingPage5, textTrainingPage6, textTrainingNextPage, textExperimentPage1, textExperimentPage2, textExperimentPage3, textExperimentPage4, textExperimentNextPage;

    private GameObject cubeFarLeft2, cubeFarLeft1, cubeFarLeft0, cubeFarRight0, cubeFarRight1, cubeFarRight2, cubeNearLeft2, cubeNearLeft1, cubeNearLeft0, cubeNearRight0, cubeNearRight1, cubeNearRight2;
    private GameObject[] cubeGameObjArr = new GameObject[12];
    //private GameObject[] textLearningPages = new GameObject[6];
    private GameObject[] textTrainingPages = new GameObject[6];
    private GameObject[] textExperimentPages = new GameObject[4];


    void Awake()
    {
        if (disableSRanipal)
        {
            EyeTracking.SetActive(false);
            ButtonEyeCalibration.SetActive(false);
        }
    }

    // Use this for initialization
    void Start()
    {
        // init LSL markerstream
        marker = FindObjectOfType<LSLMarkerStream>();
        
        // Finding the game object 
        table = GameObject.Find("Table");
        plane = GameObject.Find("Plane");
        buttonNextPage = GameObject.Find("ButtonNextPage");
        instructionsExperiment = GameObject.Find("InstructionsExperiment");
        textExperimentPage1 = GameObject.Find("TextExperimentPage1");
        textExperimentPage2 = GameObject.Find("TextExperimentPage2");
        textExperimentPage3 = GameObject.Find("TextExperimentPage3");
        textExperimentPage4 = GameObject.Find("TextExperimentPage4");
        textExperimentNextPage = GameObject.Find("TextExperimentNextPage");
        /*instructionsLearning = GameObject.Find("InstructionsLearning");
        textLearningPage1 = GameObject.Find("TextLearningPage1");
        textLearningPage2 = GameObject.Find("TextLearningPage2");
        textLearningPage3 = GameObject.Find("TextLearningPage3");
        textLearningPage4 = GameObject.Find("TextLearningPage4");
        textLearningPage5 = GameObject.Find("TextLearningPage5");
        textLearningPage6 = GameObject.Find("TextLearningPage6");
        textLearningNextPage = GameObject.Find("TextLearningNextPage");*/
        instructionsTraining = GameObject.Find("InstructionsTraining");
        textTrainingPage1 = GameObject.Find("TextTrainingPage1");
        textTrainingPage2 = GameObject.Find("TextTrainingPage2");
        textTrainingPage3 = GameObject.Find("TextTrainingPage3");
        textTrainingPage4 = GameObject.Find("TextTrainingPage4");
        textTrainingPage5 = GameObject.Find("TextTrainingPage5");
        textTrainingPage6 = GameObject.Find("TextTrainingPage6");
        textTrainingNextPage = GameObject.Find("TextTrainingNextPage");

        instructionsBaselineClosed = GameObject.Find("InstructionsBaselineClosed");
        instructionsBaselineOpen = GameObject.Find("InstructionsBaselineOpen");
        //instructionsBaselineNew = GameObject.Find("InstructionsBaselineNew");
        textBox = GameObject.Find("TextBox");
        end = GameObject.Find("End");
        endTextBox = GameObject.Find("EndTextBox");
        startTrialCanvas = GameObject.Find("startTrialCanvas");
        startTrialText = GameObject.Find("startTrialText");
        continueCanvas = GameObject.Find("continueCanvas");
        continueButton = GameObject.Find("continue");
        resting = GameObject.Find("resting");
        tableCalibration = GameObject.Find("TableCalibration");
        fixationCross = GameObject.Find("FixationCross");
        cue = GameObject.Find("Cue");
        cueText = GameObject.Find("CueText");
        mainMenu = GameObject.Find("MainMenu");
        calibrationMenu = GameObject.Find("CalibrationMenu");
        configurationMenu = GameObject.Find("ConfigurationMenu");
        inputParticipantNo = GameObject.Find("InputParticipantNo");
        inputParticipantAge = GameObject.Find("InputParticipantAge");
        inputParticipantGender = GameObject.Find("DropdownGender");
        inputArmLength = GameObject.Find("InputParticipantArmLength");
        buttonExperiment = GameObject.Find("ButtonExperiment");
        //buttonLearning = GameObject.Find("ButtonLearning");
        buttonTraining = GameObject.Find("ButtonTraining");
        buttonShoulderPos = GameObject.Find("ButtonShoulderPos");
        textHintShoulderPos = GameObject.Find("TextHintShoulderPos");
        textMissingInputs = GameObject.Find("TextMissingInputs");
        tableSetup = GameObject.Find("TableSetup");
        buttonMaximumReach = GameObject.Find("ButtonMaximumReach");
        buttonCubePositions = GameObject.Find("ButtonSetCubePositions");
        buttonTablePosition = GameObject.Find("ButtonTablePos");
        textHintShoulderFirst = GameObject.Find("TextHintShoulderFirst");
        textHintCupPos = GameObject.Find("TextHintCupPos");
        textHintTablePos = GameObject.Find("TextHintTablePos");
        breakCanvasDesktop = GameObject.Find("BreakCanvasDesktop");
        buttonBaselineClosed = GameObject.Find("ButtonBaselineClosed");
        buttonBaselineOpen = GameObject.Find("ButtonBaselineOpen");
        baselineClosedCanvas = GameObject.Find("BaselineClosedCanvas");
        startFirstTrial = GameObject.Find("startFirstTrial");
        torso = GameObject.Find("TorsoObject");
        //buttonBaselineNew = GameObject.Find("ButtonBaselineNew");

        //Stimulus
        cubeFarLeft2 = GameObject.Find("CubeFarLeft2");
        cubeFarLeft1 = GameObject.Find("CubeFarLeft1");
        cubeFarLeft0 = GameObject.Find("CubeFarLeft0");
        cubeFarRight0 = GameObject.Find("CubeFarRight0");
        cubeFarRight1 = GameObject.Find("CubeFarRight1");
        cubeFarRight2 = GameObject.Find("CubeFarRight2");
        cubeNearLeft2 = GameObject.Find("CubeNearLeft2");
        cubeNearLeft1 = GameObject.Find("CubeNearLeft1");
        cubeNearLeft0 = GameObject.Find("CubeNearLeft0");
        cubeNearRight0 = GameObject.Find("CubeNearRight0");
        cubeNearRight1 = GameObject.Find("CubeNearRight1");
        cubeNearRight2 = GameObject.Find("CubeNearRight2");

        // Assign game objects to arrays
        cubeGameObjArr[0] = cubeFarLeft2;
        cubeGameObjArr[1] = cubeFarLeft1;
        cubeGameObjArr[2] = cubeFarLeft0;
        cubeGameObjArr[3] = cubeFarRight0;
        cubeGameObjArr[4] = cubeFarRight1;
        cubeGameObjArr[5] = cubeFarRight2;
        cubeGameObjArr[6] = cubeNearLeft2;
        cubeGameObjArr[7] = cubeNearLeft1;
        cubeGameObjArr[8] = cubeNearRight0;
        cubeGameObjArr[9] = cubeNearLeft0;
        cubeGameObjArr[10] = cubeNearRight1;
        cubeGameObjArr[11] = cubeNearRight2;
        /*
        textLearningPages[0] = textLearningPage1;
        textLearningPages[1] = textLearningPage2;
        textLearningPages[2] = textLearningPage3;
        textLearningPages[3] = textLearningPage4;
        textLearningPages[4] = textLearningPage5;
        textLearningPages[5] = textLearningPage6;
        */
        textTrainingPages[0] = textTrainingPage1;
        textTrainingPages[1] = textTrainingPage2;
        textTrainingPages[2] = textTrainingPage3;
        textTrainingPages[3] = textTrainingPage4;
        textTrainingPages[4] = textTrainingPage5;
        textTrainingPages[5] = textTrainingPage6; 

        textExperimentPages[0] = textExperimentPage1;
        textExperimentPages[1] = textExperimentPage2;
        textExperimentPages[2] = textExperimentPage3;
        textExperimentPages[3] = textExperimentPage4;


        //deactivate the "Start Experiment" and "Training" Buttons:
        buttonExperiment.GetComponent<Button>().interactable = false;
        //buttonLearning.GetComponent<Button>().interactable = false;

        textHintTablePos.SetActive(false);
        textHintShoulderPos.SetActive(false);
        end.SetActive(false);
        baselineClosedCanvas.SetActive(false);
        startFirstTrial.SetActive(false);
        buttonNextPage.SetActive(false);

        torso.GetComponent<Renderer>().enabled = false;   //make torso invisible

        //##### Initialize VRQuestionnaire
        vrQuestionnaire = vrQuestionnaireToolkit.GetComponentInChildren<VRQuestionnaireToolkit.GenerateQuestionnaire>();
        //StopQuestionnaire(0);       //stop first questionnaire (as it is automaticalle started)


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
            obj.GetComponent<SphereCollider>().enabled = false;
        }

    }

    public void ActivateAllCubes()
    {
        foreach (GameObject obj in cubeGameObjArr)
        {
            obj.SetActive(true);
            obj.GetComponent<SphereCollider>().enabled = true;
        }

    }


    public int[] CreateTrialTaskArray(int totalTrials, int[] taskSequence, string[] taskNames)
    {
        //Fill an array with tasks. For example: 4 different tasks and 100 trials per task - the array should contain 100 values of each task.
        //We use a method minimizing task repetition in subsequent trials.
        int tempTrialCounter = 0;
        int tempTaskCounter = 0;
        int[] tempTrialTasks = new int[totalTrials];
        RandomizeArray.ShuffleArray(taskSequence);

        while (tempTrialCounter < totalTrials)
        {
            if (tempTaskCounter >= taskSequence.Length)
            {
                tempTaskCounter = 0;
                RandomizeArray.ShuffleArray(taskSequence);
            }

            tempTrialTasks[tempTrialCounter] = taskSequence[tempTaskCounter];

            tempTaskCounter++;
            tempTrialCounter++;

        }

        /*
        //Debug: print out the array:
        Debug.Log("Task sequence:");
        for (int i=0; i< tempTrialTasks.Length; i++)
        {
            Debug.Log(taskNames[tempTrialTasks[i]]);
        }*/

        return tempTrialTasks;
    }


    public float[] CreateDurationsArray(int arraySize, float durationAverage, float durationVariation)
    {
        //Create an array the size of arraySize with duration values which range from durationAverage-durationVariation to durationAverage+durationVariation.
        //The individual durations will be distributed evenly within this range and the order will be shuffled at the end.

        float[] tempDurations = new float[arraySize];

        
        //Debug.Log("All durations:");
        for (int i = 0; i < arraySize; i++)
        {
            //the goal here is to get linear distributed values in the range
            tempDurations[i] = i * (durationVariation * 2 / (arraySize - 1)) + durationAverage - durationVariation;
            //Debug.Log(tempDurations[i].ToString());
        }
        //shuffle cue duration order
        RandomizeArray.ShuffleArray(tempDurations);

        return tempDurations;
    }

    
    public void ControlTrial()
    {
        //Check if flag has been activated through TrialStart() via touching the startButton in VR
        if (experimentStarted)
        {
            // add the time taken to render last frame, experiment logic is based on this parameter
            // actualTime is constantly growing
            actualTime += Time.deltaTime;

            // run all trials
            if (trialSeqCounter < nrOfTrialsTotal && !experimentEnd) 
            {
                if (expControlStatus == 9)
                {
                    RunBaselineTrial();
                }
                else
                {
                    RunTrial();
                }
                
            }

            // after all trials are finished
            if (experimentEnd)  
            {
                //write specific end marker
                /*if (learningStarted)
                {
                    marker.Write("learning:end");
                    Debug.Log("learning:end");
                }
                else*/ if (trainingStarted)
                {
                    marker.Write("training:end");
                    Debug.Log("training:end");
                }
                /*else if (baselineRunning)
                {
                    marker.Write("baselineNew:end");
                    Debug.Log("baselineNew:end");
                }*/
                else
                {
                    marker.Write("experiment:end");
                    Debug.Log("experiment:end");
                }

                //activate experiment end text
                end.SetActive(true);

                experimentStarted = false;

                //go to main menu
                StartMainMenu();
            }

        }//if experimentStarted
   
    }//ControlTrial()


    public void RunTrial()
    {
        //Start of trial: ISI
        if (actualTime <= currentIsiDuration)
        {
            if(!isiStarted)
            {
                isiStarted = true;
                marker.Write("ISI started");
                Debug.Log("ISI started: " + actualTime.ToString());
            }
        }

        //After ISI: fixation cross
        //if (actualTime <= fixationDuration)
        if (actualTime > currentIsiDuration && actualTime <= fixationDuration + currentIsiDuration)
        {
            if (!fixationCrossActivated)
            {
                //ISI ended
                isiStarted = false;
                marker.Write("ISI ended");
                //Debug.Log("ISI ended: " + actualTime.ToString());

                //enable fixation cross
                fixationCross.SetActive(true);
                marker.Write("Fixation cross activated");
                Debug.Log("Fixation Cross activated: " + actualTime.ToString());
                fixationCrossActivated = true;
            }
        }

        //after fixation cross: show cue
        //if (actualTime > fixationDuration && actualTime <= fixationDuration + currentCueDuration)
        if (actualTime > fixationDuration + currentIsiDuration && actualTime <= fixationDuration + currentIsiDuration + currentCueDuration)
        {
            if (!cueActivated)
            {
                //deactivate fixation cross
                fixationCross.SetActive(false);
                marker.Write("Fixation cross deactivated");
                //Debug.Log("Fixation Cross deactivated: " + actualTime.ToString());
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
        //if (actualTime > fixationDuration + currentCueDuration)
        if (actualTime > fixationDuration + currentIsiDuration + currentCueDuration)
        {
            if (!taskSuccess)    //if the current task has not been successful yet
            {
                //show stimulus
                if (!targetActivated)
                {
                    //deactivate cue
                    cue.SetActive(false);

                    marker.Write("cue text deactivated");
                    //Debug.Log("Cue deactivated: " + actualTime.ToString());
                    cueActivated = false;


                    //activate stimulus
                    CubeVisible(currentStimulusObj);
                    Debug.Log("Stimulus activated: " + currentStimulusObj.name + " " + actualTime.ToString());

                    //set reaction start time
                    reaction_start_time = actualTime;

                    targetActivated = true;
                }


                //activate raycast for pointing detection
                if (handMovementThresholdOn)
                {
                    if (handIsMoving)
                    {
                        if (activateRaycast)
                        {
                            //if raycast was activated before -> write marker
                            marker.Write("Hand is moving -> deactivating pointing detection");
                            //Debug.Log("Hand is moving -> deactivating pointing detection " + actualTime.ToString());
                        }
                        activateRaycast = false;
                    }
                    else
                    {
                        if (!activateRaycast)
                        {
                            //if raycast was deactivated before -> write marker
                            marker.Write("Hand stopped moving -> activating pointing detection");
                            //Debug.Log("Hand stopped moving -> activating pointing detection " + actualTime.ToString());
                        }
                        activateRaycast = true;
                    }
                }
                else
                {
                    activateRaycast = true;
                }


                //check for successful response (if the collision has reached the minimum duration)
                if (collisionActive)
                {
                    //Debug.Log("colision active: " + (actualTime - collisionStartTime).ToString() + " " + actualTime.ToString());

                    //if ((float)collisionDuration.ElapsedMilliseconds / 10000 >= minimumTaskDuration)
                    if (actualTime - collisionStartTime >= minimumTaskDuration)
                    {
                        //activate correct/incorrect visual feedback
                        visualFeedbackActive = true;
                        VisualFeeback(currentCollisionObj, currentResponseType);
                    }
                }
                //if time for a response has run out -> go to next trial (but NOT if there is an active collision)
                //else if (actualTime > fixationDuration + currentCueDuration + responseTimeMax)
                else if (actualTime > fixationDuration + currentIsiDuration + currentCueDuration + responseTimeMax)
                {
                    //deactivate raycast
                    activateRaycast = false;

                    marker.Write("response time over");
                    Debug.Log("response time over. " + actualTime.ToString());

                    DeactivateAllCubes();
                    NextTrial();
                }

            }
            //if the current task has been successful
            else
            {
                //deactivate raycast
                activateRaycast = false;

                //wait for visual feedback to finish and then -> go to next trial
                //if (actualTime > fixationDuration + currentCueDuration + reaction_time + minimumTaskDuration + feedbackDuration)
                if (actualTime > fixationDuration + currentIsiDuration + currentCueDuration + reaction_time + minimumTaskDuration + feedbackDuration)
                {
                    //deactivate stimulus
                    DeactivateAllCubes();

                    marker.Write("visual feeback duration over");
                    Debug.Log("visual feeback duration over " + actualTime.ToString());

                    //marker.Write("visualFeedback:off");
                    //Debug.Log("visualFeedback:off " + actualTime.ToString());

                    //transition to next trial
                    NextTrial();
                }
            }

        }

    }


    public void RunBaselineTrial()
    {
        //Start of trial: ISI
        if (actualTime <= currentIsiDuration)
        {
            if (!isiStarted)
            {
                isiStarted = true;
                marker.Write("ISI started");
                Debug.Log("ISI started: " + actualTime.ToString());
            }
        }

        //After ISI: fixation cross
        //if (actualTime <= fixationDuration)
        if (actualTime > currentIsiDuration && actualTime <= fixationDuration + currentIsiDuration)
        {
            if (!fixationCrossActivated)
            {
                //ISI ended
                isiStarted = false;
                marker.Write("ISI ended");
                //Debug.Log("ISI ended: " + actualTime.ToString());

                //enable fixation cross
                fixationCross.SetActive(true);
                marker.Write("Fixation cross activated");
                Debug.Log("Fixation Cross activated: " + actualTime.ToString());
                fixationCrossActivated = true;
            }
        }

        //after fixation cross: show cue
        //if (actualTime > fixationDuration && actualTime <= fixationDuration + currentCueDuration)
        if (actualTime > fixationDuration + currentIsiDuration && actualTime <= fixationDuration + currentIsiDuration + currentCueDuration)
        {
            if (!cueActivated)
            {
                //deactivate fixation cross
                fixationCross.SetActive(false);
                marker.Write("Fixation cross deactivated");
                //Debug.Log("Fixation Cross deactivated: " + actualTime.ToString());
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
        //if (actualTime > fixationDuration + currentCueDuration)
        if (actualTime > fixationDuration + currentIsiDuration + currentCueDuration)
        {
            if (!taskSuccess)    //if the current task has not been successful yet
            {
                //show stimulus
                if (!targetActivated)
                {
                    //deactivate cue
                    cue.SetActive(false);

                    marker.Write("cue text deactivated");
                    //Debug.Log("Cue deactivated: " + actualTime.ToString());
                    cueActivated = false;


                    //activate stimulus
                    CubeVisible(currentStimulusObj);
                    Debug.Log("Stimulus activated: " + currentStimulusObj.name + " " + actualTime.ToString());

                    //set reaction start time
                    reaction_start_time = actualTime;

                    targetActivated = true;
                }


                //activate raycast for pointing detection
                if (handMovementThresholdOn)
                {
                    if (handIsMoving)
                    {
                        if (activateRaycast)
                        {
                            //if raycast was activated before -> write marker
                            marker.Write("Hand is moving -> deactivating pointing detection");
                            //Debug.Log("Hand is moving -> deactivating pointing detection " + actualTime.ToString());
                        }
                        activateRaycast = false;
                    }
                    else
                    {
                        if (!activateRaycast)
                        {
                            //if raycast was deactivated before -> write marker
                            marker.Write("Hand stopped moving -> activating pointing detection");
                            //Debug.Log("Hand stopped moving -> activating pointing detection " + actualTime.ToString());
                        }
                        activateRaycast = true;
                    }
                }
                else
                {
                    activateRaycast = true;
                }


                //check for successful response (if the collision has reached the minimum duration)
                if (collisionActive)
                {
                    //Debug.Log("colision active: " + (actualTime - collisionStartTime).ToString() + " " + actualTime.ToString());

                    //if ((float)collisionDuration.ElapsedMilliseconds / 10000 >= minimumTaskDuration)
                    if (actualTime - collisionStartTime >= minimumTaskDuration)
                    {
                        //activate correct/incorrect visual feedback
                        visualFeedbackActive = true;
                        VisualFeeback(currentCollisionObj, currentResponseType);
                    }
                }
                //if time for a response has run out -> go to next trial (but NOT if there is an active collision)
                //else if (actualTime > fixationDuration + currentCueDuration + responseTimeMax)
                else if (actualTime > fixationDuration + currentIsiDuration + currentCueDuration + responseTimeMax)
                {
                    //deactivate raycast
                    activateRaycast = false;

                    marker.Write("response time over");
                    Debug.Log("response time over. " + actualTime.ToString());

                    DeactivateAllCubes();
                    NextTrial();
                }

            }
            //if the current task has been successful
            else
            {
                //deactivate raycast
                activateRaycast = false;

                //wait for visual feedback to finish and then -> go to next trial
                //if (actualTime > fixationDuration + currentCueDuration + reaction_time + minimumTaskDuration + feedbackDuration)
                if (actualTime > fixationDuration + currentIsiDuration + currentCueDuration + reaction_time + minimumTaskDuration + feedbackDuration)
                {
                    //deactivate stimulus
                    DeactivateAllCubes();

                    marker.Write("visual feeback duration over");
                    Debug.Log("visual feeback duration over " + actualTime.ToString());

                    //marker.Write("visualFeedback:off");
                    //Debug.Log("visualFeedback:off " + actualTime.ToString());

                    //transition to next trial
                    NextTrial();
                }
            }

        }

    }


    public void TrialStart()
    {
        //deactivate start/Continue button
        startTrialCanvas.SetActive(false);
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        restingDetectionActive = false;
        //tableTextBackground.SetActive(true);

        //deactivate instructions
        if (instructionsExperiment.activeSelf || /*instructionsLearning.activeSelf ||*/ instructionsTraining.activeSelf)
        {
            instructionsExperiment.SetActive(false);
            //instructionsLearning.SetActive(false);
            instructionsTraining.SetActive(false);
            instructionsBaselineClosed.SetActive(false);
            instructionsBaselineOpen.SetActive(false);
            //instructionsBaselineNew.SetActive(false);
            marker.Write("instructions deactivated");
            Debug.Log("instructions deactivated");
        }

        //if this is a continue after a break -> call stopBreak()
        if (expControlStatus == 6)
        {
            StopBreak();
        }

        // reset trial time
        actualTime = 0.0f;

        experimentStarted = true;   //activate flag to start the experiment


        //set ISI duration for current trial
        currentIsiDuration = isiDurations[trialSeqCounter];
        //Debug.Log("currentIsiDuration: " + currentIsiDuration.ToString());

        //set cue duration for current trial
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
            //For the near positions we have to add 6 to the index, cause the near positions are the indexes 6-11 in the cubeGameObjArray
            currentStimulusObj = cubeGameObjArr[cubePositions[cubeSeq[cubeSeqCounter] + 6]];
        }
        else if (currentCondition == "far")
        {
            currentStimulusObj = cubeGameObjArr[cubePositions[cubeSeq[cubeSeqCounter]]];
        }

        //calculate stimulus distance from shoulder
        currentStimulusDistanceFromShoulder = Vector3.Distance(currentStimulusObj.transform.position, shoulderPosition);
        
        //calculate stimulus distance from shoulder (ignoring the height difference)
        currentStimulusDistanceFromShoulder2D = Vector3.Distance(currentStimulusObj.transform.position, new Vector3(shoulderPosition.x, currentStimulusObj.transform.position.y, shoulderPosition.z));


        //set current cube angle
        currentStimulusAngle = stimulusAngles[cubeSeq[cubeSeqCounter]];

        //write trial start marker
        tempMarkerText =
            "trialStart:" + trialSeqCounter.ToString() + ";" +
            "task:" + currentTask + ";" +
            "condition:" + currentCondition + ";" +
            "isiDuration:" + currentIsiDuration.ToString() + ";" +
            "cueDuration:" + currentCueDuration.ToString() + ";" +
            "stimulusObject:" + currentStimulusObj.gameObject.name + ";" +
            "stimulusPosition:" + currentStimulusObj.transform.position.ToString() + ";" +
            "stimulusAngle:" + currentStimulusAngle.ToString() + ";" +
            "stimulusDistanceFromShoulder:" + currentStimulusDistanceFromShoulder.ToString() + ";" +
            "stimulusDistanceFromShoulder2D:" + currentStimulusDistanceFromShoulder2D.ToString();
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
        if (cubeSeqCounter == cubeSeq.Length)
        {
            cubeSeqCounter = 0;
            RandomizeArray.ShuffleArray(cubeSeq); // re-randomize stimulus sequence 
        }
        
        taskSuccess = false;

        collisionActive = false;
        visualFeedbackActive = false;
        collisionStartTime = 0;


        //check if all trials have been run
        if (trialSeqCounter == nrOfTrialsTotal)
        {
            //set flag for experiment end and don't start another trial
            experimentEnd = true;
        }
        else
        {
            //ONLY during experiment and NOT during learning or training: BREAK TIME after 1/4, 1/2, 3/4 of total trials:
            if (!(trainingStarted /*|| learningStarted*/))
            {
                //check if start BREAK TIME or next trial
                if (trialSeqCounter == (int)(nrOfTrialsTotal / 2))   //half time break
                {
                    StartBreak(halfTimeBreakDuration, false);
                }
                else if (trialSeqCounter % standardBreakEveryTrials == 0)
                {
                    StartBreak(standardBreakDuration, false);
                }
                else if (trialSeqCounter % manualBreakEveryTrials == 0)     //manual break every X trials
                {
                    StartBreak(manualBrakeDuration, false);
                }
                else if (manualBreakTriggered) //manual break triggered by pressing the "p" key
                {
                    StartBreak(manualBrakeDuration, true);
                }
                else
                {
                    //TrialStart();

                    //wait for manual trial start by putting hand on resting position
                    experimentStarted = false;
                    startTrialCanvas.SetActive(true);
                    resting.SetActive(true);
                    restingDetectionActive = true;

                    marker.Write("Waiting for hand on resting position");
                    Debug.Log("Waiting for hand on resting position...");
                }
            }
            else
            {
                //wait for manual trial start by putting hand on resting position
                experimentStarted = false;
                resting.SetActive(true);
                restingDetectionActive = true;

                marker.Write("Waiting for hand on resting position");
                Debug.Log("Waiting for hand on resting position...");

            }
        }
        
    }


    // Enable selected cube for rendering and collider properties
    public void CubeVisible(GameObject GO)
    {
        DeactivateAllCubes();
        
        GO.SetActive(true);
        marker.Write("activateStimulus:" + GO.name);
        //Debug.Log("activateStimulus:" + GO.name);

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
            collisionStartTime = actualTime;

            //set hit flag
            collisionActive = true;

            //set response type
            currentResponseType = collisionEvent;

            //save current time as possible reaction time
            reaction_time_temp =  actualTime - reaction_start_time;


            if(earlyFeedbackOn)
            {
                //activate early visual feedback
                if (collisionEvent == currentTask)
                {
                    //correct early feedback
                    GO.gameObject.GetComponent<Renderer>().material.color = brightGreen;

                    tempMarkerText =
                        "initialVisualFeedbackStarted:" + collisionEvent + ";" +
                        "color:brightGreen;" +
                        "object:" + GO.gameObject.name;
                    marker.Write(tempMarkerText);
                    Debug.Log(tempMarkerText + " " + actualTime.ToString());
                }
                else
                {
                    //wrong early feedback
                    GO.gameObject.GetComponent<Renderer>().material.color = brightRed;

                    tempMarkerText =
                        "initialVisualFeedbackStarted:" + collisionEvent + ";" +
                        " color:brightRed;" +
                        "object:" + GO.gameObject.name;
                    marker.Write(tempMarkerText);
                    Debug.Log(tempMarkerText + " " + actualTime.ToString());
                }
            }
            else
            {
                tempMarkerText =
                    "Collision started but early feedback is disabled:" + collisionEvent + ";" +
                    " color:None;" +
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
                collisionStartTime = 0;

                //reset hit flag
                collisionActive = false;

                //clear response type
                currentResponseType = "none";

                //reset possible reaction time
                reaction_time_temp = 0;

                //reset color of the collision object
                currentCollisionObj.GetComponent<Renderer>().material.color = Color.white;

                if (earlyFeedbackOn)
                {
                    tempMarkerText =
                        "initialVisualFeedbackStopped:" + collisionEvent + ";" +
                        "color:white;" +
                        "object:" + currentCollisionObj.gameObject.name;
                    marker.Write(tempMarkerText);
                    Debug.Log(tempMarkerText + " " + actualTime.ToString());
                }
                else
                {
                    tempMarkerText =
                        "Collision stopped but early feedback is disabled:" + collisionEvent + ";" +
                        "color:None;" +
                        "object:" + currentCollisionObj.gameObject.name;
                    marker.Write(tempMarkerText);
                    Debug.Log(tempMarkerText + " " + actualTime.ToString());
                }

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
        string[] tempArray = new string[cupObjects.Length];

        for (int i=0; i<cupObjects.Length; i++)
        {
            //determine angle
            if (cupObjects[i].name.Contains("Left2"))
            {
                currentAngle = angles[0];
            }
            else if (cupObjects[i].name.Contains("Left1"))
            {
                currentAngle = angles[1];
            }
            else if (cupObjects[i].name.Contains("Left0"))
            {
                currentAngle = angles[2];
            }
            else if (cupObjects[i].name.Contains("Right0"))
            {
                currentAngle = angles[3];
            }
            else if (cupObjects[i].name.Contains("Right1"))
            {
                currentAngle = angles[4];
            }
            else if (cupObjects[i].name.Contains("Right2"))
            {
                currentAngle = angles[5];
            }

            //check if near or far
            if (cupObjects[i].name.Contains("Far"))
            {
                currentDistance = currentArmLength + (currentArmLength / 100) * offsetFar_percent;
            }
            else if (cupObjects[i].name.Contains("Near"))
            {
                currentDistance = currentArmLength - (currentArmLength / 100) * offsetNear_percent;
            }
            //Debug.Log("currentDistance in m: " + currentDistance.ToString());

            //change the height (y-axis) value from rootPosition to the cupObjects height value (so that is corretly positioned at table heigth)
            //also we take the x position from the resting object so that the middle of the cubes is aligned with the resting position and independantly from the shoulder position
            Vector3 newRootPosition = new Vector3(resting.transform.position.x, cupObjects[i].transform.position.y, rootPosition.z);

            //move current object
            cupObjects[i].transform.position = CalculatePosition(cupObjects[i], newRootPosition, currentDistance, currentAngle);

            //save position for lsl marker
            tempArray[i] = cupObjects[i].transform.position.ToString();
        }

        stimulusPositions = string.Join(" ", tempArray);

        //Debug:
        //Debug.Log("stimulusPositions array: " + stimulusPositions);

    }


    static string BoolToString(bool b)
    {
        return b ? "true":"false";
    }

    
    public void StartMainMenu()
    {
        //This method is used for starting the main menu.
        Debug.Log("Starting Main Menu");
        expControlStatus = 0;

        //activate and deactivate objects:
        plane.gameObject.SetActive(true);
        table.gameObject.SetActive(true);
        mainMenu.gameObject.SetActive(true);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);
        instructionsExperiment.SetActive(false);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        tableCalibration.SetActive(false);
        breakCanvasDesktop.SetActive(false);
        startFirstTrial.SetActive(false);
        /*
        for (int i = 0; i < textLearningPages.Length; i++)
        {
            textLearningPages[i].SetActive(false);
        }
        textLearningNextPage.SetActive(false);*/

        for (int i = 0; i < textTrainingPages.Length; i++)
        {
            textTrainingPages[i].SetActive(false);
        }
        textTrainingNextPage.SetActive(false);

        for (int i = 0; i < textExperimentPages.Length; i++)
        {
            textExperimentPages[i].SetActive(false);
        }
        textExperimentNextPage.SetActive(false);

        //reset control flags:
        flagStart = false;
        //learningStarted = false;
        trainingStarted = false;

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
        instructionsExperiment.SetActive(false);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);
    }


    public void ConfigurationExit()
    {
        //this is called when pressing the "Back" button in the configuration menu

        //save data from the inputs:
        //participantNo
        if (inputParticipantNo.GetComponent<InputField>().text != "")
        {
            idSet = true;
            participantNo = inputParticipantNo.GetComponent<InputField>().text;
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
                marker.Write("FormatException: invalid input value for participant age. " + e.ToString());
                Debug.Log("FormatException: invalid input value for participant age.");
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
                armLength = (float.Parse(inputArmLength.GetComponent<InputField>().text))/10;     //mm -> cm
                //Debug.Log("armLength: " + armLength.ToString());
                armLengthSet = true;
            }
            catch (System.FormatException e)
            {
                marker.Write("FormatException: invalid input value for prticipant arm length. " + e.ToString());
                Debug.Log("FormatException: invalid input value for participant arm length.");
                Debug.LogException(e);
                armLength = 0;
                inputArmLength.GetComponent<InputField>().text = "";
                armLengthSet = false;
            }
        }
        else
            armLengthSet = false;
        /*
        Debug.Log("participantNo: " + participantNo + " InputField.text: " + inputParticipantNo.GetComponent<InputField>().text);
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
        instructionsExperiment.SetActive(false);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(true);
        tableCalibration.SetActive(true); 
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);

        ActivateAllCubes();
    }


    public void StartBaselineClosed()
    {
        //This method is used for the "Start Baseline Closed" button on the main menu. When the button is pressed this method is executed.
        marker.Write("Main menu: Start Baseline Closed button pressed");
        Debug.Log("Starting Baseline Closed");

        expControlStatus = 7;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);

        instructionsBaselineClosed.SetActive(true);
        marker.Write("instructions activated");
        Debug.Log("Instructions activated");

        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startBaselineText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);
        startFirstTrial.SetActive(true);

        restingDetectionActive = false;

        marker.Write("Waiting for touch on startFirstTrialButton");
        Debug.Log("Waiting for touch on startFirstTrialButton...");

    }

    public void InitBaselineClosed()
    {
        baselineClosedRunNo += 1;

        //deactivate all visuals
        instructionsExperiment.SetActive(false);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        startTrialCanvas.SetActive(false);
        resting.SetActive(false);

        //activate special canvas to black out the hmd
        baselineClosedCanvas.SetActive(true);
        marker.Write("Activated Black canvas");
        Debug.Log("Activated Black canvas");

        restingDetectionActive = false;

        //start timer
        actualTime = 0;

        //activate flag
        baselineRunning = true;

        //write experiment start marker
        tempMarkerText =
            "baselineClosed:start;" +
            "runNo:" + baselineClosedRunNo.ToString() + ";" +
            "duration:" + baselineDuration.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

    }


    public void StartBaselineOpen()
    {
        //This method is used for the "Start Baseline Open" button on the main menu. When the button is pressed this method is executed.
        marker.Write("Main menu: Start Baseline Open button pressed");
        Debug.Log("Starting Baseline Open");

        expControlStatus = 8;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);

        instructionsBaselineOpen.SetActive(true);
        marker.Write("instructions activated");
        Debug.Log("Instructions activated");

        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startBaselineText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);
        startFirstTrial.SetActive(true);

        restingDetectionActive = false;

        marker.Write("Waiting for touch on startFirstTrialButton");
        Debug.Log("Waiting for touch on startFirstTrialButton...");

    }

    public void InitBaselineOpen()
    {
        baselineOpenRunNo += 1;

        //deactivate all visuals
        instructionsExperiment.SetActive(false);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        startTrialCanvas.SetActive(false);
        resting.SetActive(false);
        restingDetectionActive = false;

        //start timer
        actualTime = 0;

        //activate flag
        baselineRunning = true;

        //write experiment start marker
        tempMarkerText =
            "baselineOpen:start;" +
            "runNo:" + baselineOpenRunNo.ToString() + ";" +
            "duration:" + baselineDuration.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

    }

    /*
    public void StartBaselineNew()
    {
        //This method is used for the "Start Baseline New" button on the main menu. When the button is pressed this method is executed.
        marker.Write("Main menu: Start Baseline New button pressed");
        Debug.Log("Starting Baseline New");

        expControlStatus = 9;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);

        instructionsExperiment.SetActive(false);
        instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        instructionsBaselineNew.SetActive(true);

        marker.Write("instructions activated");
        Debug.Log("Instructions activated");

        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);
        startFirstTrial.SetActive(true);

    }*/

    /*
    public void InitBaselineNew()
    {
        //set experiment control vars
        baselineRunning = true;
        experimentEnd = false;
        trialSeqCounter = 0;
        baselineNewRunNo += 1;

        //calculate total number of trials
        nrOfTrialsTotal = trialsPerTaskBaselineNew * tasks.Length;

        //create array with tasks for all trials
        trialTasks = CreateTrialTaskArray(nrOfTrialsTotal, taskSeq, tasks);

        //create array with isi durations for all trials
        isiDurations = CreateDurationsArray(nrOfTrialsTotal, isiDurationAvg, isiDurationVariation);

        //create array with cue durations for all tasks
        cueDurations = CreateDurationsArray(nrOfTrialsTotal, cueDurationAvg, cueDurationVariation);

        // Randomize Cube Appearance Sequence
        RandomizeArray.ShuffleArray(cubeSeq);

        // activate/deactivate objects in scene
        table.gameObject.GetComponent<Renderer>().enabled = true;
        plane.gameObject.GetComponent<Renderer>().enabled = true;
        end.SetActive(false);
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        resting.SetActive(false);
        restingDetectionActive = false;
        startFirstTrial.SetActive(true);

        //set correct end text
        endTextBox.GetComponent<Text>().text = endTextTraining;

        //write experiment start marker
        tempMarkerText =
            "baselineNew:start;" +
            "runNo:" + trainingRunNo.ToString() + ";" +
            "trialsPerTask:" + trialsPerTaskTraining.ToString() + ";" +
            "trialsTotal:" + nrOfTrialsTotal.ToString() + ";" +
            "isiDurationAvg:" + isiDurationAvg.ToString() + ";" +
            "isiDurationVariation:" + isiDurationVariation.ToString() + ";" +
            "fixationDuration:" + fixationDuration.ToString() + ";" +
            "cueDurationAvg:" + cueDurationAvg.ToString() + ";" +
            "cueDurationVariation:" + cueDurationVariation.ToString() + ";" +
            "stimulusDurationMax:" + responseTimeMax.ToString() + ";" +
            "feedbackDuration:" + feedbackDuration.ToString() + ";" +
            "minTaskDuration:" + minimumTaskDuration.ToString() + ";" +
            "offsetNearPercent:" + offsetNearPercent.ToString() + ";" +
            "offsetFarPercent:" + offsetFarPercent.ToString() + ";" +
            "handMovementThreshold:" + handMovementThreshold.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        //write participant info (from configuration menu)
        tempMarkerText =
            "participantNo:" + participantNo + ";" +
            "participantAge:" + participantAge.ToString() + ";" +
            "participantGender:" + participantGender + ";" +
            "participantArmLength:" + armLength;
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

        marker.Write("Waiting for touch on startFirstTrialButton");
        Debug.Log("Waiting for touch on startFirstTrialButton...");

    }*/

    /*
    public void StartLearning()
    {
        //This method is used for the "Start Learning" button on the main menu. When the button is pressed this method is executed.
        marker.Write("Main menu: Start Learning button pressed");
        Debug.Log("Starting Learning");

        expControlStatus = 3;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);
        
        instructionsExperiment.SetActive(false);
        instructionsLearning.SetActive(true);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        instructionsBaselineNew.SetActive(false);
        buttonNextPage.SetActive(true);
        marker.Write("instructions activated");
        Debug.Log("Instructions activated");

        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);
        startFirstTrial.SetActive(false);

    }*/

    /*
    public void InitLearning()
    {
        //set experiment control vars
        learningStarted = true;
        experimentEnd = false;
        trialSeqCounter = 0;
        learningRunNo += 1;
        currentInstructionPage = 0;

        //calculate total number of trials
        nrOfTrialsTotal = trialsPerTaskLearning * tasks.Length;

        // Create array with tasks for all trials
        //For learning we do not mix tasks between all trials. We do a sequence of trials of each task.
        //The order of tasks will be randomed.
        int[] tempSequence = taskSeq;
        RandomizeArray.ShuffleArray(tempSequence);
        trialTasks = new int[nrOfTrialsTotal];
        int tempCounter = 0;

        for(int i=0; i<tempSequence.Length; i++)    //for every task
        {
            for(int j=0; j<trialsPerTaskLearning;j++)
            {
                trialTasks[tempCounter] = tempSequence[i];
                //Debug.Log(tasks[tempSequence[i]]);

                tempCounter++;
            }
        }

        /
        //Debug: print out the array:
        Debug.Log("trialTasks:");
        for (int i=0; i< trialTasks.Length; i++)
        {
            Debug.Log(tasks[trialTasks[i]]);
        } /
        
        //create array with isi durations for all trials
        isiDurations = CreateDurationsArray(nrOfTrialsTotal, isiDurationAvg, isiDurationVariation);

        //create array with cue durations for all tasks
        cueDurations = new float[nrOfTrialsTotal];

        for (int i=0; i<nrOfTrialsTotal; i++)
        {
            cueDurations[i] = cueDurationLearning;
        }


        // Randomize Cube Appearance Sequence
        RandomizeArray.ShuffleArray(cubeSeq);

        // activate/deactivate objects in scene
        table.gameObject.GetComponent<Renderer>().enabled = true;
        plane.gameObject.GetComponent<Renderer>().enabled = true;
        end.SetActive(false);
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        resting.SetActive(false);
        restingDetectionActive = false;
        startFirstTrial.SetActive(false);
        textLearningPages[currentInstructionPage].SetActive(true);
        textLearningNextPage.SetActive(true);

        //set correct end text
        endTextBox.GetComponent<Text>().text = endTextLearning;

        //write experiment start marker
        tempMarkerText =
            "learning:start;" +
            "runNo:" + learningRunNo.ToString() + ";" +
            "trialsPerTask:" + trialsPerTaskLearning.ToString() + ";" +
            "trialsTotal:" + nrOfTrialsTotal.ToString() + ";" +
            "isiDurationAvg:" + isiDurationAvg.ToString() + ";" +
            "isiDurationVariation:" + isiDurationVariation.ToString() + ";" +
            "fixationDuration:" + fixationDuration.ToString() + ";" +
            "cueDuration:" + cueDurationLearning.ToString() + ";" +
            "stimulusDurationMax:" + responseTimeMax.ToString() + ";" +
            "feedbackDuration:" + feedbackDuration.ToString() + ";" +
            "minTaskDuration:" + minimumTaskDuration.ToString() + ";" +
            "offsetNearPercent:" + offsetNearPercent.ToString() + ";" +
            "offsetFarPercent:" + offsetFarPercent.ToString() + ";" +
            "handMovementThreshold:" + handMovementThreshold.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        //write participant info (from configuration menu)
        tempMarkerText =
            "participantNo:" + participantNo + ";" +
            "participantAge:" + participantAge.ToString() + ";" +
            "participantGender:" + participantGender + ";" +
            "participantArmLength:" + armLength;
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

        //marker.Write("Waiting for touch on startFirstTrialButton");
        //Debug.Log("Waiting for touch on startFirstTrialButton...");

    }*/


    public void StartTraining()
    {
        //This method is used for the "Start Training" button on the main menu. When the button is pressed this method is executed.
        marker.Write("Main menu: Start Training button pressed");
        Debug.Log("Starting Training");

        expControlStatus = 4;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);

        instructionsExperiment.SetActive(false);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(true);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        buttonNextPage.SetActive(true);
        marker.Write("instructions activated");
        Debug.Log("Instructions activated");

        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);

    }


    public void InitTraining()
    {
        //set experiment control vars
        trainingStarted = true;
        experimentEnd = false;
        trialSeqCounter = 0;
        trainingRunNo += 1;
        currentInstructionPage = 0;
        cubeSeq = cubeSeqTraining;

        //calculate total number of trials
        nrOfTrialsTotal = trialsPerTaskTraining * tasks.Length;

        /*
        //create array with tasks for all trials
        trialTasks = CreateTrialTaskArray(nrOfTrialsTotal, taskSeq, tasks);
        */
        //Create array with tasks for all trials
        //For training we do not mix tasks between all trials. We do a sequence of trials from each task.
        //The order of tasks will be randomed.
        int[] tempSequence = taskSeq;
        RandomizeArray.ShuffleArray(tempSequence);
        trialTasks = new int[nrOfTrialsTotal];
        int tempCounter = 0;

        for (int i = 0; i < tempSequence.Length; i++)    //for every task
        {
            for (int j = 0; j < trialsPerTaskTraining; j++)
            {
                trialTasks[tempCounter] = tempSequence[i];
                //Debug.Log(tasks[tempSequence[i]]);

                tempCounter++;
            }
        }
        /*Debug: print out the array:
        Debug.Log("trialTasks:");
        for (int i = 0; i < trialTasks.Length; i++)
        {
            Debug.Log(tasks[trialTasks[i]]);
        }*/

        //create array with isi durations for all trials
        isiDurations = CreateDurationsArray(nrOfTrialsTotal, isiDurationAvg, isiDurationVariation);

        //create array with cue durations for all tasks
        cueDurations = CreateDurationsArray(nrOfTrialsTotal, cueDurationAvg, cueDurationVariation);

        // Randomize Cube Appearance Sequence
        RandomizeArray.ShuffleArray(cubeSeq);

        // activate/deactivate objects in scene
        table.gameObject.GetComponent<Renderer>().enabled = true;
        plane.gameObject.GetComponent<Renderer>().enabled = true;
        end.SetActive(false);
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        resting.SetActive(false);
        restingDetectionActive = false;
        startFirstTrial.SetActive(false);
        textTrainingPages[currentInstructionPage].SetActive(true);
        textTrainingNextPage.SetActive(true);

        //set correct end text
        endTextBox.GetComponent<Text>().text = endTextTraining;

        //write experiment start marker
        tempMarkerText =
            "training:start;" +
            "runNo:" + trainingRunNo.ToString() + ";" +
            "trialsPerTask:" + trialsPerTaskTraining.ToString() + ";" +
            "trialsTotal:" + nrOfTrialsTotal.ToString() + ";" +
            "isiDurationAvg:" + isiDurationAvg.ToString() + ";" +
            "isiDurationVariation:" + isiDurationVariation.ToString() + ";" +
            "fixationDuration:" + fixationDuration.ToString() + ";" +
            "cueDurationAvg:" + cueDurationAvg.ToString() + ";" +
            "cueDurationVariation:" + cueDurationVariation.ToString() + ";" +
            "stimulusDurationMax:" + responseTimeMax.ToString() + ";" +
            "feedbackDuration:" + feedbackDuration.ToString() + ";" +
            "minTaskDuration:" + minimumTaskDuration.ToString() + ";" +
            "offsetNearPercent:" + offsetNearPercent.ToString() + ";" +
            "offsetFarPercent:" + offsetFarPercent.ToString() + ";" +
            "handMovementThreshold:" + handMovementThreshold.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);
        
        //write participant info (from configuration menu)
        tempMarkerText =
            "participantNo:" + participantNo + ";" +
            "participantAge:" + participantAge.ToString() + ";" +
            "participantGender:" + participantGender + ";" +
            "participantArmLength:" + armLength;
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

        //marker.Write("Waiting for touch on startFirstTrialButton");
        //Debug.Log("Waiting for touch on startFirstTrialButton...");

    }


    public void StartExperiment()
    {
        //This method is used for the "Start Experiment" button on the main menu. WHen the button is pressed this method is executed.
        marker.Write("Main menu: Start Experiment button pressed");
        Debug.Log("Starting Experiment");

        expControlStatus = 5;

        //activate and deactivate objects:
        mainMenu.gameObject.SetActive(false);
        calibrationMenu.SetActive(false);
        configurationMenu.SetActive(false);

        instructionsExperiment.SetActive(true);
        //instructionsLearning.SetActive(false);
        instructionsTraining.SetActive(false);
        instructionsBaselineClosed.SetActive(false);
        instructionsBaselineOpen.SetActive(false);
        //instructionsBaselineNew.SetActive(false);
        marker.Write("instructions activated");
        Debug.Log("Instructions activated");
        buttonNextPage.SetActive(true);

        fixationCross.SetActive(false);
        cue.SetActive(false);
        DeactivateAllCubes();
        startTrialCanvas.SetActive(false);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        resting.SetActive(false);
        end.SetActive(false);
        breakCanvasDesktop.SetActive(false);
        startFirstTrial.SetActive(false);

    }


    public void InitExperiment()
    {
        //set experiment control vars
        flagStart = true;
        experimentEnd = false;
        experimentRunNo += 1;
        trialSeqCounter = 0;
        currentInstructionPage = 0;
        cubeSeq = cubeSeqExperiment;

        //calculate total number of trials
        nrOfTrialsTotal = trialsPerTask * tasks.Length;

        //create array with tasks for all trials
        trialTasks = CreateTrialTaskArray(nrOfTrialsTotal, taskSeq, tasks);

        //create array with isi durations for all trials
        isiDurations = CreateDurationsArray(nrOfTrialsTotal, isiDurationAvg, isiDurationVariation);

        //create array with cue durations for all trials
        cueDurations = CreateDurationsArray(nrOfTrialsTotal, cueDurationAvg, cueDurationVariation);

        // Randomize Cube Appearance Sequence
        RandomizeArray.ShuffleArray(cubeSeq);

        //activate/deactivate objects
        table.gameObject.GetComponent<Renderer>().enabled = true;
        plane.gameObject.GetComponent<Renderer>().enabled = true;
        end.SetActive(false);
        //startTrialCanvas.SetActive(true);
        startTrialText.GetComponent<Text>().text = startNextTrialText;
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);
        //tableTextBackground.SetActive(true);
        restingDetectionActive = false;
        resting.SetActive(false);
        startTrialCanvas.SetActive(false);
        startFirstTrial.SetActive(false);
        textExperimentPages[currentInstructionPage].SetActive(true);
        textExperimentNextPage.SetActive(true);


        //set correct end text
        endTextBox.GetComponent<Text>().text = endTextExp;

        //write experiment start marker
        tempMarkerText =
            "experiment:start;" +
            "runNo:" + experimentRunNo.ToString() + ";" +
            "trialsPerTask:" + trialsPerTask.ToString() + ";" +
            "trialsTotal:" + nrOfTrialsTotal.ToString() + ";" +
            "isiDurationAvg:" + isiDurationAvg.ToString() + ";" +
            "isiDurationVariation:" + isiDurationVariation.ToString() + ";" +
            "fixationDuration:" + fixationDuration.ToString() + ";" +
            "cueDurationAvg:" + cueDurationAvg.ToString() + ";" +
            "cueDurationVariation:" + cueDurationVariation.ToString() + ";" +
            "stimulusDurationMax:" + responseTimeMax.ToString() + ";" +
            "feedbackDuration:" + feedbackDuration.ToString() + ";" +
            "minTaskDuration:" + minimumTaskDuration.ToString() + ";" +
            "offsetNearPercent:" + offsetNearPercent.ToString() + ";" +
            "offsetFarPercent:" + offsetFarPercent.ToString() + ";" +
            "handMovementThreshold:" + handMovementThreshold.ToString();
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        //write participant info (from configuration menu)
        tempMarkerText =
            "participantNo:" + participantNo + ";" +
            "participantAge:" + participantAge.ToString() + ";" +
            "participantGender:" + participantGender + ";" +
            "participantArmLength:" + armLength;
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        //write calibration info (from calibration menu)
        tempMarkerText =
            "posTable:" + table.transform.position.ToString() + ";" +
            "posShoulder:" + shoulderPosition.ToString() + ";" +
            "posMaxReach:" + maxReachPosition.ToString() + ";" +
            "armLengthCalculated:" + armLengthCalculated.ToString() + ";" +
            "stimulusPositions:" + stimulusPositions;
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        //marker.Write("Waiting for touch on startFirstTrialButton");
        //Debug.Log("Waiting for touch on startFirstTrialButton...");

    }


    //public void StartBreak(float breakDuration)
    public void StartBreak(float breakDuration, bool isManual)
    {
        if (isManual)
        {
            manualBreakTriggered = false;
        }

        //set break timer
        breakDurationCountdown = breakDuration;

        //set experiment control status to "break"
        expControlStatus = 6;

        experimentStarted = false;

        //set break text
        breakCanvasDesktop.GetComponentInChildren<Text>().text = "BREAK TIME: " + breakDuration + "seconds left";

        //activate break text
        breakCanvasDesktop.SetActive(true);
        
        tempMarkerText =
            "break:start;" +
            "afterTrial:" + (trialSeqCounter-1).ToString() + ";" +
            "breakDuration:" + breakDurationCountdown.ToString() + ";" +
            "isManual:" + BoolToString(isManual);
        marker.Write(tempMarkerText);
        Debug.Log(tempMarkerText);

        //marker.Write("Waiting for hand on resting position");
        //Debug.Log("Waiting for hand on resting position...");
        
    }

    public void StopBreak()
    {
        //deactivate break text
        breakCanvasDesktop.SetActive(false);

        marker.Write("break:end;afterTrial:" + (trialSeqCounter-1).ToString());
        Debug.Log("break:end;afterTrial:" + (trialSeqCounter-1).ToString());

        //activate continue button so that the participant can continue with experiment
        continueCanvas.SetActive(true);
        continueButton.SetActive(true);

        marker.Write("Break is over. Waiting for continue button press.");
        Debug.Log("Break is over. Waiting for continue button press.");

        expControlStatus = 5;
    }


    public void ContinueAfterBreak()
    {
        //deactivate continueButton and text
        continueCanvas.SetActive(false);
        continueButton.SetActive(false);

        //wait for manual trial start by putting hand on resting position
        startTrialCanvas.SetActive(true);
        resting.SetActive(true);
        restingDetectionActive = true;

        marker.Write("Waiting for hand on resting position");
        Debug.Log("Waiting for hand on resting position...");
    }


    public void StartFirstTrial()
    {
        //deactivate first trial Button
        startFirstTrial.SetActive(false);

        //wait for manual trial start by putting hand on resting position
        startTrialCanvas.SetActive(true);
        resting.SetActive(true);
        restingDetectionActive = true;

        marker.Write("Waiting for hand on resting position");
        Debug.Log("Waiting for hand on resting position...");
    }


    public void SetShoulderPosition()
    {
        //This method should measure the position of the participants right shoulder.
        //THe shoulder position is later used as the root position for the calutation of the cup positions.

        //Get Tracker position
        //GameObject tracker = GameObject.Find("Controller (right)");


        try
        {
            //shoulderPosition = tracker.transform.position;
            shoulderPosition = rightHand.transform.position;
            shoulderSet = true;
            trackerFoundShoulderPos = true;
            textHintShoulderPos.SetActive(false);
            //Debug.Log("shoulderPosition: " + shoulderPosition.ToString());
        }
        catch (System.Exception e)
        {
            marker.Write("Exception: Tracker not found? " + e.ToString());
            Debug.Log("Exception: Tracker not found?");
            Debug.LogError(e);
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
        //GameObject tracker = GameObject.Find("Controller (right)");

        try
        {
            //maxReachPosition = tracker.transform.position;
            maxReachPosition = rightHand.transform.position;
            maxReachSet = true;
            //Debug.Log("maxReachPosition: " + maxReachPosition.ToString());

            //calculate armlength based on the tracking positions of shoulder and maxReach (just for fun and curiosity):
            //pythagoras: a²+b²=c² 
            //a: height difference between shoulder and table
            //b: forward position difference between shoulder and maxReach position
            //c=sqrt(a²+b²)
            armLengthCalculated = (Mathf.Sqrt( Mathf.Pow(shoulderPosition.y-maxReachPosition.y, 2) + Mathf.Pow(shoulderPosition.z-maxReachPosition.z, 2))) * 100;   //m -> cm
            //Debug.Log("armLengthCalculated: " + armLengthCalculated.ToString());

        }
        catch (System.Exception e)
        {
            marker.Write("Exception: Tracker not found? " + e.ToString());
            Debug.Log("Exception: Tracker not found?");
            Debug.LogError(e);
            textHintShoulderFirst.GetComponent<Text>().text = "Error: Tracker not found";
        }

    }


    public void SetCubePositions()
    {
        //calculate all cup positions according to the armlength and move them to the new positions:
        //MoveCups(cubeGameObjArr, resting.transform.position, 70, angles, offsetNearPercent, offsetFarPercent);
        //MoveCups(cubeGameObjArr, shoulder.transform.position, armLength, angles, offsetNearPercent, offsetFarPercent);
        MoveCups(cubeGameObjArr, shoulderPosition, maxReachPosition, stimulusAngles, offsetNearPercent, offsetFarPercent);

        cupPositionsSet = true;
    }


    public void SetTablePosition()
    {
        //How this should be done: 
        // - During Calibration put the Vive Tracker for the virtual hand on the table at the calibration position (yellow round spot in the middle of the table front)
        // - Important: If the Tracker is visually rotated in VR, like 90° tilted to the side, this will not work!!!
        // - press the "Set Table Position" button
        //The position of the Tracker is taken and now we try to calculate the offset in height and position between the Tracker on the real table
        //and the virtual table. Then the position of the real table and the virtual table are synchronized.

        //Get Tracker position
        //GameObject tracker = GameObject.Find("Controller (right)");
        Vector3 trackerPosition = new Vector3();

        //Get virtual table size in world space
        Vector3 tablePosition = table.transform.position;
        Vector3 tableLocalPosition = table.transform.localPosition;
        Vector3 tableSize = table.GetComponent<Renderer>().bounds.size;

        float trackerHeightOffset = 0.012f;       //the tracker height is measured manually cause I don't know how to get the model size. The hole Tracker is about 4cm and the object handle is about half that height.

        Vector3 tableSetupPosition = tableSetup.transform.position;

        try
        {
            //trackerPosition = tracker.transform.position;
            trackerPosition = rightHand.transform.position;
            /*
            Debug.Log("TrackerPostion: " + trackerPosition.ToString());
            Debug.Log("TableSize: " + tableSize.ToString());
            Debug.Log("tablePosition: " + tablePosition.ToString());
            Debug.Log("tableLocalPosition: " + tableLocalPosition.ToString());
            */
            //calculate offset
            float offsetX = trackerPosition.x - tableCalibration.transform.position.x;       //the Tracker should be put at the calibration position
            float offsetY = trackerPosition.y-trackerHeightOffset - tableSize.y - tablePosition.y;    //table height
            float offsetZ = trackerPosition.z - tableCalibration.transform.position.z;       //the Tracker should be put at the calibration position

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
            marker.Write("Exception: Tracker not found? " + e.ToString());
            Debug.Log("Exception: Tracker not found?");
            Debug.LogError(e);
            textHintTablePos.GetComponent<Text>().text = "Error: Tracker not found.";
            textHintTablePos.SetActive(true);
        }

    }


    public void NextInstructionPage()
    {
        /*
        if (expControlStatus == 3)  //learning
        {
            textLearningPages[currentInstructionPage].SetActive(false);
            currentInstructionPage += 1;
            textLearningPages[currentInstructionPage].SetActive(true);

            //when last page is activated
            if (currentInstructionPage == textLearningPages.Length-1) 
            {
                buttonNextPage.SetActive(false);
                startFirstTrial.SetActive(true);
                textLearningNextPage.SetActive(false);
            }
        }
        else*/ if (expControlStatus == 4) //training
        {
            textTrainingPages[currentInstructionPage].SetActive(false);
            currentInstructionPage += 1;
            textTrainingPages[currentInstructionPage].SetActive(true);

            //when last page is activated
            if (currentInstructionPage == textTrainingPages.Length-1) 
            {
                buttonNextPage.SetActive(false);
                startFirstTrial.SetActive(true);
                textTrainingNextPage.SetActive(false);
            }
        }
        else if (expControlStatus == 5) //experiment
        {
            textExperimentPages[currentInstructionPage].SetActive(false);
            currentInstructionPage += 1;
            textExperimentPages[currentInstructionPage].SetActive(true);

            //when last page is activated
            if (currentInstructionPage == textExperimentPages.Length-1) 
            {
                buttonNextPage.SetActive(false);
                startFirstTrial.SetActive(true);
                textExperimentNextPage.SetActive(false);
            }
        }

    }


    public void StartEyeCalibration()
    {
        ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
    }



    //public void StartQuestionnaire(int number)
    public void StartQuestionnaire()
    {
        /*activates the questionnaire (just makes it visible)
        vrQuestionnaire.Questionnaires[number].SetActive(true);

        //activate VivePointers
        vivePointers.SetActive(true);
        */

        //generate and start new mps questionnaire
        currentQuestionnaireName = vrQuestionnaire.GenerateNewQuestionnaire(mpsQuestionnairePath);
        vrQuestionnaire.StartQuestionnaire(currentQuestionnaireName);

        //write marker
        marker.Write("questionnaire:start;" +
                    "questionnaireName:" + currentQuestionnaireName);
        Debug.Log("questionnaire:start;" +
                    "questionnaireName:" + currentQuestionnaireName);

        //activate VivePointers
        vivePointers.SetActive(true);
        //Model.SetActive(true);

        //add listenener for questionnaire finish event:
        vrQuestionnaireExport.GetComponent<VRQuestionnaireToolkit.ExportToCSV>().QuestionnaireFinishedEvent.AddListener(QuestionnaireFinished);

        //update file name in csv export
        vrQuestionnaireExport.GetComponent<VRQuestionnaireToolkit.ExportToCSV>().FileName = currentQuestionnaireName;

        questionnaireStarted = true;

    }

    //public void StopQuestionnaire(int number)
    public void StopQuestionnaire()
    {
        /*deactivates the questionnaire (just makes it invisible)
        vrQuestionnaire.Questionnaires[number].SetActive(false);

        //deactivate VivePointers
        vivePointers.SetActive(false);
        */


        marker.Write("questionnaire:end");
        Debug.Log("questionnaire:end");

        //stop MPS questionnaire
        vrQuestionnaire.StopQuestionnaire(currentQuestionnaireName);

        //deactivate VivePointers
        vivePointers.SetActive(false);
        //Model.SetActive(false);

        //remove listenener for questionnaire finish event:
        vrQuestionnaireExport.GetComponent<VRQuestionnaireToolkit.ExportToCSV>().QuestionnaireFinishedEvent.RemoveListener(QuestionnaireFinished);


    }

    /*
    public void ResetQuestionnaire(int number)
    {
        //resets the questionnaire values to default
        vrQuestionnaire.ResetQuestionnaire(number);

    }*/

    public void QuestionnaireFinished()
    {
        questionnaireFinished = true;

        //deactivate VivePointers
        vivePointers.SetActive(false);
        //Model.SetActive(false);

    }




    // Update is called once per frame
    void Update ()
    {
        try
        {
            switch (expControlStatus)
            {
                case 0: //main menu
                    {
                        //check if all inputs in Configuration have been given and alle calibration was made
                        if (idSet && ageSet && genderSet && armLengthSet && cupPositionsSet && tablePosSet)
                        {
                            buttonExperiment.GetComponent<Button>().interactable = true;
                            //buttonLearning.GetComponent<Button>().interactable = true;
                            buttonTraining.GetComponent<Button>().interactable = true;
                            buttonBaselineClosed.GetComponent<Button>().interactable = true;
                            buttonBaselineOpen.GetComponent<Button>().interactable = true;
                            //buttonBaselineNew.GetComponent<Button>().interactable = true;
                            textMissingInputs.SetActive(false);
                        }
                        else
                        {
                            buttonExperiment.GetComponent<Button>().interactable = false;
                            //buttonLearning.GetComponent<Button>().interactable = false;
                            buttonTraining.GetComponent<Button>().interactable = false;
                            buttonBaselineClosed.GetComponent<Button>().interactable = false;
                            buttonBaselineOpen.GetComponent<Button>().interactable = false;
                            //buttonBaselineNew.GetComponent<Button>().interactable = false;
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
                            buttonCubePositions.GetComponent<Button>().interactable = true;

                            if(trackerFoundCupPos)
                                textHintCupPos.SetActive(false);
                            else
                                textHintCupPos.SetActive(true);                        
                        }
                        else
                        {
                            buttonCubePositions.GetComponent<Button>().interactable = false;
                            textHintCupPos.SetActive(true);
                        }
                        break;
                    }
                /*
                case 3: //learning (manual trial start and consecutive trials for each task task)
                    {
                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("learning:abort");
                            Debug.Log("learning:abort");

                            experimentStarted = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else if (learningStarted)
                            ControlTrial();
                        else
                            InitLearning(); // run only once after
                        break;
                    }*/
                case 4: //training (like experiment but shorter)
                    {
                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("training:abort");
                            Debug.Log("training:abort");

                            experimentStarted = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else if (trainingStarted)
                            ControlTrial();
                        else
                            InitTraining(); // run only once after
                        break;
                    }
                case 5: //experiment
                    {
                        //check for manual break by pressing the "p" key
                        if (Input.GetKeyDown("p"))
                        {
                            marker.Write("manual break button was pressed");
                            Debug.Log("manual break button was pressed");
                            manualBreakTriggered = true;
                        }

                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("experiment:abort");
                            Debug.Log("experiment:abort");

                            experimentStarted = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else if (flagStart)
                            ControlTrial();
                        else
                            InitExperiment(); // run only once
                        break;
                    }
                case 6: //break
                    {
                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("break:abort");
                            Debug.Log("break:abort");

                            experimentStarted = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else
                        {
                            breakDurationCountdown -= Time.deltaTime;
                            int breakDurationText = (int)breakDurationCountdown + 1;

                            //update break text
                            breakCanvasDesktop.GetComponentInChildren<Text>().text = "BREAK TIME:\n" + breakDurationText.ToString() + " seconds left";

                            //check break timer
                            if (breakDurationCountdown <= 0)
                            {
                                //stop break and continue with experiment
                                StopBreak();
                            }
                        }
                        
                        break;
                    }
                case 7: //baseline with eyes closed
                    {
                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("baselineClosed:abort");
                            Debug.Log("baselineClosed:abort");

                            //deactivate special canvas
                            baselineClosedCanvas.SetActive(false);

                            baselineRunning = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else if (baselineRunning)
                        {
                            actualTime += Time.deltaTime;

                            if (actualTime >= baselineDuration)
                            {
                                //end baseline
                                baselineRunning = false;

                                //display end text on table
                                endTextBox.GetComponent<Text>().text = endTextBaseline;
                                end.SetActive(true);

                                //deactivate special canvas
                                baselineClosedCanvas.SetActive(false);
                                marker.Write("Deactivated Black canvas");
                                Debug.Log("Deactivated Black canvas");

                                //lsl marker
                                marker.Write("baselineClosed:end");
                                Debug.Log("baselineClosed:end");

                                StartMainMenu();
                            }
                        }

                        break;
                    }
                case 8: //baseline with eyes open
                    {
                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("baselineOpen:abort");
                            Debug.Log("baselineOpen:abort");

                            baselineRunning = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else if (baselineRunning)
                        {
                            actualTime += Time.deltaTime;

                            if (actualTime >= baselineDuration)
                            {
                                //end baseline
                                baselineRunning = false;

                                //display end text on table
                                endTextBox.GetComponent<Text>().text = endTextBaseline;
                                end.SetActive(true);

                                //lsl marker
                                marker.Write("baselineOpen:end");
                                Debug.Log("baselineOpen:end");

                                StartMainMenu();
                            }
                        }

                        break;
                    }
                /*
                case 9: //baseline new
                    {
                        //check for abort by pressing the escape key
                        if (Input.GetKeyDown("escape"))
                        {
                            marker.Write("baselineNew:abort");
                            Debug.Log("baselineNew:abort");

                            baselineRunning = false;

                            //go to main menu
                            StartMainMenu();
                        }
                        else if (flagStart)
                            ControlTrial();
                        else
                            InitBaselineNew(); // run only once
                        break;

                    }*/
            }//switch
            
        }
        catch (System.Exception e)  //catch errors and log them and write them to lsl stream, then throw the exception again
        {
            marker.Write(e.ToString());
            Debug.LogError(e);
            throw (e);
        }

    }//update()


    private void FixedUpdate()
    {
        //check if hand is moving or not
        if (handMovementThresholdOn)
        {
            //check current velocity of the hand
            //calculate distance between old and current hand position
            try
            {
                if (vr_hand_R == null)
                {
                    vr_hand_R = GameObject.Find("vr_hand_R");
                    //Debug.Log("vr_hand_R is null");
                }

                if (handPosition_old != Vector3.zero)
                {
                    float distance = (Vector3.Distance(handPosition_old, vr_hand_R.transform.position));
                    //Debug.Log("distance: " + distance.ToString() + " threshold: " + handMovementThreshold.ToString());

                    if (distance > handMovementThreshold)
                    {
                        handIsMoving = true;
                        //Debug.Log("distance: " + distance.ToString() + " threshold: " + handMovementThreshold.ToString());
                    }
                    else
                    {
                        handIsMoving = false;
                    }
                }
                else
                {
                    handIsMoving = false;
                }

                //Debug.Log("handIsMoving: " + BoolToString(handIsMoving));

                //update old hand position
                handPosition_old = vr_hand_R.transform.position;
            }
            catch(System.NullReferenceException e)
            {
                Debug.Log("ERROR: could not get vr_hand_R.transform.position");
            }

        }

    }//FixedUpdate()

}