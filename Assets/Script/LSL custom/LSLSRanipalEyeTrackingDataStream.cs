using UnityEngine;
using LSL;
using Assets.LSL4Unity.Scripts;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;

public class LSLSRanipalEyeTrackingDataStream : MonoBehaviour
{
    [Header("GazeBehaviorStream")]
    public string StreamNameEyedata = "GazeBehavior";
    public string StreamTypeEyeData = "Gaze";
    public string uidEyeData = "awe9btu8en5uilt7i";
    public double dataRateEyeData = 90;
    public MomentForSampling samplingEyeData;

    private int channelCountEyeData = 13;
    private liblsl.StreamOutlet outletEyeData;
    private liblsl.StreamInfo streamInfoEyeData;
    private liblsl.XMLElement objs, obj;
    private liblsl.XMLElement channelsEyeData, channelEyeData;
    private const liblsl.channel_format_t lslChannelFormatEyeData = liblsl.channel_format_t.cf_float32;

    private EyeData_v2 eyeTrackingData = new EyeData_v2();
    private bool eye_callback_registered;
    private float[] currentSample;
    private VerboseData verboseData;
    Vector3 originCorrected, directionCorrected;
    private float currentDataValidity;
    private float eyeOpennessLeft, eyeOpennessRight;


    [Header("FocusedObjectsEvents")]
    public GameObject markerStream;
    private LSLMarkerStream_FocusedObjs focusedObjectsStream;
    public bool drawRaycast = false;
    public float rayLength = 10.0f;                      //used for assigning a length to the raycast
    //public GUIControl guic;
    public GameObject VRCamera;
    private GameObject oldHitObject, currentHitObject = null;
    private RaycastHit raycastHit;                      //used for detecting raycast collision
    private int layerMask = 1 << 8;                     //only hit objects in this layer


    [Header("FocusedHitPointStream")]
    public string StreamNameHitPoint = "FocusedHitPoints";
    public string StreamTypeHitPoint = "Gaze";
    public string uidHitPoint = "awe9btu8en5uilt7i";
    public double dataRateHitPoint = 90;
    public MomentForSampling samplingHitPoint;

    private int channelCountHitPoint = 3;
    private float[] currentSampleHitPoint;
    private liblsl.StreamOutlet outletHitPoint;
    private liblsl.StreamInfo streamInfoHitPoint;
    private liblsl.XMLElement objsHitPoint, objHitPoint;
    private liblsl.XMLElement channelsHitPoint, channelHitPoint;
    private const liblsl.channel_format_t lslChannelFormatHitPoint = liblsl.channel_format_t.cf_float32;
    private Vector3 currentHitPoint;
    private Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;


