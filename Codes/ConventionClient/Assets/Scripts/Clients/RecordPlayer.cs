using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using silab.conventions.parameters;
using silab.conventions.ui;

namespace silab.conventions.clients {

    public class RecordPlayer : MonoBehaviour {

        [SerializeField] BuilderClient builder_client;
        [SerializeField] BuilderClientAudio audio_client;
        [SerializeField] InstructionVisualizer instruction_visualizer;

        bool is_playing = false;
        [SerializeField] GameObject recording_indicator;
        float record_start_time, record_end_time;
        float current_time;
        [SerializeField] TextMeshProUGUI record_time_text;
        int frame_count = 0;
        [SerializeField] RecordEnableButton record_enable_button;

        public bool IsPlaying => is_playing;

        void Update() {
            if (builder_client.HandsFrames.hands_list.Count > 0 && audio_client.IsReady) {
                if (record_start_time == 0 && record_end_time == 0) {
                    record_start_time = builder_client.HandsFrames.hands_list[0].start;
                    record_end_time = builder_client.HandsFrames.hands_list[builder_client.HandsFrames.hands_list.Count - 1].end;
                    current_time = record_start_time;
                    frame_count = 0;
                    // UpdateHandVisualization();
                }
                if (record_enable_button.IsEnable) {
                    UpdateHandVisualization();
                }
            }

            if (is_playing) {
                GoForwardRecord(Time.deltaTime);
                // UpdateHandVisualization();
            }

            recording_indicator.SetActive(is_playing);
            UpdateRecordTimeText();

            if (frame_count >= builder_client.HandsFrames.hands_list.Count - 1) {
                is_playing = false;
            }
        }

        void UpdateHandVisualization() {
            MetaQuestHands hands = builder_client.HandsFrames.hands_list[frame_count];
            MetaQuestHand hand_left = hands.left;
            MetaQuestHand hand_right = hands.right;
            instruction_visualizer.VisualizeFinger(hand_left, "left");
            instruction_visualizer.VisualizeFinger(hand_right, "right");
        }

        public float GetRelativeTime() {
            if (record_start_time == 0) {
                return 0;
            } else if (record_end_time == 0) {
                return 0;
            } else {
                return (current_time - record_start_time) / (record_end_time - record_start_time);
            }
        }

        public void SetRelativeTime(float new_relative_time) {
            if (record_start_time == 0 || record_end_time == 0) {
                return;
            }
            float old_relative_time = GetRelativeTime();
            current_time = record_start_time + new_relative_time * (record_end_time - record_start_time);
            current_time = Mathf.Clamp(current_time, record_start_time, record_end_time);
            float frame_start_time = builder_client.HandsFrames.hands_list[frame_count].start;
            if (old_relative_time < new_relative_time) {
                while (frame_start_time < current_time && frame_count < builder_client.HandsFrames.hands_list.Count - 1) {
                    frame_count++;
                    frame_start_time = builder_client.HandsFrames.hands_list[frame_count].start;
                }
            } else {
                while (frame_start_time > current_time && frame_count > 0) {
                    frame_count--;
                    frame_start_time = builder_client.HandsFrames.hands_list[frame_count].start;
                }
            }
        }
        
        public void PlayRecord() {
            if (!builder_client.IsHandReady && !is_playing && builder_client.HandsFrames.hands_list.Count == 0) {
                return;
            }
            if (frame_count >= builder_client.HandsFrames.hands_list.Count - 1) {
                current_time = record_start_time;
                frame_count = 0;
            }
            EnableVisualization(true);
            is_playing = true;
            audio_client.SetTime(Mathf.Max(0, current_time - record_start_time));
            audio_client.PlayAudio();
        }

        public void PauseRecord() {
            if (!is_playing) {
                return;
            }
            audio_client.PauseAudio();
            is_playing = false;
        }

        public void GoForwardRecord(float time) {
            if (!builder_client.IsHandReady && !is_playing && builder_client.HandsFrames.hands_list.Count == 0) {
                return;
            }
            current_time += time;
            current_time = Mathf.Clamp(current_time, record_start_time, record_end_time);
            float frame_start_time = builder_client.HandsFrames.hands_list[frame_count].start;
            if (time > 0) {
                while (frame_start_time < current_time && frame_count < builder_client.HandsFrames.hands_list.Count - 1) {
                    frame_count++;
                    frame_start_time = builder_client.HandsFrames.hands_list[frame_count].start;
                }
            } else {
                while (frame_start_time > current_time && frame_count > 0) {
                    frame_count--;
                    frame_start_time = builder_client.HandsFrames.hands_list[frame_count].start;
                }
            }
        }

        public void ResetRecord() {
            current_time = 0;
            frame_count = 0;
            record_start_time = 0;
            record_end_time = 0;
            builder_client.IsHandReady = false;
            audio_client.FinishAudio();
            is_playing = false;
            EnableVisualization(false);
            builder_client.ResetHandsFrames();
        }

        void UpdateRecordTimeText() {
            if (record_start_time == 0 || record_end_time == 0) {
                record_time_text.text = "00:00 / 00:00";
            } else {
                int relative_current_time = (int)(current_time - record_start_time);
                int current_minute = relative_current_time / 60;
                int current_second = relative_current_time % 60;
                int relative_end_time = (int)(record_end_time - record_start_time);
                int end_minute = relative_end_time / 60;
                int end_second = relative_end_time % 60;
                record_time_text.text = current_minute + ":" + current_second.ToString("D2") + " / " + end_minute + ":" + end_second.ToString("D2");
            }
        }

        public void EnableVisualization(bool enable) {
            if (!builder_client.IsHandReady && !is_playing && builder_client.HandsFrames.hands_list.Count == 0) {
                return;
            }
            instruction_visualizer.EnableInstructionVisualization(enable);
            if (is_playing && builder_client.IsHandReady && builder_client.HandsFrames.hands_list.Count > 0) {
                PauseRecord();
            }
        }
    }
}
