using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using silab.conventions.communication;

namespace silab.conventions.ui {

    public class RecordStartButton : MetaQuestButtonBase {

        bool is_recording = false;
        [SerializeField] StartRecording start_recording;
        [SerializeField] StopRecording stop_recording;
        [SerializeField] TextMeshProUGUI text;

        
        new void Start() {
            base.Start();
        }

        public override void UnselectButtonFunc() {
            if (is_recording) {
                is_recording = false;
                stop_recording.GetData();
                text.text = "Start recording";
                Debug.Log("Record Stop Button selected");
            } else {
                is_recording = true;
                start_recording.GetData();
                text.text = "Stop recording";
                Debug.Log("Record Start Button selected");
            }
        }
    }
}