    // Start is called before the first frame update
    void Start()
    {
        focusedObjectsStream = markerStream.GetComponent<LSLMarkerStream_FocusedObjs>();

        //##### Initialize EyeData LSL Stream #####
        currentSample = new float[channelCountEyeData];

        streamInfoEyeData = new liblsl.StreamInfo(StreamNameEyedata, StreamTypeEyeData, channelCountEyeData, dataRateEyeData, lslChannelFormatEyeData, uidEyeData);

        //##### Setup LSL stream metadata
        streamInfoEyeData.desc().append_child("synchronization").append_child_value("can_drop_samples", "true");

        //### GazeBehaviorStream:
        channelsEyeData = streamInfoEyeData.desc().append_child("channels");

        //Gaze origin
        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "GazeOrigin_X");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "PositionX");
        channelEyeData.append_child_value("unit", "m");
        channelEyeData.append_child_value("coordinate_system", "world-space");

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "GazeOrigin_Y");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "PositionY");
        channelEyeData.append_child_value("unit", "m");
        channelEyeData.append_child_value("coordinate_system", "world-space");

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "GazeOrigin_Z");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "PositionZ");
        channelEyeData.append_child_value("unit", "m");
        channelEyeData.append_child_value("coordinate_system", "world-space");

        //gaze direction
        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "GazeDirection_X");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "DirectionX");
        channelEyeData.append_child_value("unit", "m");
        channelEyeData.append_child_value("coordinate_system", "world-space");

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "GazeDirection_Y");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "DirectionY");
        channelEyeData.append_child_value("unit", "m");
        channelEyeData.append_child_value("coordinate_system", "world-space");

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "GazeDirection_Z");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "DirectionZ");
        channelEyeData.append_child_value("unit", "m");
        channelEyeData.append_child_value("coordinate_system", "world-space");

        //Convergence distance
        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "ConvergenceDistanceValidity");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "validity");              //?? type not defined
        channelEyeData.append_child_value("unit", "normalized");

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "ConvergenceDistance");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "distance");              //?? type not defined
        channelEyeData.append_child_value("unit", "mm");



        //Pupil diameter
        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "PupilDiameterLeft");
        channelEyeData.append_child_value("eye", "left");
        channelEyeData.append_child_value("type", "Diameter");                  //the overall pupil diameter
        channelEyeData.append_child_value("unit", "mm");                        //is it mm oder m ?

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "PupilDiameterRight");
        channelEyeData.append_child_value("eye", "right");
        channelEyeData.append_child_value("type", "Diameter");                  //the overall pupil diameter
        channelEyeData.append_child_value("unit", "mm");                        //is it mm oder m ?

        //Eye openness
        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "EyeOpennessLeft");
        channelEyeData.append_child_value("eye", "left");
        channelEyeData.append_child_value("type", "Openness");                  //??
        channelEyeData.append_child_value("unit", "normalized");                //The openness value of an eye, clamped between 0 (fully closed) and 1 (fully open)

        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "EyeOpennessRight");
        channelEyeData.append_child_value("eye", "right");
        channelEyeData.append_child_value("type", "Openness");                  //??
        channelEyeData.append_child_value("unit", "normalized");                //The openness value of an eye, clamped between 0 (fully closed) and 1 (fully open)

        //validity of the data
        channelEyeData = channelsEyeData.append_child("channel");
        channelEyeData.append_child_value("label", "DataValidity");
        channelEyeData.append_child_value("eye", "combined");
        channelEyeData.append_child_value("type", "Validity");                  //?? type not defined
        channelEyeData.append_child_value("unit", "normalized");

        outletEyeData = new liblsl.StreamOutlet(streamInfoEyeData);


        //### HitPointStream:
        streamInfoHitPoint = new liblsl.StreamInfo(StreamNameHitPoint, StreamTypeHitPoint, channelCountHitPoint, dataRateHitPoint, lslChannelFormatHitPoint, uidHitPoint);

        streamInfoHitPoint.desc().append_child("synchronization").append_child_value("can_drop_samples", "true");

        channelsHitPoint = streamInfoHitPoint.desc().append_child("channels");

        channelHitPoint = channelsHitPoint.append_child("channel");
        channelHitPoint.append_child_value("label", "HitPoint_X");
        channelHitPoint.append_child_value("eye", "combined");
        channelHitPoint.append_child_value("type", "IntersectionX");
        channelHitPoint.append_child_value("unit", "m");
        channelHitPoint.append_child_value("coordinate_system", "world-space");

        channelHitPoint = channelsHitPoint.append_child("channel");
        channelHitPoint.append_child_value("label", "HitPoint_Y");
        channelHitPoint.append_child_value("eye", "combined");
        channelHitPoint.append_child_value("type", "IntersectionY");
        channelHitPoint.append_child_value("unit", "m");
        channelHitPoint.append_child_value("coordinate_system", "world-space");

        channelHitPoint = channelsHitPoint.append_child("channel");
        channelHitPoint.append_child_value("label", "HitPoint_Z");
        channelHitPoint.append_child_value("eye", "combined");
        channelHitPoint.append_child_value("type", "IntersectionZ");
        channelHitPoint.append_child_value("unit", "m");
        channelHitPoint.append_child_value("coordinate_system", "world-space");

        outletHitPoint = new liblsl.StreamOutlet(streamInfoHitPoint);

    }

    private void pushSample()
    {
        if (outletEyeData == null /*|| guic.disableSRanipal*/)
            return;
        else
        {
            //get eyetracking data
            /*
            currentSample[0] = verboseData.left.eye_data_validata_bit_mask;
            currentSample[1] = verboseData.left.gaze_origin_mm.x;
            currentSample[2] = verboseData.left.gaze_origin_mm.y;
            currentSample[3] = verboseData.left.gaze_origin_mm.z;
            currentSample[4] = verboseData.left.gaze_direction_normalized.x;
            currentSample[5] = verboseData.left.gaze_direction_normalized.y;
            currentSample[6] = verboseData.left.gaze_direction_normalized.z;
            currentSample[7] = verboseData.left.pupil_diameter_mm;
            currentSample[8] = verboseData.left.eye_openness;
            currentSample[9] = verboseData.left.pupil_position_in_sensor_area.x;
            currentSample[10] = verboseData.left.pupil_position_in_sensor_area.y;
            currentSample[11] = verboseData.right.eye_data_validata_bit_mask;
            currentSample[12] = verboseData.right.gaze_origin_mm.x;
            currentSample[13] = verboseData.right.gaze_origin_mm.y;
            currentSample[14] = verboseData.right.gaze_origin_mm.z;
            currentSample[15] = verboseData.right.gaze_direction_normalized.x;
            currentSample[16] = verboseData.right.gaze_direction_normalized.y;
            currentSample[17] = verboseData.right.gaze_direction_normalized.z;
            currentSample[18] = verboseData.right.pupil_diameter_mm;
            currentSample[19] = verboseData.right.eye_openness;
            currentSample[20] = verboseData.right.pupil_position_in_sensor_area.x;
            currentSample[21] = verboseData.right.pupil_position_in_sensor_area.y;
            currentSample[22] = System.Convert.ToSingle(verboseData.combined.convergence_distance_validity);
            currentSample[23] = verboseData.combined.convergence_distance_mm;
            currentSample[24] = verboseData.combined.eye_data.eye_data_validata_bit_mask;
            currentSample[25] = verboseData.combined.eye_data.gaze_origin_mm.x;     //lieber von GazeRay nehmen!
            currentSample[26] = verboseData.combined.eye_data.gaze_origin_mm.y;     //lieber von GazeRay nehmen!
            currentSample[27] = verboseData.combined.eye_data.gaze_origin_mm.z;     //lieber von GazeRay nehmen!
            currentSample[28] = verboseData.combined.eye_data.gaze_direction_normalized.x;  //lieber von GazeRay nehmen!
            currentSample[29] = verboseData.combined.eye_data.gaze_direction_normalized.y;  //lieber von GazeRay nehmen!
            currentSample[30] = verboseData.combined.eye_data.gaze_direction_normalized.z;  //lieber von GazeRay nehmen!
            currentSample[31] = verboseData.combined.eye_data.pupil_diameter_mm;
            currentSample[32] = verboseData.combined.eye_data.eye_openness;
            currentSample[33] = verboseData.combined.eye_data.pupil_position_in_sensor_area.x;
            currentSample[34] = verboseData.combined.eye_data.pupil_position_in_sensor_area.y;
            */

            currentSample[0] = originCorrected.x;                                   //data from GazeRay
            currentSample[1] = originCorrected.y;                                   //data from GazeRay
            currentSample[2] = originCorrected.z;                                   //data from GazeRay
            currentSample[3] = directionCorrected.x;                                //data from GazeRay
            currentSample[4] = directionCorrected.y;                                //data from GazeRay
            currentSample[5] = directionCorrected.z;                                //data from GazeRay
            currentSample[6] = System.Convert.ToSingle(verboseData.combined.convergence_distance_validity);
            currentSample[7] = verboseData.combined.convergence_distance_mm;
            currentSample[8] = verboseData.left.pupil_diameter_mm;
            currentSample[9] = verboseData.right.pupil_diameter_mm;
            currentSample[10] = verboseData.left.eye_openness;                       //value is between 0 and 1
            currentSample[11] = verboseData.right.eye_openness;                      //value is between 0 and 1
            currentSample[12] = currentDataValidity;                                //validity of current sample: 0 if not valid, 1 if valid

            outletEyeData.push_sample(currentSample, liblsl.local_clock());

        }

    }


    private void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeTrackingData = eye_data;
    }



    void FixedUpdate()
    {
        ////##### Get focused object
        ////Debug.Log("gazeOrigin.x:" + (verboseData.combined.eye_data.gaze_origin_mm.x/1000).ToString() + " gazeOrigin.y:" + (verboseData.combined.eye_data.gaze_origin_mm.y/1000).ToString() + " gazeOrigin.z:" + (verboseData.combined.eye_data.gaze_origin_mm.z/1000).ToString());
        ////Debug.Log("originCorrected: " + (VRCamera.transform.position.x + -verboseData.combined.eye_data.gaze_origin_mm.x / 1000).ToString() + " " + (VRCamera.transform.position.x + verboseData.combined.eye_data.gaze_origin_mm.y / 1000).ToString() + " " + (VRCamera.transform.position.x + verboseData.combined.eye_data.gaze_origin_mm.z / 1000).ToString());
        ////originCorrected = new Vector3(VRCamera.transform.position.x + -verboseData.combined.eye_data.gaze_origin_mm.x / 1000, VRCamera.transform.position.x + -verboseData.combined.eye_data.gaze_origin_mm.y / 1000, VRCamera.transform.position.x + verboseData.combined.eye_data.gaze_origin_mm.z / 1000);
        //originCorrected = VRCamera.transform.position + GazeOriginCombinedLocal;

        ////convert direction from right-handed to left-handed coordinate system
        ////directionCorrected = new Vector3(VRCamera.transform.rotation.eulerAngles.x * -verboseData.combined.eye_data.gaze_direction_normalized.x, VRCamera.transform.rotation.eulerAngles.y * verboseData.combined.eye_data.gaze_direction_normalized.z, VRCamera.transform.rotation.eulerAngles.z * verboseData.combined.eye_data.gaze_direction_normalized.y);
        ////directionCorrected = new Vector3(VRCamera.transform.up.x * -verboseData.combined.eye_data.gaze_direction_normalized.x, VRCamera.transform.forward.y * -verboseData.combined.eye_data.gaze_direction_normalized.y, VRCamera.transform.forward.z + verboseData.combined.eye_data.gaze_direction_normalized.z);
        ////directionCorrected = VRCamera.transform.TransformDirection(verboseData.combined.eye_data.gaze_direction_normalized.x, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);
        //directionCorrected = VRCamera.transform.TransformDirection(GazeDirectionCombinedLocal);

        ////This statement is called when the raycast is hitting a collider in the scene
        //if (Physics.Raycast(originCorrected, directionCorrected, out raycastHit, rayLength))
        //{
        //    //only when raycast is set to active
        //    if (drawRaycast)
        //    {
        //        //This will constantly draw the ray in our scene view so we can see where the ray is going
        //        Debug.DrawRay(originCorrected, directionCorrected * raycastHit.distance, Color.blue, 0.1f);
        //    }

        //    currentHitObject = raycastHit.collider.gameObject;

        //    if (currentHitObject != oldHitObject)
        //    {
        //        if (oldHitObject != null)
        //        {
        //            //old focus:out;object
        //            //Debug.Log("focus:out;object: " + oldHitObject.name);
        //            focusedObjectsStream.Write("focus:out;object: " + oldHitObject.name);
        //        }
                
        //        //new focused object
        //        //Debug.Log("focus:in;object: " + currentHitObject.name);
        //        focusedObjectsStream.Write("focus:in;object: " + currentHitObject.name);

        //    }

        //    //get current Hit Point from raycast:
        //    currentHitPoint = raycastHit.point;

        //}
        //else
        //{
        //    currentHitObject = null;

        //    if (oldHitObject != null)
        //    {
        //        //current focus:out;object
        //        //Debug.Log("focus:out;object: " + oldHitObject.name);
        //        focusedObjectsStream.Write("focus:out;object: " + oldHitObject.name);
        //    }

        //    //set current Hit Point to zero (because no hit from raycast):
        //    //currentHitPoint = Vector3.zero;
        //    //set current Hit Point to NaN (because no hit from raycast):
        //    currentHitPoint = new Vector3(float.NaN, float.NaN, float.NaN);

        //}

        ////update old hit object
        //oldHitObject = currentHitObject;


        ////update Hit Point sample:
        //currentSampleHitPoint = new float[] { currentHitPoint.x, currentHitPoint.y, currentHitPoint.z };


        ////check MomentForSampling
        //if (samplingEyeData == MomentForSampling.FixedUpdate)
        //    pushSample();

        //if (samplingHitPoint == MomentForSampling.FixedUpdate)
        //    outletHitPoint.push_sample(currentSampleHitPoint, liblsl.local_clock());

    }

    void Update()
    {
        //Get eye data:
        if (SRanipal_Eye_v2.GetVerboseData(out verboseData)) { }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal))
        {
            currentDataValidity = 1;
        }
        else
        {
            currentDataValidity = 0;
        }

        //if (SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.LEFT, out eyeOpennessLeft)) { }

        //if (SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.RIGHT, out eyeOpennessRight)) { }

        //##### Get focused object
        //Debug.Log("gazeOrigin.x:" + (verboseData.combined.eye_data.gaze_origin_mm.x/1000).ToString() + " gazeOrigin.y:" + (verboseData.combined.eye_data.gaze_origin_mm.y/1000).ToString() + " gazeOrigin.z:" + (verboseData.combined.eye_data.gaze_origin_mm.z/1000).ToString());
        //Debug.Log("originCorrected: " + (VRCamera.transform.position.x + -verboseData.combined.eye_data.gaze_origin_mm.x / 1000).ToString() + " " + (VRCamera.transform.position.x + verboseData.combined.eye_data.gaze_origin_mm.y / 1000).ToString() + " " + (VRCamera.transform.position.x + verboseData.combined.eye_data.gaze_origin_mm.z / 1000).ToString());
        //originCorrected = new Vector3(VRCamera.transform.position.x + -verboseData.combined.eye_data.gaze_origin_mm.x / 1000, VRCamera.transform.position.x + -verboseData.combined.eye_data.gaze_origin_mm.y / 1000, VRCamera.transform.position.x + verboseData.combined.eye_data.gaze_origin_mm.z / 1000);
        originCorrected = VRCamera.transform.position + GazeOriginCombinedLocal;

        //convert direction from right-handed to left-handed coordinate system
        //directionCorrected = new Vector3(VRCamera.transform.rotation.eulerAngles.x * -verboseData.combined.eye_data.gaze_direction_normalized.x, VRCamera.transform.rotation.eulerAngles.y * verboseData.combined.eye_data.gaze_direction_normalized.z, VRCamera.transform.rotation.eulerAngles.z * verboseData.combined.eye_data.gaze_direction_normalized.y);
        //directionCorrected = new Vector3(VRCamera.transform.up.x * -verboseData.combined.eye_data.gaze_direction_normalized.x, VRCamera.transform.forward.y * -verboseData.combined.eye_data.gaze_direction_normalized.y, VRCamera.transform.forward.z + verboseData.combined.eye_data.gaze_direction_normalized.z);
        //directionCorrected = VRCamera.transform.TransformDirection(verboseData.combined.eye_data.gaze_direction_normalized.x, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);
        directionCorrected = VRCamera.transform.TransformDirection(GazeDirectionCombinedLocal);

        //This statement is called when the raycast is hitting a collider in the scene
        if (Physics.Raycast(originCorrected, directionCorrected, out raycastHit, rayLength, layerMask))     //only hit objects in the specified layer
        {
            //only when raycast is set to active
            if (drawRaycast)
            {
                //This will constantly draw the ray in our scene view so we can see where the ray is going
                Debug.DrawRay(originCorrected, directionCorrected * raycastHit.distance, Color.blue, 0.1f);
            }

            currentHitObject = raycastHit.collider.gameObject;

            if (currentHitObject != oldHitObject)
            {
                if (oldHitObject != null)
                {
                    //old focus:out;object
                    Debug.Log("focus:out;object: " + oldHitObject.name);
                    focusedObjectsStream.Write("focus:out;object: " + oldHitObject.name);
                }

                //new focused object
                Debug.Log("focus:in;object: " + currentHitObject.name);
                focusedObjectsStream.Write("focus:in;object: " + currentHitObject.name);

            }

            //get current Hit Point from raycast:
            currentHitPoint = raycastHit.point;

        }
        else
        {
            currentHitObject = null;

            if (oldHitObject != null)
            {
                //current focus:out;object
                Debug.Log("focus:out;object: " + oldHitObject.name);
                focusedObjectsStream.Write("focus:out;object: " + oldHitObject.name);
            }

            //set current Hit Point to zero (because no hit from raycast):
            //currentHitPoint = Vector3.zero;
            //set current Hit Point to NaN (because no hit from raycast):
            currentHitPoint = new Vector3(float.NaN, float.NaN, float.NaN);

        }

        //update old hit object
        oldHitObject = currentHitObject;


        //update Hit Point sample:
        currentSampleHitPoint = new float[] { currentHitPoint.x, currentHitPoint.y, currentHitPoint.z };


        //check MomentForSampling
        if (samplingEyeData == MomentForSampling.Update)
            pushSample();

        if (samplingHitPoint == MomentForSampling.Update)
            outletHitPoint.push_sample(currentSampleHitPoint, liblsl.local_clock());
    }

    void LateUpdate()
    {
        //check MomentForSampling
        if (samplingEyeData == MomentForSampling.LateUpdate)
            pushSample();

        if (samplingHitPoint == MomentForSampling.LateUpdate)
            outletHitPoint.push_sample(currentSampleHitPoint, liblsl.local_clock());
    }

}
