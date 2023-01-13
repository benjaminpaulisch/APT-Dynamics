using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;

/// <summary>
/// GenerateQuestionnaire.class
/// 
/// version 1.0
/// date: July 1st, 2020
/// authors: Martin Feick & Niko Kleer
/// </summary>

namespace VRQuestionnaireToolkit
{
    public class GenerateQuestionnaireBeMoBIL : MonoBehaviour
    {
        /*
        public string JsonInputPath_1;
        public string JsonInputPath_2;
        public string JsonInputPath_3;
        public string JsonInputPath_4;
        public string JsonInputPath_5;
        public string JsonInputPath_6;
        public string JsonInputPath_7;
        public string JsonInputPath_8;
        public string JsonInputPath_9;
        public string JsonInputPath_10;
        */

        private List<string> JsonInputFiles;
        public List<GameObject> Questionnaires; // list containing all questionnaires 

        private PageFactory _pageFactory;
        private ExportToCSV _exportToCsvScript;
        private GameObject _exportToCsv;
        public GameObject questionnaire;
        public RectTransform QuestionRecTest;
        public GameObject questionPosition;

        private JSONArray _qData;
        private JSONArray _qConditions;
        private JSONArray _qOptions;

        private GameObject currentQuestionnaire;
        private int numberQuestionnaires;
        private string qId;
        private string pId;

        private void FireEvent()
        {
            print("QuestionnaireFinishedEvent");
        }

        void Start()
        {
            _exportToCsv = GameObject.FindGameObjectWithTag("ExportToCSV");
            _exportToCsvScript = _exportToCsv.GetComponent<ExportToCSV>();
            _exportToCsvScript.QuestionnaireFinishedEvent.AddListener(FireEvent);

            //numberQuestionnaires = 1;
            numberQuestionnaires = 0;

            Questionnaires = new List<GameObject>();
            JsonInputFiles = new List<string>();

            /*
            if (JsonInputPath_1 != "")
            {
                JsonInputFiles.Add(JsonInputPath_1);
            }
            if (JsonInputPath_2 != "")
            {
                JsonInputFiles.Add(JsonInputPath_2);
            }
            if (JsonInputPath_3 != "")
            {
                JsonInputFiles.Add(JsonInputPath_3);
            }
            if (JsonInputPath_4 != "")
            {
                JsonInputFiles.Add(JsonInputPath_4);
            }
            if (JsonInputPath_5 != "")
            {
                JsonInputFiles.Add(JsonInputPath_5);
            }
            if (JsonInputPath_6 != "")
            {
                JsonInputFiles.Add(JsonInputPath_6);
            }
            if (JsonInputPath_7 != "")
            {
                JsonInputFiles.Add(JsonInputPath_7);
            }
            if (JsonInputPath_8 != "")
            {
                JsonInputFiles.Add(JsonInputPath_8);
            }
            if (JsonInputPath_9 != "")
            {
                JsonInputFiles.Add(JsonInputPath_9);
            }
            if (JsonInputPath_10 != "")
            {
                JsonInputFiles.Add(JsonInputPath_10);
            }


            foreach (string InputPath in JsonInputFiles)
                GenerateNewQuestionnaire(InputPath);

            for (int i = 1; i < Questionnaires.Count; i++)
                Questionnaires[i].SetActive(false);

            Questionnaires[0].SetActive(true);
            */

        }

        public string GenerateNewQuestionnaire(string inputPath)
        {
            //returns the internal questionnaire name for identification (needed for starting and stopping a questionnaire)

            /*
            if (numberQuestionnaires > 1)
                currentQuestionnaire.SetActive(false);
            */

            currentQuestionnaire = Instantiate(questionnaire);
            currentQuestionnaire.name = "Questionnaire_" + numberQuestionnaires;

            // Place in hierarchy 
            RectTransform radioGridRec = currentQuestionnaire.GetComponent<RectTransform>();
            radioGridRec.SetParent(QuestionRecTest);

            /*
            radioGridRec.localPosition = new Vector3(0, 0, 0);
            radioGridRec.localRotation = Quaternion.identity;
            //[BPA: changed to values to 1,1,1, because the values don't matter at all as they have to influence on the actual size at point of generation]
            //radioGridRec.localScale = new Vector3(radioGridRec.localScale.x * 0.01f, radioGridRec.localScale.y * 0.01f, radioGridRec.localScale.z * 0.01f);
            radioGridRec.localScale = new Vector3(1, 1, 1);
            */

            //set questionnaire transform according to reference object
            //currentQuestionnaire.transform.position = questionPosition.transform.position;
            currentQuestionnaire.transform.position = QuestionRecTest.transform.position;
            //currentQuestionnaire.transform.rotation = questionPosition.transform.rotation;
            currentQuestionnaire.transform.rotation = QuestionRecTest.transform.rotation;

            currentQuestionnaire.transform.localScale = QuestionRecTest.transform.localScale;
            //radioGridRec.localScale = questionPosition.transform.localScale;
            QuestionRecTest.localScale = new Vector3(QuestionRecTest.transform.localScale.x * QuestionRecTest.transform.localScale.x, QuestionRecTest.transform.localScale.y * QuestionRecTest.transform.localScale.y, QuestionRecTest.transform.localScale.z * QuestionRecTest.transform.localScale.z);


            //_pageFactory = this.GetComponentInChildren<PageFactory>();
            _pageFactory = currentQuestionnaire.GetComponentInChildren<PageFactory>();

            Questionnaires.Add(currentQuestionnaire);
            numberQuestionnaires++;

            ReadJson(inputPath);

            return currentQuestionnaire.name;
        }


