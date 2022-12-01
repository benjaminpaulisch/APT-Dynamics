using UnityEngine;
using System.Collections;
using LSL;

namespace Assets.LSL4Unity.Scripts
{
    [HelpURL("https://github.com/xfleckx/LSL4Unity/wiki#using-a-marker-stream")]
    public class LSLMarkerStream_FocusedObjs : MonoBehaviour
    {
        public string lslStreamName = "CPS1_FocusedObjectEvents";
        private string lslStreamType = "Markers";
        public string unique_source_id = "bdsogup0tbnaepu";
        private liblsl.channel_format_t lslChannelFormat = liblsl.channel_format_t.cf_string;

        private int lslChannelCount = 1;
        private liblsl.StreamInfo lslStreamInfo;
        private liblsl.StreamOutlet lslOutlet;

        //Assuming that markers are never send in regular intervalls
        private double nominal_srate = liblsl.IRREGULAR_RATE;        

        private string[] sample;
 
        void Awake()
        {
            sample = new string[lslChannelCount];

            lslStreamInfo = new liblsl.StreamInfo(
                                        lslStreamName,
                                        lslStreamType,
                                        lslChannelCount,
                                        nominal_srate,
                                        lslChannelFormat,
                                        unique_source_id);
            
            lslOutlet = new liblsl.StreamOutlet(lslStreamInfo);
        }

        public void Write(string marker)
        {
            sample[0] = marker;
            lslOutlet.push_sample(sample);
        }

        public void Write(string marker, double customTimeStamp)
        {
            sample[0] = marker;
            lslOutlet.push_sample(sample, customTimeStamp);
        }

        public void Write(string marker, float customTimeStamp)
        {
            sample[0] = marker;
            lslOutlet.push_sample(sample, customTimeStamp);
        }

        public void WriteBeforeFrameIsDisplayed(string marker)
        {
            StartCoroutine(WriteMarkerAfterImageIsRendered(marker));
        }

        IEnumerator WriteMarkerAfterImageIsRendered(string pendingMarker)
        {
            yield return new WaitForEndOfFrame();

            Write(pendingMarker);

            yield return null;
        }

    }
}