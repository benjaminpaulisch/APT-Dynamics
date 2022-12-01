using UnityEngine;
using System.Collections;
using LSL;
using Assets.LSL4Unity.Scripts;
using Assets.LSL4Unity.Scripts.Common;

public class lsl_rb_stream_object: MonoBehaviour 
{
    public enum MocapObjectClass { Rigid, Bone, Skeleton, MarkerSet }

    //unity editor interface:
    public string StreamName = "";
	public string StreamType = "MoCap";
    public MocapObjectClass mocapObjectClass;
    public double dataRate=90;
	public string uid = "21da06a47ac14a1a84d56d629d0486e5";
	public MomentForSampling sampling;
	public GameObject sampleSource;

	private int ChannelCount = 7;	// 3 Pos. (x,y,z) + 4 Rot (x,y,z,w)  
	private liblsl.StreamOutlet outlet;
	private liblsl.StreamInfo streamInfo;
	private liblsl.XMLElement objs, obj;
	private liblsl.XMLElement channels, chan;
	private float[] currentSample;



	public liblsl.StreamInfo GetStreamInfo()
	{
		return streamInfo; 
	}
	

	public double GetDataRate()
	{
		return dataRate;
	}

	public void SetDataRate(double rate )
	{
		dataRate=rate;
	}


	public bool HasConsumer()
	{
		if (outlet != null)
			return outlet.have_consumers();

		return false;
	}


	// Use this for initialization
	void Start () 
	{

		if (sampleSource != null)
		{
			//if no StreamName specified, we take the gameObject's name
			if (StreamName == "") {
				StreamName = sampleSource.name;
            }
			
			// initialize the array once
			currentSample = new float[ChannelCount];

			//dataRate = LSLUtils.GetSamplingRateFor(sampling);

			streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, dataRate, liblsl.channel_format_t.cf_float32, uid);

			//setup LSL stream metadata (code from vizard) 
			streamInfo.desc().append_child("synchronization").append_child_value("can_drop_samples", "true");

			var setup = streamInfo.desc().append_child("setup");
			setup.append_child_value("name", StreamName);
			// channels with position and orientation in quaternions
			objs = setup.append_child("objects");
			obj = objs.append_child("object");
			obj.append_child_value("label", sampleSource.name);
			obj.append_child_value("class", mocapObjectClass.ToString());

			channels = streamInfo.desc().append_child("channels");

			chan = channels.append_child("channel");
			chan.append_child_value("label", "Position_X");
			chan.append_child_value("object", sampleSource.name);
			chan.append_child_value("type", "PositionX");
			chan.append_child_value("unit", "m");

            chan = channels.append_child("channel");
            chan.append_child_value("label", "Position_Y");
            chan.append_child_value("object", sampleSource.name);
            chan.append_child_value("type", "PositionY");
            chan.append_child_value("unit", "m");

            chan = channels.append_child("channel");
            chan.append_child_value("label", "Position_Z");
            chan.append_child_value("object", sampleSource.name);
            chan.append_child_value("type", "PositionZ");
            chan.append_child_value("unit", "m");

            chan = channels.append_child("channel");
			chan.append_child_value("label", "Quaternion_X");
			chan.append_child_value("object", sampleSource.name);
			chan.append_child_value("type", "OrientationX");
			chan.append_child_value("unit", "rad");

			chan = channels.append_child("channel");
			chan.append_child_value("label", "Quaternion_Y");
			chan.append_child_value("object", sampleSource.name);
			chan.append_child_value("type", "OrientationY");
			chan.append_child_value("unit", "rad");

			chan = channels.append_child("channel");
			chan.append_child_value("label", "Quaternion_Z");
			chan.append_child_value("object", sampleSource.name);
			chan.append_child_value("type", "OrientationZ");
			chan.append_child_value("unit", "rad");

			chan = channels.append_child("channel");
			chan.append_child_value("label", "Quaternion_W");
			chan.append_child_value("object", sampleSource.name);
			chan.append_child_value("type", "OrientationW");
			chan.append_child_value("unit", "rad");

			outlet = new liblsl.StreamOutlet(streamInfo);
			
		}
	}

	private void pushSample()
	{
		if (outlet == null)
			return;
		else
		{
			/* if (Vector3.Magnitude(firstDevice.velocity) > 1)
					Debug.Log("Position:" +firstDevice.transform.pos);
				if (Vector3.Magnitude(firstDevice.angularVelocity) > 1)
					Debug.Log("Rotation"+firstDevice.transform.rot);*/
			// reuse the array for each sample to reduce allocation costs
			// currently only for right-hand device
			currentSample[0] = sampleSource.transform.position.x;
			currentSample[1] = sampleSource.transform.position.y;
			currentSample[2] = sampleSource.transform.position.z;
			currentSample[3] = sampleSource.transform.rotation.x;
			currentSample[4] = sampleSource.transform.rotation.y;
			currentSample[5] = sampleSource.transform.rotation.z;
			currentSample[6] = sampleSource.transform.rotation.w;

			outlet.push_sample(currentSample, liblsl.local_clock());

			//Debug.Log(currentSample[0]+","+currentSample[1]+","+currentSample[2]);
		}
	}

	void FixedUpdate()
	{
		if (sampling == MomentForSampling.FixedUpdate)
			pushSample();
	}

	void Update()
	{
		if (sampling == MomentForSampling.Update)
			pushSample();
	}

	void LateUpdate()
	{
		if (sampling == MomentForSampling.LateUpdate)
			pushSample();
	}
}