        public GameObject GetCurrentQuestionnaire()
        {
            return currentQuestionnaire;
        }


        void ReadJson(string jsonPath)
        {
            // reads and parses .json input file
            string JSONString = File.ReadAllText(jsonPath);
            var N = JSON.Parse(JSONString);

            //----------- Read metadata from .JSON file ----------//
            string title = N["qTitle"].Value;
            string instructions = N["qInstructions"].Value;
            qId = N["qId"].Value; //read questionnaire ID

            // Generates the last page
            _pageFactory.GenerateAndDisplayFirstAndLastPage(true, instructions, title);

            int i = 0;

            /*
            Continuously reads data from the .json file 
            */
            while (true)
            {
                pId = N["questions"][i]["pId"].Value; //read new page

                if (pId != "")
                {
                    string qType = N["questions"][i]["qType"].Value;
                    string qInstructions = N["questions"][i]["qInstructions"].Value;

                    _qData = N["questions"][i]["qData"].AsArray;
                    if (_qData == "")
                        _qData[0] = N["questions"][i]["qData"].Value;

                    _qConditions = N["questions"][i]["qConditions"].AsArray;
                    if (_qConditions == "")
                        _qConditions[0] = N["questions"][i]["qConditions"].Value;

                    _qOptions = N["questions"][i]["qOptions"].AsArray;
                    if (_qOptions == "")
                        _qOptions[0] = N["questions"][i]["qOptions"].Value;

                    _pageFactory.AddPage(qId, qType, qInstructions, _qData, _qConditions, _qOptions);
                    i++;
                }
                else
                {
                    // Read data for final page from .JSON file
                    string headerFinalSlide = N["qMessage"].Value;
                    string textFinalSlide = N["qAcknowledgments"].Value;

                    // Generates the last page
                    _pageFactory.GenerateAndDisplayFirstAndLastPage(false, textFinalSlide, headerFinalSlide);

                    // Initialize (Dis-/enable GameObjects)
                    _pageFactory.InitSetup();

                    break;
                }
            }
        }

        /*
        //[BPA: added this method to reset a questionnaire]
        public void ResetQuestionnaire(int questionnaireNo)
        {
            GameObject tempQuestionnaire = Instantiate(questionnaire);
            tempQuestionnaire.name = "Questionnaire_" + numberQuestionnaires;

            // Place in hierarchy 
            RectTransform radioGridRec = tempQuestionnaire.GetComponent<RectTransform>();
            radioGridRec.SetParent(QuestionRecTest);


            radioGridRec.localPosition = new Vector3(0, 0, 0);
            radioGridRec.localRotation = Quaternion.identity;
            //[BPA: changed to values to 1,1,1, because the values don't matter at all as they have to influence on the actual size at point of generation]
            //radioGridRec.localScale = new Vector3(radioGridRec.localScale.x * 0.01f, radioGridRec.localScale.y * 0.01f, radioGridRec.localScale.z * 0.01f);
            radioGridRec.localScale = new Vector3(1, 1, 1);

            _pageFactory = this.GetComponentInChildren<PageFactory>();

            //Questionnaires.Add(tempQuestionnaire);
            Questionnaires[questionnaireNo] = tempQuestionnaire;
            //numberQuestionnaires++;

            //ReadJson(inputPath);
            ReadJson(JsonInputFiles[questionnaireNo]);

        }*/

        public void StartQuestionnaire(string questionName)
        {
            //activates the question with the name specified by the parameter questionName
            GameObject tempQuestionnaire = Questionnaires.Find(x => x.name == questionName);

            if (tempQuestionnaire != null)
            {
                tempQuestionnaire.SetActive(true);
            }
            else
            {
                Debug.LogError("Could not start unknown questionnaire '" + questionName + "'");
            }

        }


        public void StopQuestionnaire(string questionName)
        {
             //deactivates the question with the name specified by the parameter questionName
            GameObject tempQuestionnaire = Questionnaires.Find(x => x.name == questionName);

            if (tempQuestionnaire != null)
            {
                tempQuestionnaire.SetActive(false);
            }
            else
            {
                Debug.LogError("Could not stop unknown questionnaire '" + questionName + "'");
            }

        }
    }
}