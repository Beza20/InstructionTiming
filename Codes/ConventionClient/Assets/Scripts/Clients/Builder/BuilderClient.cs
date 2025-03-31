using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;
using silab.conventions.parameters;
using silab.conventions.utils;

namespace silab.conventions.clients {

    public class BuilderClient : CommunicationBase {

        [SerializeField] RecordPlayer record_player;

        float t;
        [SerializeField] float interval;
        MetaQuestHandsFrames hands_frames;
        bool is_hand_ready;

        [SerializeField] BuilderClientAudio audio_client;

        public bool IsHandReady {
            get => is_hand_ready;
            set => is_hand_ready = value;
        }

        void Start() {
            hands_frames = new MetaQuestHandsFrames("instructor");
        }

        void Update() {
            t += Time.deltaTime;
            if (t > interval) {
                t = 0;
                if (!is_hand_ready) {
                    GetData();
                } else {
                    if (!audio_client.IsReady && !audio_client.IsLoading) {
                        audio_client.GetAudioRequest();
                    }
                }
            }
        }

        public MetaQuestHandsFrames HandsFrames => hands_frames;

        public void ResetHandsFrames() {
            hands_frames = new MetaQuestHandsFrames("instructor");
        }
        
        public override void GetResultsFromServer(string parameter_json) {
            MetaQuestHandsFrames result_frames = JsonUtility.FromJson<MetaQuestHandsFrames>(parameter_json);
            if (result_frames.hands_list.Count == 0) {
                Debug.Log("No hands data received");
                is_hand_ready = hands_frames.hands_list.Count > 0;
            } else {
                hands_frames = result_frames;
                MetaQuestHands hands = hands_frames.hands_list[0];
                Debug.Log("Right forearm stub: " + hands.right.forearm_stub);
                Debug.Log("Left forearm stub: " + hands.left.forearm_stub);
            }
        }
    }
}
