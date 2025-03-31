using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;

namespace silab.conventions.clients {

    public class BuilderClientAudio : CommunicationBase {

        [SerializeField] float interval;
        float t;
        AudioSource audio_source;
        bool is_loading = false;
        bool is_ready = false;
        
        void Start() {
            audio_source = GetComponent<AudioSource>();
        }

        public bool IsLoading => is_loading;

        public bool IsReady {
            get => is_ready;
            set => is_ready = value;
        }

        public void PlayAudio() {
            Debug.Log("Playing audio with BuilderClientAudio");
            audio_source.Play();
        }

        public void PauseAudio() {
            Debug.Log("Pausing audio with BuilderClientAudio");
            audio_source.Pause();
        }

        public void SetTime(float time) {
            audio_source.time = time;
        }

        public void FinishAudio() {
            Debug.Log("Stoping audio with BuilderClientAudio");
            audio_source.Stop();
            audio_source.clip = null;
            is_ready = false;
            System.GC.Collect();
        }

        public void GetAudioRequest() {
            is_loading = true;
            GetAudioData();
        }

        public override void GetAudioRequestsFromServer(AudioClip audio_clip) {
            audio_source.clip = audio_clip;
            is_loading = false;
            is_ready = true;
            Debug.Log("Audio received");
        }
    }
}
